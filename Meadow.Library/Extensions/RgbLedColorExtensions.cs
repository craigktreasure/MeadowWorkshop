namespace Meadow.Library.Extensions;

using Meadow.Foundation;
using Meadow.Peripherals.Leds;

public static class RgbLedColorExtensions
{
    /// <summary>
    /// Converts an <see cref="RgbLedColors" /> to a <see cref="Color" />.
    /// </summary>
    /// <param name="color">The color.</param>
    /// <returns><see cref="Color"/>.</returns>
    public static Color ToColor(this RgbLedColors color)
        => color switch
        {
            RgbLedColors.Black => Color.Black,
            RgbLedColors.Red => Color.Red,
            RgbLedColors.Green => Color.Green,
            RgbLedColors.Blue => Color.Blue,
            RgbLedColors.Yellow => Color.Yellow,
            RgbLedColors.Magenta => Color.Magenta,
            RgbLedColors.Cyan => Color.Cyan,
            RgbLedColors.White => Color.White,
            RgbLedColors.count => Color.Default,
            _ => Color.Default,
        };
}
