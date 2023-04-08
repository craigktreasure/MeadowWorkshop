namespace HackKit.Pro.TemeratureMonitor2;

using Meadow;
using Meadow.Devices;
using Meadow.Foundation;
using Meadow.Foundation.Displays.Lcd;
using Meadow.Foundation.Sensors.Atmospheric;
using Meadow.Library;
using Meadow.Library.Peripherals;
using Meadow.Units;
using System;
using System.Threading.Tasks;

// Change F7FeatherV2 to F7FeatherV1 for V1.x boards
public class MeadowApp : App<F7FeatherV1>
{
    private Bme280 bme280;

    private CharacterDisplay display;

    public override Task Initialize()
    {
        OnboardLed led = new(Device.Pins, Color.Red);

        this.InitializeCharacterDisplay();
        this.InitializeBme280();

        led.SetColor(Color.Green);

        return base.Initialize();
    }

    public override async Task Run()
    {
        // get an initial reading
        await this.ReadConditions();

        // start updating continuously
        this.bme280.StartUpdating(TimeSpan.FromSeconds(1));

        await base.Run();
    }

    protected async Task ReadConditions()
    {
        (Temperature?, RelativeHumidity?, Pressure?) conditions = await this.bme280.Read();
        this.OutputConditionsToConsole(conditions);
    }

    private void InitializeBme280()
    {
        Console.WriteLine("Temperature (BME280) Initializing...");

        // configure our BME280 on the I2C Bus
        this.bme280 = new Bme280(
            Device.CreateI2cBus(),
            (byte)Bme280.Addresses.Address_0x77 //default
        );

        this.bme280.Subscribe(Bme280.CreateObserver(
            handler: h => this.OutputConditions(h.New),
            filter: e =>
            {
                if (e.Old is { } old)
                {
                    double tempDelta = (e.New.Temperature.Value - old.Temperature.Value).Abs().Celsius;
                    double pressureDelta = (e.New.Pressure.Value - old.Pressure.Value).Abs().Bar;
                    double humidityDelta = (e.New.Humidity.Value - old.Humidity.Value).Abs().Percent;
                    return (tempDelta > 0.2) || (pressureDelta > 5 || (humidityDelta > 0.1f));
                }

                return false;
            }));

        // get chip id
        Console.WriteLine($"ChipID: {this.bme280.GetChipID():X2}");

        Console.WriteLine("Temperature (BME280) Initialized");
    }

    private void InitializeCharacterDisplay()
    {
        Console.WriteLine("Charter display initializing...");

        this.display = new CharacterDisplay(
            pinRS: Device.Pins.D15,
            pinE: Device.Pins.D14,
            pinD4: Device.Pins.D13,
            pinD5: Device.Pins.D12,
            pinD6: Device.Pins.D11,
            pinD7: Device.Pins.D10,
            rows: 4, columns: 20    // Adjust dimensions to fit your display
        );

        this.display.ClearLines();

        this.display.WriteLine("Conditions", 0);
        this.display.WriteLine("Initializing...", 1);
    }

    private void OutputConditions(AtmosphericConditions conditions)
    {
        this.OutputConditionsToDisplay(conditions);

        this.OutputConditionsToConsole(conditions);
    }

    private void OutputConditionsToConsole(AtmosphericConditions conditions)
    {
        Console.WriteLine("Atmospheric conditions:");
        Console.WriteLine($"  Temperature: {conditions.Temperature?.Celsius} deg C");
        Console.WriteLine($"  Pressure: {conditions.Pressure} Pa");
        Console.WriteLine($"  Relative Humidity: {conditions.Humidity}%");
    }

    private void OutputConditionsToDisplay(AtmosphericConditions conditions)
    {
        double degreesCelsius = conditions.Temperature?.Celsius ?? 0;
        double degreesFaranheit = conditions.Temperature?.Fahrenheit ?? 0;

        this.display.WriteLine($"Temp: {degreesCelsius:00.0} C {degreesFaranheit:00.0} F", 1);

        this.display.WriteLine($"Press: {conditions.Pressure:0.0} Pa", 2);
        this.display.WriteLine($"Humidity: {conditions.Humidity:00.0}%", 3);
    }
}
