#include "BrainIO.h"

BrainIO::BrainIO(uint8_t analogPin)
  : _pin(analogPin),
    _rawValue(ADC_MIDPOINT),
    _baseline(ADC_MIDPOINT),
    _amplitude(0),
    _crossings(0),
    _lastSecondMs(0),
    _dominantHz(0),
    _lastSampleMs(0)
{}

// ─────────────────────────────────────────────────────────────────────────────
void BrainIO::begin() {
  // Seed baseline with first real reading so it doesn't start from 0
  _baseline     = analogRead(_pin);
  _lastSampleMs = millis();
  _lastSecondMs = millis();
}

// ─────────────────────────────────────────────────────────────────────────────
int BrainIO::update() {
  // Throttle to SAMPLE_RATE_HZ — don't read faster than needed
  if (millis() - _lastSampleMs < SAMPLE_INTERVAL_MS) return _rawValue;
  _lastSampleMs = millis();

  int prev     = _rawValue;
  _rawValue    = analogRead(_pin);

  // Track amplitude as a smoothed deviation from the DC baseline
  int deviation = abs(_rawValue - _baseline);
  _amplitude    = (_amplitude * 7 + deviation) / 8;  // exponential smoothing

  // ── Zero-crossing detection ───────────────────────────────────────────────
  // When the signal flips from above-baseline to below (or vice versa), count it.
  // Two crossings = one full wave cycle → dominant freq = crossings / 2 per second.
  bool prevAbove = (prev      > _baseline);
  bool nowAbove  = (_rawValue > _baseline);
  if (prevAbove != nowAbove) _crossings++;

  // Once per second, snapshot dominantHz and reset counter
  if (millis() - _lastSecondMs >= 1000) {
    _dominantHz   = _crossings / 2.0;
    _crossings    = 0;
    _lastSecondMs = millis();
  }

  // Keep baseline tracking the slow DC level (EMA, alpha ≈ 0.01)
  _updateBaseline(_rawValue);

  return _rawValue;
}

// ─────────────────────────────────────────────────────────────────────────────
void BrainIO::_updateBaseline(int sample) {
  // Very slow low-pass: baseline follows DC drift, ignores 7–31 Hz EEG signal
  _baseline = (_baseline * 99 + sample) / 100;
}

// ─────────────────────────────────────────────────────────────────────────────
int BrainIO::getRawValue()  { return _rawValue; }
int BrainIO::getCentered()  { return _rawValue - _baseline; }
int BrainIO::getAmplitude() { return _amplitude; }

float BrainIO::getDominantHz() { return _dominantHz; }

// Alpha: 8–12 Hz — what this circuit is primarily designed to detect
bool BrainIO::isAlpha() {
  return (_dominantHz >= BAND_ALPHA_LOW_HZ && _dominantHz < BAND_ALPHA_HIGH_HZ);
}

// Beta: 12–30 Hz — alert / concentrating state
bool BrainIO::isBeta() {
  return (_dominantHz >= BAND_BETA_LOW_HZ && _dominantHz < BAND_BETA_HIGH_HZ);
}

// ─────────────────────────────────────────────────────────────────────────────
bool BrainIO::isSignalPresent() {
  // The circuit's HPF at 7 Hz means a flat line here = bad electrode contact
  return (_amplitude > NOISE_FLOOR);
}

bool BrainIO::isClipping() {
  // If swing exceeds ±CLIP_THRESHOLD from midpoint the ADC is hitting its limits.
  // Fix: turn the Stage 5 potentiometer to reduce gain.
  return (abs(_rawValue - ADC_MIDPOINT) > CLIP_THRESHOLD);
}

// ─────────────────────────────────────────────────────────────────────────────
void BrainIO::printDiagnostics() {
  // One compact line — readable at 115200 baud in Serial Monitor

  Serial.print("RAW:");    Serial.print(_rawValue);
  Serial.print("  CTR:");  Serial.print(getCentered());   // + or - from baseline
  Serial.print("  AMP:");  Serial.print(_amplitude);
  Serial.print("  Hz~");   Serial.print(_dominantHz, 1);
  Serial.print("  Band:");

  if      (isAlpha()) Serial.print("ALPHA");
  else if (isBeta())  Serial.print("BETA ");
  else                Serial.print("--   ");

  // Flags — these tell you what's wrong
  if (!isSignalPresent()) Serial.print("  [NO SIGNAL — check electrodes]");
  if (isClipping())       Serial.print("  [CLIPPING — turn pot down]");
  if (isSignalPresent() && !isClipping()) Serial.print("  [OK]");

  Serial.println();
}
