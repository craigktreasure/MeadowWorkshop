namespace Meadow.Library.Peripherals
{
    using System;

    [Flags]
    public enum RgbColor : byte
    {
        None,
        Red,
        Blue,
        Magenta = Red | Blue,
        Green,
        Yellow = Red | Green,
        Cyan = Blue | Green,
        White = Red | Blue | Green,
    }
}
