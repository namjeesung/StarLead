using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace StarLead;

public static class ThemeService
{
    public static void Apply(string theme, string? visualStyle = null)
    {
        bool dark = theme.Equals("Dark", StringComparison.OrdinalIgnoreCase);
        var style = visualStyle ?? App.Data?.Settings.VisualStyle ?? "LiquidGlass";
        var palette = (style, dark) switch
        {
            ("Ocean", false) => ("#FFF4FAFF", "#EAF8FCFF", "#F9FFFFFF", "#FF101820", "#FF667781", "#294A7085", "#FF0A84FF", "#260A84FF"),
            ("Ocean", true) => ("#FF08141E", "#E8142633", "#F21C3444", "#FFF2FAFF", "#FF9EB4C2", "#42B5E7FF", "#FF40C8FF", "#3640C8FF"),
            ("Aurora", false) => ("#FFFFF7FC", "#ECFFF9FE", "#FAFFFFFF", "#FF20121F", "#FF826B7E", "#29A56B94", "#FFBF5AF2", "#2EBF5AF2"),
            ("Aurora", true) => ("#FF160B1B", "#E825142C", "#F235203D", "#FFFFF4FF", "#FFC8A7C8", "#4CFFB4EF", "#FFFF6BCB", "#3DFF6BCB"),
            ("Graphite", false) => ("#FFF4F4F5", "#EDF7F7F8", "#FBFFFFFF", "#FF171719", "#FF707075", "#2E4A4A50", "#FF343438", "#19343438"),
            ("Graphite", true) => ("#FF0B0B0C", "#ED19191B", "#F2242427", "#FFF7F7F8", "#FFA7A7AC", "#47FFFFFF", "#FFF2F2F4", "#2AFFFFFF"),
            (_, false) => ("#FFF5F7FC", "#EFFFFFFF", "#FAFFFFFF", "#FF171A24", "#FF6F7585", "#2444506B", "#FF6C63FF", "#286C63FF"),
            _ => ("#FF0E111A", "#EA1A1F2B", "#F2242A38", "#FFF4F6FF", "#FFA6ADC2", "#42FFFFFF", "#FF918BFF", "#38918BFF")
        };
        Set("WindowBrush", palette.Item1); Set("PanelBrush", palette.Item2); Set("CardBrush", palette.Item3); Set("TextBrush", palette.Item4);
        Set("MutedBrush", palette.Item5); Set("BorderBrush", palette.Item6); Set("AccentBrush", palette.Item7); Set("AccentSoftBrush", palette.Item8);
        var logoPath = dark ? "StarLead-mark-white.png" : "StarLead-mark-black.png";
        var logo = new BitmapImage();
        logo.BeginInit();
        logo.UriSource = new Uri($"pack://application:,,,/Assets/{logoPath}", UriKind.Absolute);
        logo.CacheOption = BitmapCacheOption.OnLoad;
        logo.EndInit();
        logo.Freeze();
        Application.Current.Resources["LogoAsset"] = logo;
    }
    private static void Set(string key, string color) => Application.Current.Resources[key] = new SolidColorBrush((Color)ColorConverter.ConvertFromString(color));
}
