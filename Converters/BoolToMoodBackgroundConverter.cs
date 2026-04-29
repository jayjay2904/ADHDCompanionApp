using System.Globalization;

namespace ADHDCompanionApp.Converters;

public class BoolToMoodBackgroundConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        var isSelected = value is bool selected && selected;

        return isSelected
            ? Color.FromArgb("#D4E6C6")
            : Colors.Transparent;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        => throw new NotImplementedException();
}