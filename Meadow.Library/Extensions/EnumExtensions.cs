namespace Meadow.Library
{
    using System;

    public static class EnumExtensions
    {
        public static TEnum GetNext<TEnum>(this TEnum current)
            where TEnum : Enum
        {
            if (!Enum.IsDefined(typeof(TEnum), current))
            {
                throw new ArgumentOutOfRangeException(nameof(current), $"The value specified is not defined in the enum ({typeof(TEnum).FullName}): '{current}'.");
            }

            TEnum[] values = Enum<TEnum>.GetValues();

            if (values.Length == 0)
            {
                throw new InvalidOperationException($"The enum type has no values: '{typeof(TEnum).FullName}'");
            }

            int nextIndex = Array.IndexOf(values, current) + 1;
            return nextIndex >= values.Length ? values[0] : values[nextIndex];
        }
    }
}
