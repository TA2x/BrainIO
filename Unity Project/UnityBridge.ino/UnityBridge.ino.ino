#include "EEG_Reader.h"

// Circuit output wire → Arduino A0
// (This is where the audio jack tip would go in the original guide — Step 8)
EEG_Reader eeg(A0);

#define PRINT_EVERY_MS 20   // how often to print (ms) — not too fast to read

unsigned long lastPrint = 0;

// ─────────────────────────────────────────────────────────────────────────────
void setup() {
  Serial.begin(115200);
  eeg.begin();
}

// ─────────────────────────────────────────────────────────────────────────────
void loop() {
  eeg.update();   // must be called every loop — handles timing internally

  if (millis() - lastPrint >= PRINT_EVERY_MS) {
    lastPrint = millis();

    Serial.print(eeg.getRawValue());   Serial.print(',');
    Serial.print(eeg.getCentered());   Serial.print(',');
    Serial.print(eeg.getAmplitude());  Serial.print(',');
    Serial.print(eeg.getDominantHz(), 1); Serial.print(',');
    Serial.println(eeg.isAlpha() ? 'A' : eeg.isBeta() ? 'B' : 'N');
  }
}