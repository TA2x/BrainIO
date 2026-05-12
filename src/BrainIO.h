#ifndef BRAINIO_H
#define BRAINIO_H

#include <Arduino.h>

// Brainwave band limits (Hz)
#define BAND_ALPHA_LOW_HZ   8
#define BAND_ALPHA_HIGH_HZ  12
#define BAND_BETA_LOW_HZ    12
#define BAND_BETA_HIGH_HZ   30

// Sampling
#define SAMPLE_RATE_HZ     256
#define SAMPLE_INTERVAL_MS (1000 / SAMPLE_RATE_HZ)

// ADC midpoint and thresholds
#define ADC_MIDPOINT     512
#define NOISE_FLOOR      8
#define CLIP_THRESHOLD   500

class EEG_Reader {
public:
  EEG_Reader(uint8_t analogPin);

  void begin();
  int  update();

  int getRawValue();
  int getCentered();
  int getAmplitude();

  float getDominantHz();
  bool  isAlpha();
  bool  isBeta();

  bool isSignalPresent();
  bool isClipping();

  void printDiagnostics();

private:
  uint8_t _pin;
  int     _rawValue;
  int     _baseline;
  int     _amplitude;

  unsigned int  _crossings;
  unsigned long _lastSecondMs;
  float         _dominantHz;

  unsigned long _lastSampleMs;

  void _updateBaseline(int sample);
};

#endif
