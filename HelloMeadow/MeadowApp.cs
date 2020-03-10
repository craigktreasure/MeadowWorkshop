namespace MeadowWorkshop
{
    using Meadow;
    using Meadow.Devices;
    using Meadow.Foundation;
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
            //this.CycleLedsMeadowFoundation();
            this.CycleLedsMeadowFoundationPwm();
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

        private void CycleLedsMeadowFoundationPwm()
        {
            RgbPwmLed rgbPwmLed = new RgbPwmLed(Device,
                redPwmPin: Device.Pins.OnboardLedRed,
                greenPwmPin: Device.Pins.OnboardLedGreen,
                bluePwmPin: Device.Pins.OnboardLedBlue);

            // alternate between blinking and pulsing the LED
            while (true)
            {
                for (int i = 0; i < 360; i++)
                {
                    double hue = ((double)i / 360F);

                    // set the color of the RGB
                    rgbPwmLed.SetColor(Color.FromHsba(hue, 1, 1));

                    // for a fun, fast rotation through the hue spectrum:
                    //Thread.Sleep(1);
                    // for a gentle walk through the forest of colors;
                    Thread.Sleep(18);
                }
            }
        }
    }
}
