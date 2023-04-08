namespace MeadowWorkshop;

using Meadow;
using Meadow.Devices;
using Meadow.Foundation;
using Meadow.Foundation.Leds;
using Meadow.Library;
using Meadow.Library.Peripherals;
using Meadow.Peripherals.Leds;
using System;
using System.Threading;
using System.Threading.Tasks;

public class MeadowApp : App<F7FeatherV1>
{
    public override Task Run()
    {
        //this.CycleLeds();
        //this.CycleLedsMeadowFoundation();
        this.CycleLedsMeadowFoundationPwm();
        //this.CycleLedsMeadowFoundationPwmNew();

        return base.Run();
    }

    public void CycleLeds()
    {
        const int wait = 200;

        OnboardLed led = new(Device.Pins);

        RgbLedColors currentColor = default;

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

        RgbLed led = new(
            redPin: Device.Pins.OnboardLedRed,
            greenPin: Device.Pins.OnboardLedGreen,
            bluePin: Device.Pins.OnboardLedBlue,
            CommonType.CommonAnode);

        RgbLedColors currentColor = default;

        while (true)
        {
            currentColor = currentColor.GetNext();

            if (currentColor == RgbLedColors.count)
            {
                // Skip RgbLedColors.count
                continue;
            }

            Console.WriteLine($"Setting color to {currentColor}.");
            led.SetColor(currentColor);
            Thread.Sleep(wait);
        }
    }

    private void CycleLedsMeadowFoundationPwm()
    {
        RgbPwmLed rgbPwmLed = new(
            redPwmPin: Device.Pins.OnboardLedRed,
            greenPwmPin: Device.Pins.OnboardLedGreen,
            bluePwmPin: Device.Pins.OnboardLedBlue,
            CommonType.CommonAnode);

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

    private void CycleLedsMeadowFoundationPwmNew()
    {
        TimeSpan duration = TimeSpan.FromMilliseconds(1000);
        RgbPwmLed rgbPwmLed = new(
            redPwmPin: Device.Pins.OnboardLedRed,
            greenPwmPin: Device.Pins.OnboardLedGreen,
            bluePwmPin: Device.Pins.OnboardLedBlue,
            CommonType.CommonAnode);

        void ShowColorPulse(Color color, TimeSpan duration)
        {
            rgbPwmLed.StartPulse(color, duration / 2);
            Thread.Sleep(duration);
            rgbPwmLed.Stop();
        }

        Console.WriteLine("Cycle colors...");

        while (true)
        {
            ShowColorPulse(Color.Blue, duration);
            ShowColorPulse(Color.Cyan, duration);
            ShowColorPulse(Color.Green, duration);
            ShowColorPulse(Color.GreenYellow, duration);
            ShowColorPulse(Color.Yellow, duration);
            ShowColorPulse(Color.Orange, duration);
            ShowColorPulse(Color.OrangeRed, duration);
            ShowColorPulse(Color.Red, duration);
            ShowColorPulse(Color.MediumVioletRed, duration);
            ShowColorPulse(Color.Purple, duration);
            ShowColorPulse(Color.Magenta, duration);
            ShowColorPulse(Color.Pink, duration);
        }
    }
}
