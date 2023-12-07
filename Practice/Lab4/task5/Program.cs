using Raven.Iot.Device;
using System.Device.I2c;
using Iot.Device.RotaryEncoder;
using System.Device.Gpio;
using Raven.Iot.Device.MicrocontrollerBoard;

const int D0 = 24;
const int D1 = 25;

if (DeviceHelper.I2cDeviceSearch([1], [32]) is [I2cConnectionSettings settings])
{
    using MicrocontrollerBoard<Reqest, Response> board = new(settings);

    using ScaledQuadratureEncoder encoder = new(
        pinA: DeviceHelper.WiringPiToBcm(D0),
        pinB: DeviceHelper.WiringPiToBcm(D1),
        PinEventTypes.Falling,
        pulsesPerRotation: 20,
        pulseIncrement: 1,
        rangeMin: 0.0,
        rangeMax: 255.0);

    encoder.Debounce = TimeSpan.FromMilliseconds(2);

    encoder.ValueChanged += (o, e) =>
    {
        board.WriteRequest(new() { Pwm = (int)e.Value });
        Console.WriteLine(e.Value);
    };

    Console.ReadKey();
}

Console.WriteLine("Device not found");


public readonly record struct Reqest(int Pwm);
public readonly record struct Response();
