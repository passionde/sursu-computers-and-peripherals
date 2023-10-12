#include <Servo.h>

Servo servo;

void setup() {
  servo.attach(D0, 600, 2600);
}

void loop() {
  int angle = map(analogRead(A0), 0, 4095, 0, 180);
  servo.write(angle);
  delay(25);
}