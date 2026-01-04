using System.Globalization;

namespace ProductionAnalysis.Application;

/// <summary>
/// Утилитный класс для сравнения значений различных типов
/// </summary>
public static class ValueComparer
{
    private const double Epsilon = 0.0001;

    public static bool AreEqual(object? value1, object? value2)
    {
        if (value1 == null && value2 == null)
        {
            return true;
        }

        if (value1 == null || value2 == null)
        {
            return false;
        }

        if (TryConvertToDouble(value1, out var num1) && TryConvertToDouble(value2, out var num2))
        {
            return Math.Abs(num1 - num2) < Epsilon;
        }

        return value1.Equals(value2);
    }

    public static bool AreDictionariesEqual(
        Dictionary<int, object> dict1,
        Dictionary<int, object> dict2)
    {
        if (dict1.Count != dict2.Count)
        {
            return false;
        }

        foreach (var (key, value1) in dict1)
        {
            if (!dict2.TryGetValue(key, out var value2))
            {
                return false;
            }

            if (!AreEqual(value1, value2))
            {
                return false;
            }
        }

        return true;
    }

    public static bool TryConvertToDouble(object value, out double result)
    {
        result = 0;

        return value switch
        {
            int i => (result = i, true).Item2,
            long l => (result = l, true).Item2,
            double d => (result = d, true).Item2,
            decimal dec => (result = (double)dec, true).Item2,
            float f => (result = f, true).Item2,
            string s => double.TryParse(s, NumberStyles.Any, CultureInfo.InvariantCulture, out result),
            _ => false
        };
    }
}