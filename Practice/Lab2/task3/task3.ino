#include <SPI.h>
#include <AmperkaFET.h>
#include <sstream>
#include <iostream>

FET mosfet(D17, 2);
 
void setup() {
  mosfet.begin();
}
 
void loop() {
  if (!Serial.available()) return;

  std::stringstream ss;
  ss << Serial.readString().c_str();

  uint32_t device, pin;
  bool mode;
  ss >> device >> pin >> mode;

  if (device > 1) device = 255;
  if (pin > 7) pin = 255;

  mosfet.digitalWrite(device, pin, mode);
}