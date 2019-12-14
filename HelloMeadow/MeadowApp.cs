namespace MeadowWorkshop
{
    using Meadow;
    using Meadow.Devices;
    using Meadow.Library;
    using Meadow.Library.Peripherals;
    using System;
    using System.Threading;

    public class MeadowApp : App<F7Micro, MeadowApp>
    {
        public MeadowApp()
        {
            this.CycleLeds();
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
    }
}
