namespace HackKit.Pro.TemperatureMonitor3;

using Meadow;
using Meadow.Devices;
using Meadow.Foundation;
using Meadow.Foundation.Displays;
using Meadow.Foundation.Graphics;
using Meadow.Foundation.Sensors.Switches;
using Meadow.Foundation.Sensors.Temperature;
using Meadow.Hardware;
using Meadow.Library.Peripherals;
using Meadow.Units;
using System;
using System.Threading.Tasks;

// Change F7FeatherV2 to F7FeatherV1 for V1.x boards
public class MeadowApp : App<F7FeatherV1>
{
    private AnalogTemperature analogTemperature;

    private MicroGraphics graphics;

    private St7789 st7789;

    private Mcp9808 mcp9808;

    private SpdtSwitch spdtSwitch;

    private Temperature lastAnalogConditions;

    private Temperature lastMcp9808Conditions;

    private string lastMcp908TextValue = string.Empty;

    private string lastAnalogTextValue = string.Empty;

    private bool displayInCelcius = true;

    private readonly object updateLock = new object();

    public override Task Run()
    {
        TimeSpan updateDuration = TimeSpan.FromSeconds(5);

        this.mcp9808 = InitializeMcp9808TemperatureSensor();
        if (this.mcp9808.Temperature is not null)
        {
            this.Display9808Temperature(this.mcp9808.Temperature.Value);
        }

        this.mcp9808.Subscribe(Mcp9808.CreateObserver(
            handler: this.Mcp9808TemperatureUpdated,
            filter: result =>
            {
                if (result.Old is not null)
                {
                    return (result.New - result.Old.Value).Abs().Celsius > 0.1;
                }

                return false;
            }));

        this.mcp9808.StartUpdating(updateDuration);
        this.analogTemperature.StartUpdating(updateDuration);

        return base.Run();
    }

    public override Task Initialize()
    {
        OnboardLed led = new(Device.Pins, Color.Red);

        this.spdtSwitch = new SpdtSwitch(Device.CreateDigitalInputPort(Device.Pins.D04, InterruptMode.EdgeBoth));
        this.displayInCelcius = this.spdtSwitch.IsOn;
        this.spdtSwitch.Changed += this.SpdtSwitch_Changed;

        this.st7789 = InitializeLcdScreen(out this.graphics);
        this.analogTemperature = InitializeAnalogTemperatureSensor(this.AnalogTemperatureUpdated);
        this.mcp9808 = InitializeMcp9808TemperatureSensor();

        led.SetColor(Color.Green);

        return base.Initialize();
    }

    private void SpdtSwitch_Changed(object sender, EventArgs e)
    {
        lock (this.updateLock)
        {
            this.displayInCelcius = this.spdtSwitch.IsOn;

            if (this.displayInCelcius)
            {
                Console.WriteLine("Now displaying in Celcius");
            }
            else
            {
                Console.WriteLine("Now displaying in Fahrenheit");
            }

            if (this.lastMcp9808Conditions != null)
            {
                this.Display9808Temperature(this.lastMcp9808Conditions);
            }

            if (this.lastAnalogConditions != null)
            {
                this.Display9808Temperature(this.lastAnalogConditions);
            }
        }
    }

    private static Mcp9808 InitializeMcp9808TemperatureSensor()
    {
        Console.WriteLine("Initializing MCP9808 temperature sensor...");

        Mcp9808 mcp9808 = new(Device.CreateI2cBus());

        Console.WriteLine($"Device ID: {mcp9808.GetDeviceId()}");
        Console.WriteLine($"Manufacturer ID: {mcp9808.GetManufactureId()}");
        Console.WriteLine($"Resolution: {mcp9808.GetResolution()}");
        Console.WriteLine($"Temperature: {mcp9808.Temperature} deg C");

        return mcp9808;
    }

    private static St7789 InitializeLcdScreen(out MicroGraphics graphicsLibrary)
    {
        Console.WriteLine("Initializing LCD screen...");

        SpiClockConfiguration config = new(
            speed: new Frequency(48000, Frequency.UnitType.Kilohertz),
            mode: SpiClockConfiguration.Mode.Mode3);

        ISpiBus spiBus = Device.CreateSpiBus(
                clock: Device.Pins.SCK,
                copi: Device.Pins.MOSI,
                cipo: Device.Pins.MISO,
                config: config);

        St7789 st7789 = new(
            spiBus: spiBus,
            chipSelectPin: Device.Pins.D02,
            dcPin: Device.Pins.D01,
            resetPin: Device.Pins.D00,
            width: 240, height: 240
        );

        graphicsLibrary = new MicroGraphics(st7789)
        {
            IgnoreOutOfBoundsPixels = true,
            Rotation = RotationType._270Degrees
        };

        LoadScreen(graphicsLibrary);

        return st7789;
    }

    private static AnalogTemperature InitializeAnalogTemperatureSensor(EventHandler<IChangeResult<Temperature>> updatedHandler)
    {
        Console.WriteLine("Initializing analog temperature sensor...");

        AnalogTemperature analogTemperature = new(
            analogPin: Device.Pins.A00,
            sensorType: AnalogTemperature.KnownSensorType.LM35);
        analogTemperature.TemperatureUpdated += updatedHandler;

        return analogTemperature;
    }

    private void AnalogTemperatureUpdated(object sender, IChangeResult<Temperature> e)
    {
        lock (this.updateLock)
        {
            this.DisplayAnalogTemperature(e.New);
        }
    }

    private void DisplayAnalogTemperature(Temperature temperature)
    {
        if (!string.IsNullOrEmpty(this.lastAnalogTextValue))
        {
            this.graphics.DrawText(
                x: 48, y: 160,
                text: this.lastAnalogTextValue,
                color: Color.Black,
                scaleFactor: ScaleFactor.X2);
        }

        this.lastAnalogTextValue = this.GetTemperatureDisplayText(temperature);
        this.lastAnalogConditions = temperature;

        this.graphics.DrawText(
            x: 48, y: 160,
            text: this.lastAnalogTextValue,
            color: Color.White,
            scaleFactor: ScaleFactor.X2);

        this.graphics.Show();
    }

    private void Mcp9808TemperatureUpdated(IChangeResult<Temperature> e)
    {
        lock (this.updateLock)
        {
            this.Display9808Temperature(e.New);
        }
    }

    private void Display9808Temperature(Temperature temperature)
    {
        if (!string.IsNullOrEmpty(this.lastMcp908TextValue))
        {
            this.graphics.DrawText(
                x: 48, y: 40,
                text: this.lastMcp908TextValue,
                color: Color.Black,
                scaleFactor: ScaleFactor.X2);
        }

        this.OutputMcp9808ConditionsToConsole(temperature);

        this.lastMcp908TextValue = this.GetTemperatureDisplayText(temperature);
        this.lastMcp9808Conditions = temperature;

        this.graphics.DrawText(
            x: 48, y: 40,
            text: this.lastMcp908TextValue,
            color: Color.White,
            scaleFactor: ScaleFactor.X2);
    }

    private void OutputMcp9808ConditionsToConsole(Temperature temperature)
    {
        Console.WriteLine("Mcp9808 Atmospheric conditions:");
        Console.WriteLine($"  Temperature: {temperature} deg C");
    }

    private static void LoadScreen(MicroGraphics graphicsLibrary)
    {
        Console.WriteLine("LoadScreen...");

        graphicsLibrary.Clear();

        graphicsLibrary.Stroke = 3;

        graphicsLibrary.CurrentFont = new Font12x20();
        graphicsLibrary.DrawText(54, 10, "MCP9808", Color.White);

        graphicsLibrary.DrawText(54, 130, "LM35 DZ", Color.White);

        graphicsLibrary.DrawLine(0, 220, 240, 220, Color.White);
        graphicsLibrary.DrawLine(0, 230, 240, 230, Color.White);

        graphicsLibrary.Show();
    }

    private string GetTemperatureDisplayText(Temperature temperature)
    {
        double temp = this.displayInCelcius ? temperature.Celsius : temperature.Fahrenheit;
        char unit = this.displayInCelcius ? 'C' : 'F';

        return $"{temp:00.0}°{unit}";
    }
}
