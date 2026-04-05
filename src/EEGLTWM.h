#ifndef EEGLTWM_H
#define EEGLTWM_H

#include <Arduino.h>

class EEGLTWM {
  public:
    EEGLTWM(int pin);
    
    void begin();
    void update();

    int getRaw();
    bool blinkDetected();
    int getFocus();

  private:
    int _pin;
    int _raw;
    int _prevRaw;

    bool _blink;
};

#endif
