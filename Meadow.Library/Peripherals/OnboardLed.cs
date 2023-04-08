namespace Meadow.Library.Peripherals;

using Meadow.Foundation;
using Meadow.Foundation.Leds;
using Meadow.Hardware;
using Meadow.Library.Extensions;
using Meadow.Peripherals.Leds;

/// <summary>
/// Represents the onboard LED for the F7 Feather.
/// Implements the <see cref="RgbPwmLed" />
/// </summary>
/// <seealso cref="RgbPwmLed" />
public class OnboardLed : RgbPwmLed
{
    public OnboardLed(IF7FeatherPinout pins)
        : base(pins.OnboardLedRed, pins.OnboardLedGreen, pins.OnboardLedBlue, CommonType.CommonAnode)
    {
    }

    public OnboardLed(IF7FeatherPinout pins, Color initialColor)
        : this(pins)
    {
        this.SetColor(initialColor);
    }

    /// <summary>
    /// Sets the color of the LED.
    /// </summary>
    /// <param name="color">The color.</param>
    /// <param name="brightness">The brightness.</param>
    public void SetColor(RgbLedColors color, float brightness = 1)
        => this.SetColor(color.ToColor(), brightness);
}
