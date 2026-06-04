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

void BrainIO::begin() {
  _baseline     = analogRead(_pin);
  _lastSampleMs = millis();
  _lastSecondMs = millis();
}

int BrainIO::update() {
  if (millis() - _lastSampleMs < SAMPLE_INTERVAL_MS) return _rawValue;
  _lastSampleMs = millis();

  int prev     = _rawValue;
  _rawValue    = analogRead(_pin);

  int deviation = abs(_rawValue - _baseline);
  _amplitude    = (_amplitude * 7 + deviation) / 8;

  bool prevAbove = (prev      > _baseline);
  bool nowAbove  = (_rawValue > _baseline);
  if (prevAbove != nowAbove) _crossings++;

  if (millis() - _lastSecondMs >= 1000) {
    _dominantHz   = _crossings / 2.0;
    _crossings    = 0;
    _lastSecondMs = millis();
  }

  _updateBaseline(_rawValue);
  return _rawValue;
}

void BrainIO::_updateBaseline(int sample) {
  _baseline = (_baseline * 99 + sample) / 100;
}

int BrainIO::getRawValue()  { return _rawValue; }
int BrainIO::getCentered()  { return _rawValue - _baseline; }
int BrainIO::getAmplitude() { return _amplitude; }
float BrainIO::getDominantHz() { return _dominantHz; }

bool BrainIO::isAlpha() {
  return (_dominantHz >= BAND_ALPHA_LOW_HZ && _dominantHz < BAND_ALPHA_HIGH_HZ);
}

bool BrainIO::isBeta() {
  return (_dominantHz >= BAND_BETA_LOW_HZ && _dominantHz < BAND_BETA_HIGH_HZ);
}

bool BrainIO::isSignalPresent() {
  return (_amplitude > NOISE_FLOOR);
}

bool BrainIO::isClipping() {
  return (abs(_rawValue - ADC_MIDPOINT) > CLIP_THRESHOLD);
}

void BrainIO::printDiagnostics() {
  Serial.print("RAW:"); Serial.print(_rawValue);
  Serial.print("  CTR:"); Serial.print(getCentered());
  Serial.print("  AMP:"); Serial.print(_amplitude);
  Serial.print("  Hz~"); Serial.print(_dominantHz, 1);
  Serial.print("  Band:");

  if (isAlpha()) Serial.print("ALPHA");
  else if (isBeta())  Serial.print("BETA ");
  else Serial.print("--   ");

  if (!isSignalPresent()) Serial.print("  [NO SIGNAL]");
  if (isClipping()) Serial.print("  [CLIPPING]");
  if (isSignalPresent() && !isClipping()) Serial.print("  [OK]");

  Serial.println();
}
