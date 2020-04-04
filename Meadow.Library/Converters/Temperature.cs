namespace Meadow.Library.Converters
{
    public static class Temperature
    {
        public static double ConvertCelsiusToFahrenheit(double c)
        {
            return ((9.0 / 5.0) * c) + 32;
        }

        public static float ConvertCelsiusToFahrenheit(float c)
        {
            return ((9.0f / 5.0f) * c) + 32;
        }

        public static double ConvertFahrenheitToCelsius(double f)
        {
            return (5.0 / 9.0) * (f - 32);
        }

        public static float ConvertFahrenheitToCelsius(float f)
        {
            return (5.0f / 9.0f) * (f - 32);
        }
    }
}
