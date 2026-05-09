# BrainIO — Arduino Library
### For the Instructables DIY EEG Circuit (AD620AN + TL084CN)

---

## How the original circuit works (quick recap)

The guide sends the final signal into a PC via a 3.5mm audio jack (Step 8).
We skip the audio jack entirely and wire that same output directly to Arduino.

The 6 analog stages in the circuit handle everything before Arduino sees it:

| Stage | What it does |
|---|---|
| 1 — AD620AN | Amplifies the tiny brain signal ~89× (set by the 560Ω resistor) |
| 2 — 60Hz notch | Kills power-line hum |
| 3 — 7Hz HPF | Removes slow skin/DC drift |
| 4 — 31Hz LPF | Cuts noise above beta waves |
| 5 — Variable gain | Extra 83–455× boost via potentiometer |
| 6 — 60Hz notch | Second hum removal → **this is where we tap the output** |

---

## Wiring to Arduino

The output tap point is described in Step 8 of the guide:
**between the 22kΩ resistor and 220nF capacitor in Stage 6.**
This is where the yellow alligator clip went in the guide's photo.

```
DIY EEG Circuit                     Arduino Uno / Nano
───────────────                     ──────────────────

  Stage 6 output                         
  (between 22kΩ & 220nF cap) ────────►  A0   ← signal wire
  
  Circuit GND  ───────────────────────►  GND  ← IMPORTANT: shared ground
  
  NOTE: The circuit's ±9V batteries power the op-amps only.
        They do NOT connect to Arduino's 5V or Vin.
        Only GND is shared.
```

> **Important:** Arduino's ADC reads 0–5V only. The circuit output is
> centered at 0V (circuit GND). You need the signal to sit around 2.5V
> for Arduino to see both the positive and negative swings.
>
> **Quick fix:** Add a voltage divider bias at A0 — two 10kΩ resistors
> from 5V and GND meeting at A0, with a 10µF cap to GND. This shifts
> the signal up to 2.5V without affecting the AC brain signal.

---

## Electrode placement (from Step 9 of the guide)

```
  GND electrode   → Left mastoid (bone behind left ear)
  Active (+IN)    → ~1 inch above & right of nasion (Fp2 position, forehead)
  Active (-IN)    → ~1 inch above inion (O2 position, back of head)
```

Use electrode gel for good contact. Secure with a bandana as the guide suggests.

---

## Installation

1. Copy the `EEG_Reader` folder into your Arduino `libraries/` directory.
2. Restart Arduino IDE.
3. Open **File → Examples → EEG_Reader → BasicTest**.
4. Upload to Arduino, open Serial Monitor at **115200 baud**.

---

## What to look for in Serial Monitor

```
RAW:519  CTR:+7   AMP:3   Hz~0.0  Band:--    [NO SIGNAL — check electrodes]
```
→ No signal. Check gel contact and GND connection.

```
RAW:531  CTR:+19  AMP:22  Hz~10.3  Band:ALPHA  [OK]
```
→ Healthy Alpha signal! Eyes are probably closed and relaxed.

```
RAW:548  CTR:+36  AMP:41  Hz~18.1  Band:BETA   [OK]
```
→ Beta activity — alert / eyes open state.

```
RAW:812  CTR:+300  AMP:290  Hz~14.2  Band:BETA  [CLIPPING — turn pot down]
```
→ Too much gain. Turn the Stage 5 potentiometer anticlockwise.

---

## API

```cpp
EEG_Reader eeg(A0);

eeg.begin();              // call in setup()
eeg.update();             // call every loop()

eeg.getRawValue();        // 0–1023, raw ADC
eeg.getCentered();        // deviation from DC midpoint (+ or -)
eeg.getAmplitude();       // smoothed signal strength

eeg.getDominantHz();      // estimated dominant frequency
eeg.isAlpha();            // true if 8–12 Hz
eeg.isBeta();             // true if 12–30 Hz

eeg.isSignalPresent();    // false = flat line
eeg.isClipping();         // true = reduce potentiometer gain
eeg.printDiagnostics();   // one-line Serial summary
```
