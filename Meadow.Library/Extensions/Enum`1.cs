namespace Meadow.Library;

using System;
using System.Linq;

public static class Enum<TEnum>
    where TEnum : Enum
{
    /// <summary>
    /// Retrieves an array of the names of the constants in a specified enumeration.
    /// </summary>
    /// <returns><see cref="string[]"/>.</returns>
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
    /// <returns><see cref="TEnum[]"/>.</returns>
    public static TEnum[] GetValues()
    {
        return (TEnum[])Enum.GetValues(typeof(TEnum));
    }

    /// <summary>
    /// Retrieves an array of the values of the constants in a specified enumeration.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <returns><see cref="T[]"/>.</returns>
    public static T[] GetValues<T>()
    {
        return Enum.GetValues(typeof(TEnum)).Cast<T>().ToArray();
    }
}
