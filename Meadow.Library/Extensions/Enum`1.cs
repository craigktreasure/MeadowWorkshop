namespace Meadow.Library
{
    using System;
    using System.Collections.Generic;

    public static class Enum<TEnum>
        where TEnum : Enum
    {
        /// <summary>
        /// Retrieves an array of the names of the constants in a specified enumeration.
        /// </summary>
        /// <returns><see cref="System.String[]"/>.</returns>
        public static string[] GetNames()
        {
            return Enum.GetNames(typeof(TEnum));
        }

        /// <summary>
        /// Returns the underlying type of the specified enumeration.
        /// </summary>
        /// <returns><see cref="Type"/>.</returns>
        public static Type GetUnderlyingType()
        {
            return Enum.GetUnderlyingType(typeof(TEnum));
        }

        /// <summary>
        /// Retrieves an array of the values of the constants in a specified enumeration.
        /// </summary>
        /// <returns><see cref="IReadOnlyList{TEnum}"/>.</returns>
        public static TEnum[] GetValues()
        {
            return (TEnum[])Enum.GetValues(typeof(TEnum));
        }
    }
}
