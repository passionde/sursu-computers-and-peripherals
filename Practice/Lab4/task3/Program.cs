using Iot.Device.OneWire;

bool flag = true;

Console.CancelKeyPress += (sender, eventArgs) =>
{
    flag = false;
};

while (flag)
{
    foreach (var dev in OneWireThermometerDevice.EnumerateDevices())
    {
        Console.WriteLine($"Name: {dev.DeviceId}");
        var temp = await dev.ReadTemperatureAsync();
        Console.WriteLine($"Temperature: {temp.DegreesCelsius:g}\n");
    }
}

