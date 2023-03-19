using System.Windows.Data;
using System;

namespace DiffDemo
{
    public enum DiffEntityTypes
    {
        Chars,
        Words,
        Lines
    }

    public class DiffEntityTypesBooleanConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
            => value?.Equals(parameter) ?? false; // or return parameter.Equals(YourEnumType.SomeDefaultValue);

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
            => value.Equals(true) ? parameter : Binding.DoNothing;
    }
}
