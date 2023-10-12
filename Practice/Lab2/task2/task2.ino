void setup() {
  analogWriteFreq(25000);
}

void loop() {
  int val = map(analogRead(A0), 0, 4095, 0, 255);
  analogWrite(D0, val);
  analogWrite(D1, val);
}