#ifndef EEG_READER_H
#define EEG_READER_H

#include <Arduino.h>

// ═══════════════════════════════════════════════════════════════════════════
//  EEG_Reader — for the Instructables DIY EEG circuit (AD620AN + TL084CN)
//
//  The circuit already does all the hard analog work:
//    Stage 1 : AD620AN instrumentation amp — gain ~89x (560Ω resistor)
//    Stage 2 : 60 Hz notch filter  (kills power-line hum)
//    Stage 3 : 7 Hz high-pass filter (removes slow skin/DC drift)
//    Stage 4 : 31 Hz low-pass filter (removes noise above beta waves)
//    Stage 5 : variable gain amp  83x–455x (set with potentiometer)
//    Stage 6 : second 60 Hz notch filter → SIGNAL OUTPUT
//
//  The output (tapped between the 22kΩ resistor and 220nF cap in Stage 6)
//  goes to Arduino A0 instead of the PC sound card.
// ═══════════════════════════════════════════════════════════════════════════

// ── Brainwave band limits (Hz) ───────────────────────────────────────────────
// This circuit is designed for Alpha (8–12 Hz) and Beta (12–30 Hz) primarily.
// The 7 Hz HPF cuts Delta/Theta anyway, so those are filtered in hardware.
#define BAND_ALPHA_LOW_HZ   8
#define BAND_ALPHA_HIGH_HZ  12
#define BAND_BETA_LOW_HZ    12
#define BAND_BETA_HIGH_HZ   30

// ── Sampling ─────────────────────────────────────────────────────────────────
// The circuit passes 7–31 Hz. Nyquist says we need > 62 samples/sec minimum.
// 256 Hz is a standard EEG sample rate and well within Arduino's capability.
#define SAMPLE_RATE_HZ     256
#define SAMPLE_INTERVAL_MS (1000 / SAMPLE_RATE_HZ)   // ~3.9 ms

// ── ADC midpoint ─────────────────────────────────────────────────────────────
// The circuit runs on ±9V batteries. The output is centered around 0V (circuit GND).
// Arduino only reads 0–5V, so we bias the output to sit at ~2.5V = ADC 512.
// If your readings idle near 512, the DC bias is correct.
#define ADC_MIDPOINT  512

// ── Signal quality thresholds ────────────────────────────────────────────────
// The circuit's Stage 5 potentiometer sets final gain (83–455x on top of 89x).
// A healthy signal should swing above the noise floor without clipping the ADC.
#define NOISE_FLOOR        8    // ADC counts — below = flat line / no contact
#define CLIP_THRESHOLD   500    // ADC counts from midpoint — above = too much gain


class EEG_Reader {
public:
  // Pass the analog pin wired to the circuit's output (e.g. A0)
  EEG_Reader(uint8_t analogPin);

  void begin();    // Call once in setup()
  int  update();   // Call every loop() — handles timing internally

  // ── Raw readings ──────────────────────────────────────────────────────────
  int getRawValue();   // Latest ADC reading (0–1023)
  int getCentered();   // Reading minus the DC midpoint (negative to positive)
  int getAmplitude();  // Smoothed peak swing — main indicator of signal strength

  // ── Band detection (zero-crossing estimator) ──────────────────────────────
  // Counts how many times per second the signal crosses the midpoint.
  // Crossings/2 ≈ dominant frequency in Hz.
  // Not a real FFT — but enough to tell Alpha from Beta for a sanity check.
  float getDominantHz();  // Estimated frequency right now
  bool  isAlpha();        // true if dominant freq is in 8–12 Hz range
  bool  isBeta();         // true if dominant freq is in 12–30 Hz range

  // ── Signal quality ────────────────────────────────────────────────────────
  bool isSignalPresent();  // false = flat line → check electrodes / connections
  bool isClipping();       // true = too much gain → turn the potentiometer down

  // Prints a one-line summary to Serial — open at 115200 baud
  void printDiagnostics();

private:
  uint8_t _pin;
  int     _rawValue;
  int     _baseline;       // Slow EMA tracks DC drift — should stay near ADC_MIDPOINT
  int     _amplitude;      // Smoothed peak deviation from baseline

  // Zero-crossing state
  unsigned int  _crossings;
  unsigned long _lastSecondMs;
  float         _dominantHz;   // Updated once per second

  unsigned long _lastSampleMs;

  void _updateBaseline(int sample);
};

#endif
