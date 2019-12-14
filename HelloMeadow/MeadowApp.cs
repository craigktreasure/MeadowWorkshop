namespace MeadowWorkshop
{
    using Meadow;
    using Meadow.Devices;
    using Meadow.Foundation.Leds;
    using Meadow.Library;
    using Meadow.Library.Peripherals;
    using System;
    using System.Threading;

    public class MeadowApp : App<F7Micro, MeadowApp>
    {
        public MeadowApp()
        {
            //this.CycleLeds();
            this.CycleLedsMeadowFoundation();
        }

        public void CycleLeds()
        {
            const int wait = 200;

            using OnboardLed led = new OnboardLed(Device);

            RgbColor currentColor = RgbColor.None;

            while (true)
            {
                currentColor = currentColor.GetNext();
                Console.WriteLine($"Setting color to {currentColor}.");
                led.SetColor(currentColor);
                Thread.Sleep(wait);
            }
        }

        private void CycleLedsMeadowFoundation()
        {
            const int wait = 200;

            RgbLed led = new RgbLed(Device,
                redPin: Device.Pins.OnboardLedRed,
                greenPin: Device.Pins.OnboardLedGreen,
                bluePin: Device.Pins.OnboardLedBlue);

            RgbLed.Colors currentColor = RgbLed.Colors.count;

            while (true)
            {
                do
                {
                    currentColor = currentColor.GetNext();
                }
                while (currentColor == RgbLed.Colors.count); // Skip RgbLed.Colors.count

                Console.WriteLine($"Setting color to {currentColor}.");
                led.SetColor(currentColor);
                Thread.Sleep(wait);
            }
        }
    }
}
