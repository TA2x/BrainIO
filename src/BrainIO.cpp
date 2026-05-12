#include "BrainIO.h"

EEG_Reader::EEG_Reader(uint8_t analogPin)
: _pin(analogPin),
_rawValue(ADC_MIDPOINT),
_baseline(ADC_MIDPOINT),
_amplitude(0),
_crossings(0),
_lastSecondMs(0),
_dominantHz(0),
_lastSampleMs(0)
{}

void EEG_Reader::begin() {
  _baseline     = analogRead(_pin);
  _lastSampleMs = millis();
  _lastSecondMs = millis();
}

int EEG_Reader::update() {
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

void EEG_Reader::_updateBaseline(int sample) {
  _baseline = (_baseline * 99 + sample) / 100;
}

int EEG_Reader::getRawValue()  { return _rawValue; }
int EEG_Reader::getCentered()  { return _rawValue - _baseline; }
int EEG_Reader::getAmplitude() { return _amplitude; }
float EEG_Reader::getDominantHz() { return _dominantHz; }

bool EEG_Reader::isAlpha() {
  return (_dominantHz >= BAND_ALPHA_LOW_HZ && _dominantHz < BAND_ALPHA_HIGH_HZ);
}

bool EEG_Reader::isBeta() {
  return (_dominantHz >= BAND_BETA_LOW_HZ && _dominantHz < BAND_BETA_HIGH_HZ);
}

bool EEG_Reader::isSignalPresent() {
  return (_amplitude > NOISE_FLOOR);
}

bool EEG_Reader::isClipping() {
  return (abs(_rawValue - ADC_MIDPOINT) > CLIP_THRESHOLD);
}

void EEG_Reader::printDiagnostics() {
  Serial.print("RAW:");    Serial.print(_rawValue);
  Serial.print("  CTR:");  Serial.print(getCentered());
  Serial.print("  AMP:");  Serial.print(_amplitude);
  Serial.print("  Hz~");   Serial.print(_dominantHz, 1);
  Serial.print("  Band:");

  if      (isAlpha()) Serial.print("ALPHA");
  else if (isBeta())  Serial.print("BETA ");
  else                Serial.print("--   ");

  if (!isSignalPresent()) Serial.print("  [NO SIGNAL]");
  if (isClipping())       Serial.print("  [CLIPPING]");
  if (isSignalPresent() && !isClipping()) Serial.print("  [OK]");

  Serial.println();
}
