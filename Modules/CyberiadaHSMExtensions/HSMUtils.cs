using Godot;
using System;
using System.Collections.Generic;

public static class HSMUtils
{

    public static T UniversalParse<T>(string input) where T : notnull
    {
        if (string.IsNullOrWhiteSpace(input))
        {
            return default(T);
        }

        if (typeof(T) == typeof(string))
        {
            return (T)(object)input;
        }

        if (typeof(T) == typeof(int) && int.TryParse(input, out int intResult))
        {
            return (T)(object)intResult;
        }

        if (typeof(T) == typeof(float) && float.TryParse(input, out float floatResult))
        {
            return (T)(object)floatResult;
        }

        if (typeof(T) == typeof(double) && double.TryParse(input, out double doubleResult))
        {
            return (T)(object)doubleResult;
        }

        if (typeof(T) == typeof(decimal) && decimal.TryParse(input, out decimal decimalResult))
        {
            return (T)(object)decimalResult;
        }

        // Если тип не поддерживается или парсинг не удался
        return default(T); // Вернет 0
    }

    public static T GetValue<T>(Tuple<string, string> value) where T : notnull
    {
        return HSMUtils.UniversalParse<T>(value.Item2);
    }

}
