namespace HackKit.Pro.TemeratureMonitor2
{
    using Meadow;
    using Meadow.Devices;
    using Meadow.Foundation.Displays.Lcd;
    using Meadow.Foundation.Sensors.Atmospheric;
    using Meadow.Library.Converters;
    using Meadow.Library.Peripherals;
    using Meadow.Peripherals.Sensors.Atmospheric;
    using System;
    using System.Threading.Tasks;

    public class MeadowApp : App<F7Micro, MeadowApp>
    {
        private Bme280 bme280;

        private CharacterDisplay display;

        public MeadowApp()
        {
            OnboardLed led = new OnboardLed(Device);
            led.SetColor(RgbColor.Green);

            this.InitializeCharacterDisplay();
            this.InitializeBme280();
        }

        private void InitializeCharacterDisplay()
        {
            Console.WriteLine("Charter display initialized...");

            this.display = new CharacterDisplay(
                Device,
                pinRS: Device.Pins.D15,
                pinE: Device.Pins.D14,
                pinD4: Device.Pins.D13,
                pinD5: Device.Pins.D12,
                pinD6: Device.Pins.D11,
                pinD7: Device.Pins.D10,
                rows: 4, columns: 20    // Adjust dimensions to fit your display
            );

            this.display.Clear();

            this.display.WriteLine("Conditions", 0);
            this.display.WriteLine("Initializing...", 1);
        }

        private void InitializeBme280()
        {
            Console.WriteLine("Temperature (BME280) Initializing...");

            // configure our BME280 on the I2C Bus
            Meadow.Hardware.II2cBus i2c = Device.CreateI2cBus();
            this.bme280 = new Bme280(
                i2c,
                Bme280.I2cAddress.Adddress0x77 //default
            );

            this.bme280.Subscribe(new FilterableObserver<AtmosphericConditionChangeResult, AtmosphericConditions>(
                //h => Console.WriteLine($"Temp and pressure changed by threshold; new temp: {h.New.Temperature}, old: {h.Old.Temperature}"),
                h => this.OutputConditions(h.New),
                e => (Math.Abs(e.Delta.Temperature) > 0.2) || (Math.Abs(e.Delta.Pressure) > 5 || (Math.Abs(e.Delta.Humidity) > 0.1f))
            ));

            // classical .NET events can also be used:
            //this.bme280.Updated += this.AtmosphericConditionsChangedHandler;

            // get chip id
            Console.WriteLine($"ChipID: {this.bme280.GetChipID():X2}");

            // get an initial reading
            //this.ReadConditions().Wait();

            // start updating continuously
            this.bme280.StartUpdating(
                temperatureSampleCount: Bme280.Oversample.OversampleX2,
                pressureSampleCount: Bme280.Oversample.OversampleX16,
                humiditySampleCount: Bme280.Oversample.OversampleX1);
        }

        private void AtmosphericConditionsChangedHandler(object _, AtmosphericConditionChangeResult e)
        {
            this.OutputConditions(e.New);
        }

        private void OutputConditions(AtmosphericConditions conditions)
        {
            this.OutputConditionsToDisplay(conditions);

            this.OutputConditionsToConsole(conditions);
        }

        private void OutputConditionsToDisplay(AtmosphericConditions conditions)
        {
            float degreesCelsius = conditions.Temperature;
            float degreesFaranheit = Temperature.ConvertCelsiusToFahrenheit(conditions.Temperature);

            this.display.WriteLine($"Temp: {degreesCelsius:00.0} C {degreesFaranheit:00.0} F", 1);

            this.display.WriteLine($"Press: {conditions.Pressure:0.0} Pa", 2);
            this.display.WriteLine($"Humidity: {conditions.Humidity:00.0}%", 3);
        }

        private void OutputConditionsToConsole(AtmosphericConditions conditions)
        {
            Console.WriteLine("Atmospheric conditions:");
            Console.WriteLine($"  Temperature: {conditions.Temperature} deg C");
            Console.WriteLine($"  Pressure: {conditions.Pressure} Pa");
            Console.WriteLine($"  Relative Humidity: {conditions.Humidity}%");
        }

        protected async Task ReadConditions()
        {
            AtmosphericConditions conditions = await this.bme280.Read();
            this.OutputConditionsToConsole(conditions);
        }
    }
}
