using Avalonia.Data.Converters;
using System;
using System.Globalization;

namespace SpriteAtlasForge.App.Converters;

public static class ObjectConverters
{
    public static readonly IValueConverter IsNull =
        new FuncValueConverter<object?, bool>(x => x is null);

    public static readonly IValueConverter IsNotNull =
        new FuncValueConverter<object?, bool>(x => x is not null);

    public static readonly IValueConverter BoolToVisibility =
        new FuncValueConverter<bool, bool>(x => x);

    public static readonly IValueConverter InverseBool =
        new FuncValueConverter<bool, bool>(x => !x);
}

public class EnumToStringConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is Enum enumValue)
        {
            return enumValue.ToString();
        }
        return value?.ToString();
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is string str && targetType.IsEnum)
        {
            return Enum.Parse(targetType, str);
        }
        return value;
    }
}
