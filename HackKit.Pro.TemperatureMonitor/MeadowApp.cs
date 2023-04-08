namespace HackKit.Pro.TemperatureMonitor;

using Meadow;
using Meadow.Devices;
using Meadow.Foundation;
using Meadow.Foundation.Displays;
using Meadow.Foundation.Graphics;
using Meadow.Foundation.Sensors.Temperature;
using Meadow.Hardware;
using Meadow.Library.Peripherals;
using Meadow.Units;
using System;
using System.Threading.Tasks;

public class MeadowApp : App<F7FeatherV2>
{
    private readonly Color[] colors = new Color[4]
    {
        Color.FromHex("#67E667"),
        Color.FromHex("#00CC00"),
        Color.FromHex("#269926"),
        Color.FromHex("#008500")
    };

    private AnalogTemperature analogTemperature;

    private MicroGraphics graphics;

    private St7789 st7789;

    public override Task Initialize()
    {
        Console.WriteLine("Initializing...");

        OnboardLed led = new(Device.Pins, Color.Red);

        this.analogTemperature = new AnalogTemperature(
            analogPin: Device.Pins.A00,
            sensorType: AnalogTemperature.KnownSensorType.LM35
        );
        this.analogTemperature.TemperatureUpdated += this.AnalogTemperatureUpdated;

        SpiClockConfiguration config = new(
            speed: new Frequency(48000, Frequency.UnitType.Kilohertz),
            mode: SpiClockConfiguration.Mode.Mode3);

        ISpiBus spiBus = Device.CreateSpiBus(
                clock: Device.Pins.SCK,
                copi: Device.Pins.MOSI,
                cipo: Device.Pins.MISO,
                config: config);

        this.st7789 = new St7789(
            spiBus: spiBus,
            chipSelectPin: Device.Pins.D02,
            dcPin: Device.Pins.D01,
            resetPin: Device.Pins.D00,
            width: 240, height: 240);

        this.graphics = new MicroGraphics(this.st7789)
        {
            IgnoreOutOfBoundsPixels = true,
            Rotation = RotationType._270Degrees
        };

        led.SetColor(Color.Green);

        return base.Initialize();
    }

    public override Task Run()
    {
        this.LoadScreen();
        this.analogTemperature?.StartUpdating(TimeSpan.FromSeconds(5));

        return base.Run();
    }

    private void AnalogTemperatureUpdated(object sender, IChangeResult<Temperature> e)
    {
        this.graphics.DrawRectangle(
                x: 48, y: 160,
                width: 144,
                height: 40,
                color: this.colors[^1],
                filled: true);

        this.graphics.DrawText(
            x: 48, y: 160,
            text: $"{e.New.Celsius:00.0}°C",
            color: Color.White,
            scaleFactor: ScaleFactor.X2);

        this.graphics.Show();
    }

    private void LoadScreen()
    {
        Console.WriteLine("LoadScreen...");

        this.graphics.Clear();

        int radius = 225;
        int originX = this.graphics.Width / 2;
        int originY = (this.graphics.Height / 2) + 130;

        this.graphics.Stroke = 3;
        for (int i = 1; i < 5; i++)
        {
            this.graphics.DrawCircle(
                centerX: originX,
                centerY: originY,
                radius: radius,
                color: this.colors[i - 1],
                filled: true);

            radius -= 20;
        }

        this.graphics.DrawLine(0, 220, 239, 220, Color.White);
        this.graphics.DrawLine(0, 230, 239, 230, Color.White);

        this.graphics.CurrentFont = new Font12x20();
        this.graphics.DrawText(54, 130, "TEMPERATURE", Color.White);

        this.graphics.Show();
    }
}
