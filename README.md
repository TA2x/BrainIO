# BrainIO

Arduino library for reading EEG signals from the DIY EEG circuit based on **AD620AN + TL084CN**.

BrainIO connects directly to the circuit output and provides:

* Raw EEG readings
* Signal amplitude estimation
* Dominant frequency detection
* Alpha/Beta band classification
* Signal quality diagnostics

## Wiring

Connect the final output of Stage 6 (the same point used for the audio jack in the original guide) to Arduino.

| DIY EEG Circuit | Arduino |
| --------------- | ------- |
| Stage 6 Output  | A0      |
| GND             | GND     |

> The EEG circuit is powered by its own ±9V supply. Do not connect it to Arduino 5V or Vin. Only share GND.

### ADC Biasing

Arduino analog inputs cannot read negative voltages. Bias the signal to approximately **2.5V** using:

* 10kΩ from 5V to A0
* 10kΩ from A0 to GND
* 10µF capacitor from A0 to GND

## Installation

1. Copy `BrainIO` into your Arduino `libraries` folder.
2. Restart Arduino IDE.
3. Open **File → Examples → BrainIO → BasicTest**.
4. Upload and open Serial Monitor at **115200 baud**.

## Example

```cpp
#include <BrainIO.h>

BrainIO eeg(A0);

void setup() {
    Serial.begin(115200);
    eeg.begin();
}

void loop() {
    eeg.update();
    eeg.printDiagnostics();
}
```

## API

```cpp
eeg.begin();
eeg.update();

eeg.getRawValue();
eeg.getCentered();
eeg.getAmplitude();

eeg.getDominantHz();

eeg.isAlpha();
eeg.isBeta();

eeg.isSignalPresent();
eeg.isClipping();

eeg.printDiagnostics();
```

## Electrode Placement

* Ground → Left mastoid (behind left ear)
* Active (+) → Fp2 (forehead)
* Active (−) → O2 (back of head)

Use conductive gel and ensure good electrode contact for reliable readings.
