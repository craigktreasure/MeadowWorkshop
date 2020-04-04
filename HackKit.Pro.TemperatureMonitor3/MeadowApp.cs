namespace HackKit.Pro.TemperatureMonitor3
{
    using Meadow;
    using Meadow.Devices;
    using Meadow.Foundation;
    using Meadow.Foundation.Displays.Tft;
    using Meadow.Foundation.Graphics;
    using Meadow.Foundation.Sensors.Switches;
    using Meadow.Foundation.Sensors.Temperature;
    using Meadow.Hardware;
    using Meadow.Library.Converters;
    using Meadow.Library.Peripherals;
    using Meadow.Peripherals.Sensors.Atmospheric;
    using System;

    public class MeadowApp : App<F7Micro, MeadowApp>
    {
        private readonly AnalogTemperature analogTemperature;

        private readonly int displayWidth, displayHeight;

        private readonly GraphicsLibrary graphics;

        private readonly St7789 st7789;

        private readonly Mcp9808 mcp9808;

        private readonly SpdtSwitch spdtSwitch;

        private AtmosphericConditions lastAnalogConditions;

        private AtmosphericConditions lastMcp9808Conditions;

        private string lastMcp908TextValue = string.Empty;

        private string lastAnalogTextValue = string.Empty;

        private bool displayInCelcius = true;

        private readonly object updateLock = new object();

        public MeadowApp()
        {
            this.spdtSwitch = new SpdtSwitch(Device.CreateDigitalInputPort(Device.Pins.D04, InterruptMode.EdgeBoth));
            this.displayInCelcius = this.spdtSwitch.IsOn;
            this.spdtSwitch.Changed += this.SpdtSwitch_Changed;

            this.st7789 = InitializeLcdScreen(out this.displayWidth, out this.displayHeight, out this.graphics);
            this.analogTemperature = InitializeAnalogTemperatureSensor(this.AnalogTemperatureUpdated);

            this.mcp9808 = InitializeMcp9808TemperatureSensor();
            this.Display9808Temperature(this.mcp9808.GetTemperature());
            this.mcp9808.Subscribe(new FilterableObserver<AtmosphericConditionChangeResult, AtmosphericConditions>(
                this.Mcp9808TemperatureUpdated,
                e => Math.Abs(e.Delta.Temperature) > 0.1
                ));
            this.mcp9808.StartUpdating();

            OnboardLed led = new OnboardLed(Device);
            led.SetColor(RgbColor.Green);
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

            Mcp9808 mcp9808 = Mcp9808.FromF7Micro(Device);

            Console.WriteLine($"Device ID: {mcp9808.GetDeviceId()}");
            Console.WriteLine($"Device Revision: {mcp9808.GetDeviceRevision()}");
            Console.WriteLine($"Manufacturer ID: {mcp9808.GetManufacturerId()}");
            Console.WriteLine($"Resolution: {mcp9808.GetResolution()}");
            Console.WriteLine($"Temperature: {mcp9808.GetTemperature().Temperature} deg C");

            return mcp9808;
        }

        private static St7789 InitializeLcdScreen(out int displayWidth, out int displayHeight, out GraphicsLibrary graphicsLibrary)
        {
            Console.WriteLine("Initializing LCD screen...");

            SpiClockConfiguration config = new SpiClockConfiguration(
                speedKHz: 6000,
                SpiClockConfiguration.Mode.Mode3);

            ISpiBus spiBus = Device.CreateSpiBus(
                    Device.Pins.SCK, Device.Pins.MOSI, Device.Pins.MISO, config);

            St7789 st7789 = new St7789(
                device: Device,
                spiBus: spiBus,
                chipSelectPin: Device.Pins.D02,
                dcPin: Device.Pins.D01,
                resetPin: Device.Pins.D00,
                width: 240, height: 240
            );

            displayWidth = Convert.ToInt32(st7789.Width);
            displayHeight = Convert.ToInt32(st7789.Height);

            graphicsLibrary = new GraphicsLibrary(st7789)
            {
                Rotation = GraphicsLibrary.RotationType._270Degrees
            };

            LoadScreen(graphicsLibrary, displayWidth, displayHeight);

            return st7789;
        }

        private static AnalogTemperature InitializeAnalogTemperatureSensor(EventHandler<AtmosphericConditionChangeResult> updatedHandler)
        {
            Console.WriteLine("Initializing analog temperature sensor...");

            AnalogTemperature analogTemperature = new AnalogTemperature(
                device: Device,
                analogPin: Device.Pins.A00,
                sensorType: AnalogTemperature.KnownSensorType.LM35
            );
            analogTemperature.Updated += updatedHandler;

            analogTemperature.StartUpdating();

            return analogTemperature;
        }

        private void AnalogTemperatureUpdated(object sender, AtmosphericConditionChangeResult e)
        {
            lock (this.updateLock)
            {
                this.DisplayAnalogTemperature(new AtmosphericConditions
                {
                    Humidity = e.New.Humidity,
                    Pressure = e.New.Pressure,
                    Temperature = e.New.Temperature / 1000
                });
            }
        }

        private void DisplayAnalogTemperature(AtmosphericConditions conditions)
        {
            if (!string.IsNullOrEmpty(this.lastAnalogTextValue))
            {
                this.graphics.DrawText(
                    x: 48, y: 160,
                    text: this.lastAnalogTextValue,
                    color: Color.Black,
                    scaleFactor: GraphicsLibrary.ScaleFactor.X2);
            }

            this.lastAnalogTextValue = this.GetTemperatureDisplayText(conditions);
            this.lastAnalogConditions = conditions;

            this.graphics.DrawText(
                x: 48, y: 160,
                text: this.lastAnalogTextValue,
                color: Color.White,
                scaleFactor: GraphicsLibrary.ScaleFactor.X2);

            this.graphics.Show();
        }

        private void Mcp9808TemperatureUpdated(AtmosphericConditionChangeResult e)
        {
            lock (this.updateLock)
            {
                this.Display9808Temperature(e.New);
            }
        }

        private void Display9808Temperature(AtmosphericConditions conditions)
        {
            if (!string.IsNullOrEmpty(this.lastMcp908TextValue))
            {
                this.graphics.DrawText(
                    x: 48, y: 40,
                    text: this.lastMcp908TextValue,
                    color: Color.Black,
                    scaleFactor: GraphicsLibrary.ScaleFactor.X2);
            }

            this.OutputMcp9808ConditionsToConsole(conditions);

            this.lastMcp908TextValue = this.GetTemperatureDisplayText(conditions);
            this.lastMcp9808Conditions = conditions;

            this.graphics.DrawText(
                x: 48, y: 40,
                text: this.lastMcp908TextValue,
                color: Color.White,
                scaleFactor: GraphicsLibrary.ScaleFactor.X2);
        }

        private void OutputMcp9808ConditionsToConsole(AtmosphericConditions conditions)
        {
            Console.WriteLine("Mcp9808 Atmospheric conditions:");
            Console.WriteLine($"  Temperature: {conditions.Temperature} deg C");
        }

        private static void LoadScreen(GraphicsLibrary graphicsLibrary, int displayWidth, int displayHeight)
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

        private string GetTemperatureDisplayText(AtmosphericConditions conditions)
        {
            float temp = conditions.Temperature;
            char unit = this.displayInCelcius ? 'C' : 'F';

            if (!this.displayInCelcius)
            {
                temp = Temperature.ConvertCelsiusToFahrenheit(temp);
            }

            return $"{temp:00.0}°{unit}";
        }
    }
}
