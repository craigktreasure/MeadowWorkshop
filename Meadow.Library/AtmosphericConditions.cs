namespace Meadow.Library;

using Meadow.Units;

public readonly record struct AtmosphericConditions(Temperature? Temperature, RelativeHumidity? Humidity, Pressure? Pressure)
{
    public static implicit operator AtmosphericConditions((Temperature? Temperature, RelativeHumidity? Humidity, Pressure? Pressure) value)
    {
        return new AtmosphericConditions(value.Temperature, value.Humidity, value.Pressure);
    }
}
