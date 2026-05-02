using System.Globalization;

namespace ADHDCompanionApp.Helpers;

public class ArloPromptColourConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        var prompt = value?.ToString()?.ToLowerInvariant() ?? string.Empty;

        if (prompt.Contains("overwhelmed"))
            return Color.FromArgb("#D4E6C6");

        if (prompt.Contains("start"))
            return Color.FromArgb("#F3E2B8");

        if (prompt.Contains("low energy") || prompt.Contains("tired"))
            return Color.FromArgb("#D7E7F3");

        if (prompt.Contains("anxious"))
            return Color.FromArgb("#E4D8F2");

        if (prompt.Contains("avoiding"))
            return Color.FromArgb("#F6D4C8");

        if (prompt.Contains("too much"))
            return Color.FromArgb("#CFE8E4");

        if (prompt.Contains("stuck"))
            return Color.FromArgb("#E3D7F4");

        if (prompt.Contains("failed"))
            return Color.FromArgb("#F4D6DD");

        return Color.FromArgb("#D4E6C6");
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}