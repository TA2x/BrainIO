/*
  BasicTest.ino — EEG_Reader Circuit Test Sketch
  ═══════════════════════════════════════════════════════════════════════════
  PURPOSE:
    Verify your Instructables DIY EEG circuit (AD620AN + TL084CN) is alive
    and giving a sensible signal before doing anything else with it.

  WHAT TO EXPECT:
    1. No electrodes connected  → AMP stays < 8, "[NO SIGNAL]" shown
    2. Electrodes on, eyes open → AMP climbs, Hz in Beta range (12–30)
    3. Electrodes on, eyes shut and relaxed → Hz drops to Alpha range (8–12)
       (this is the key test — it's what the circuit was designed to show!)

  Open Serial Monitor at 115200 baud to watch.
  ═══════════════════════════════════════════════════════════════════════════
*/

#include "EEG_Reader.h"

// Circuit output wire → Arduino A0
// (This is where the audio jack tip would go in the original guide — Step 8)
EEG_Reader eeg(A0);

#define PRINT_EVERY_MS 300   // how often to print (ms) — not too fast to read

unsigned long lastPrint = 0;

// ─────────────────────────────────────────────────────────────────────────────
void setup() {
  Serial.begin(115200);
  Serial.println();
  Serial.println("====== DIY EEG Circuit — BasicTest ======");
  Serial.println("AD620AN + TL084CN | Instructables guide");
  Serial.println();
  Serial.println("Checklist before starting:");
  Serial.println("  [ ] 2x 9V batteries connected (±9V to op-amps)");
  Serial.println("  [ ] GND electrode on left mastoid (bone behind ear)");
  Serial.println("  [ ] Active electrodes at Fp2 (forehead) and O2 (back of head)");
  Serial.println("  [ ] Stage 5 potentiometer set to mid-position to start");
  Serial.println("  [ ] Circuit output wire connected to Arduino A0");
  Serial.println("  [ ] Circuit GND connected to Arduino GND");
  Serial.println();

  eeg.begin();
}

// ─────────────────────────────────────────────────────────────────────────────
void loop() {
  eeg.update();   // must be called every loop — handles timing internally

  if (millis() - lastPrint >= PRINT_EVERY_MS) {
    lastPrint = millis();

    eeg.printDiagnostics();

    // Extra guidance printed below each line
    if (!eeg.isSignalPresent()) {
      Serial.println("  → Check electrode gel contact, and that GND is wired to Arduino GND");
    }
    else if (eeg.isClipping()) {
      Serial.println("  → Turn the Stage 5 potentiometer anticlockwise to reduce gain");
    }
    else if (eeg.isAlpha()) {
      Serial.println("  → Alpha detected! Try opening eyes — it should drop toward Beta");
    }
    else if (eeg.isBeta()) {
      Serial.println("  → Beta range. Close eyes and relax — it should shift to Alpha");
    }
  }
}
