#include <Wire.h> // Библиотека для работы с I2C
#include <TroykaMeteoSensor.h> // Библиотека для работы с SHT31
#include <TroykaIMU.h> // Библиотека для работы с LPS25HB

TroykaMeteoSensor meteoSensor;
Barometer barometer;

void setup() {
  // Инициализация последовательного порта
  Serial.begin(9600);

  // Инициализация I2C
  Wire.begin();

  // Инициализация датчика SHT31
  meteoSensor.begin();

  // Инициализация датчика LPS25HB
  barometer.begin();
}

void meteoInfo() {
  // считываем данные с датчика
  int stateSensor = meteoSensor.read();
  // проверяем состояние данных
  switch (stateSensor) {
    case SHT_OK:
      // выводим показания влажности и температуры
      Serial.print("Temperature = ");
      Serial.print(meteoSensor.getTemperatureC());
      Serial.println(" C \t");
      Serial.print("Humidity = ");
      Serial.print(meteoSensor.getHumidity());
      Serial.println(" %\r\n");
      break;
    // ошибка данных или сенсор не подключён
    default:
      Serial.println("Data error or sensor not connected");
  }
}

void barometerInfo() {
  Serial.print("Pressure = ");
  Serial.print(barometer.readPressureMillimetersHg());
  Serial.println(" мм рт.ст.");

  Serial.print("Altitude = ");
  Serial.print(barometer.readAltitude());
  Serial.println(" m");

  Serial.print("Temperature = ");
  Serial.print(barometer.readTemperatureC());
  Serial.println(" C \t");
  Serial.println("\n");
}

void loop() {
  meteoInfo();
  barometerInfo();
  delay(1000); // Задержка между измерениями
}