namespace HackKit.Pro.TemperatureMonitor
{
    using Meadow;
    using Meadow.Devices;
    using Meadow.Foundation;
    using Meadow.Foundation.Displays.Tft;
    using Meadow.Foundation.Graphics;
    using Meadow.Foundation.Sensors.Temperature;
    using Meadow.Hardware;
    using Meadow.Peripherals.Sensors.Atmospheric;
    using System;

    public class MeadowApp : App<F7Micro, MeadowApp>
    {
        private readonly AnalogTemperature analogTemperature;

        private readonly Color[] colors = new Color[4]
                {
            Color.FromHex("#008500"),
            Color.FromHex("#269926"),
            Color.FromHex("#00CC00"),
            Color.FromHex("#67E667")
        };

        private readonly int displayWidth, displayHeight;

        private readonly GraphicsLibrary graphics;

        private readonly St7789 st7789;

        public MeadowApp()
        {
            Console.WriteLine("Initializing...");

            this.analogTemperature = new AnalogTemperature(
                device: Device,
                analogPin: Device.Pins.A00,
                sensorType: AnalogTemperature.KnownSensorType.LM35
            );
            this.analogTemperature.Updated += this.AnalogTemperatureUpdated;

            SpiClockConfiguration config = new SpiClockConfiguration(
                speedKHz: 6000,
                SpiClockConfiguration.Mode.Mode3);

            ISpiBus spiBus = Device.CreateSpiBus(
                    Device.Pins.SCK, Device.Pins.MOSI, Device.Pins.MISO, config);

            this.st7789 = new St7789(
                device: Device,
                spiBus: spiBus,
                chipSelectPin: Device.Pins.D02,
                dcPin: Device.Pins.D01,
                resetPin: Device.Pins.D00,
                width: 240, height: 240
            );

            this.displayWidth = Convert.ToInt32(this.st7789.Width);
            this.displayHeight = Convert.ToInt32(this.st7789.Height);

            this.graphics = new GraphicsLibrary(this.st7789)
            {
                Rotation = GraphicsLibrary.RotationType._270Degrees
            };

            this.LoadScreen();
            this.analogTemperature.StartUpdating();
        }

        private void AnalogTemperatureUpdated(object sender, AtmosphericConditionChangeResult e)
        {
            float oldTemp = e.Old.Temperature / 1000;
            float newTemp = e.New.Temperature / 1000;

            this.graphics.DrawText(
                x: 48, y: 160,
                text: $"{oldTemp.ToString("##.#")}°C",
                color: this.colors[this.colors.Length - 1],
                scaleFactor: GraphicsLibrary.ScaleFactor.X2);
            this.graphics.DrawText(
                x: 48, y: 160,
                text: $"{newTemp.ToString("##.#")}°C",
                color: Color.White,
                scaleFactor: GraphicsLibrary.ScaleFactor.X2);

            this.graphics.Show();
        }

        private void LoadScreen()
        {
            Console.WriteLine("LoadScreen...");

            this.graphics.Clear();

            int radius = 225;
            int originX = this.displayWidth / 2;
            int originY = (this.displayHeight / 2) + 130;

            this.graphics.Stroke = 3;
            for (int i = 1; i < 5; i++)
            {
                this.graphics.DrawCircle(
                    centerX: originX,
                    centerY: originY,
                    radius: radius,
                    color: this.colors[i - 1],
                    filled: true);

                this.graphics.Show();
                radius -= 20;
            }

            this.graphics.DrawLine(0, 220, 240, 220, Color.White);
            this.graphics.DrawLine(0, 230, 240, 230, Color.White);

            this.graphics.CurrentFont = new Font12x20();
            this.graphics.DrawText(54, 130, "TEMPERATURE", Color.White);

            this.graphics.Show();
        }
    }
}
