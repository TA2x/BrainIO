#include "EEGLTWM.h"

EEGLTWM::EEGLTWM(int pin) {
  _pin = pin;
}

void EEGLTWM::begin() {
  pinMode(_pin, INPUT);
}

void EEGLTWM::update() {
  _prevRaw = _raw;
  _raw = analogRead(_pin);

  if (abs(_raw - _prevRaw) > 100){
    _blink = true;
  } else {
    _blink = false;
  }
}

int EEGLTWM::getRaw(){
  return _raw;
}

bool EEGLTWM::blinkDetected(){
  return _blink;
}

int EEGLTWM::getFocus(){
  return map(_raw, 0, 1023, 0, 100);
}
