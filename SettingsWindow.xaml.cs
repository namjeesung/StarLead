using Microsoft.Win32;
using System.Reflection;
using System.Windows;

namespace StarLead;

public partial class SettingsWindow : Window
{
    private readonly Dictionary<string, bool> _visibilityBackup;
    private readonly double _originalOpacity;
    private readonly bool _originalShowNames;
    private readonly double _originalLinearWidth;
    private readonly string _originalLinearWidthMode;
    private readonly (bool Computer, bool Downloads, bool Recycle) _originalSystemVisibility;
    private bool _initialized;
    private bool _saved;
    public SettingsWindow()
    {
        _originalOpacity = App.Data.Settings.BackgroundOpacity;
        _originalShowNames = App.Data.Settings.ShowIconNames;
        _originalLinearWidth = App.Data.Settings.LinearPanelWidth;
        _originalLinearWidthMode = App.Data.Settings.LinearWidthMode;
        _originalSystemVisibility = (App.Data.Settings.ShowMyComputer, App.Data.Settings.ShowDownloads, App.Data.Settings.ShowRecycleBin);
        InitializeComponent(); var s = App.Data.Settings;
        ModifierBox.ItemsSource = new[] { LocalizationService.Get("None"), "Ctrl", "Alt", "Shift", "Ctrl+Alt", "Ctrl+Shift" };
        KeyBox.ItemsSource = new[] { "Space", "`", "-", "=", "[", "]", "\\", ";", "'", ",", ".", "/" }.Concat(Enumerable.Range(1, 12).Select(i => $"F{i}")).Concat("ABCDEFGHIJKLMNOPQRSTUVWXYZ".Select(c => c.ToString()));
        ModifierBox.SelectedItem = s.HotKeyModifier is "无" or "None" ? LocalizationService.Get("None") : s.HotKeyModifier; KeyBox.SelectedItem = s.HotKeyKey; StartupBox.IsChecked = s.StartWithWindows; LightRadio.IsChecked = s.Theme == "Light"; DarkRadio.IsChecked = s.Theme == "Dark";
        ChineseRadio.IsChecked = s.Language != "en-US"; EnglishRadio.IsChecked = s.Language == "en-US";
        LiquidGlassRadio.IsChecked = s.VisualStyle == "LiquidGlass"; OceanRadio.IsChecked = s.VisualStyle == "Ocean"; AuroraRadio.IsChecked = s.VisualStyle == "Aurora"; GraphiteRadio.IsChecked = s.VisualStyle == "Graphite";
        ShowNamesBox.IsChecked = s.ShowIconNames;
        KeyboardModeRadio.IsChecked = s.PanelMode != "Linear"; LinearModeRadio.IsChecked = s.PanelMode == "Linear";
        OpacitySlider.Value = s.BackgroundOpacity; UpdateLocalizedValues();
        LinearWidthSlider.Value = s.LinearPanelWidth;
        MyComputerBox.IsChecked = s.ShowMyComputer; DownloadsBox.IsChecked = s.ShowDownloads; RecycleBinBox.IsChecked = s.ShowRecycleBin;
        RememberPositionRadio.IsChecked = s.PositionMode == "Remember"; CenterPositionRadio.IsChecked = s.PositionMode != "Remember";
        var configurable = App.Data.Slots.ToList(); KeysList.ItemsSource = configurable; _visibilityBackup = configurable.ToDictionary(x => x.Key, x => x.IsVisible);
        _initialized = true;
    }
    private void Save_Click(object sender, RoutedEventArgs e)
    {
        var s = App.Data.Settings; s.Theme = DarkRadio.IsChecked == true ? "Dark" : "Light"; var selectedModifier = ModifierBox.SelectedItem?.ToString() ?? "Ctrl"; s.HotKeyModifier = selectedModifier is "无" or "None" ? "None" : selectedModifier; s.HotKeyKey = KeyBox.SelectedItem?.ToString() ?? "Space"; s.StartWithWindows = StartupBox.IsChecked == true; s.PositionMode = RememberPositionRadio.IsChecked == true ? "Remember" : "Center"; s.ShowIconNames = ShowNamesBox.IsChecked == true;
        s.PanelMode = LinearModeRadio.IsChecked == true ? "Linear" : "Keyboard"; s.BackgroundOpacity = OpacitySlider.Value; s.LinearPanelWidth = LinearWidthSlider.Value;
        s.ShowMyComputer = MyComputerBox.IsChecked == true; s.ShowDownloads = DownloadsBox.IsChecked == true; s.ShowRecycleBin = RecycleBinBox.IsChecked == true;
        ThemeService.Apply(s.Theme, s.VisualStyle); App.MainPanel.RefreshRows(); UpdateStartup(s.StartWithWindows); App.Data.SaveAll(); _saved = true; DialogResult = true;
    }
    private void Cancel_Click(object sender, RoutedEventArgs e) { DialogResult = false; }
    private void Theme_Checked(object sender, RoutedEventArgs e)
    {
        if (!_initialized) return;
        App.Data.Settings.Theme = DarkRadio.IsChecked == true ? "Dark" : "Light";
        ThemeService.Apply(App.Data.Settings.Theme, App.Data.Settings.VisualStyle); App.MainPanel.ApplyBackgroundOpacity(); App.Data.SaveAll();
    }
    private void VisualStyle_Checked(object sender, RoutedEventArgs e)
    {
        if (!_initialized) return;
        App.Data.Settings.VisualStyle = OceanRadio.IsChecked == true ? "Ocean" : AuroraRadio.IsChecked == true ? "Aurora" : GraphiteRadio.IsChecked == true ? "Graphite" : "LiquidGlass";
        ThemeService.Apply(App.Data.Settings.Theme, App.Data.Settings.VisualStyle);
        App.MainPanel.ApplyBackgroundOpacity();
        App.Data.SaveAll();
    }
    private void Language_Checked(object sender, RoutedEventArgs e)
    {
        if (!_initialized) return;
        var currentModifier = ModifierBox.SelectedItem?.ToString();
        App.Data.Settings.Language = EnglishRadio.IsChecked == true ? "en-US" : "zh-CN";
        LocalizationService.Apply(App.Data.Settings.Language);
        ModifierBox.ItemsSource = new[] { LocalizationService.Get("None"), "Ctrl", "Alt", "Shift", "Ctrl+Alt", "Ctrl+Shift" };
        ModifierBox.SelectedItem = currentModifier is "无" or "None" ? LocalizationService.Get("None") : currentModifier;
        UpdateLocalizedValues();
        ((App)Application.Current).RefreshLanguage();
        App.Data.SaveAll();
    }
    private void UpdateLocalizedValues()
    {
        OpacityText.Text = $"{App.Data.Settings.BackgroundOpacity:0}%（{LocalizationService.Get("OpaqueIcons")}）";
        LinearWidthText.Text = LocalizationService.Format("MaxWidthPreview", App.Data.Settings.LinearPanelWidth);
    }
    private void PanelMode_Checked(object sender, RoutedEventArgs e) { if (!_initialized) return; App.Data.Settings.PanelMode = LinearModeRadio.IsChecked == true ? "Linear" : "Keyboard"; App.MainPanel.ApplyPanelMode(); App.Data.SaveAll(); }
    private void OpacitySlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e) { if (!_initialized) return; App.Data.Settings.BackgroundOpacity = e.NewValue; UpdateLocalizedValues(); App.MainPanel.ApplyBackgroundOpacity(); }
    private void SystemComponent_Changed(object sender, RoutedEventArgs e) { if (!_initialized) return; var s = App.Data.Settings; s.ShowMyComputer = MyComputerBox.IsChecked == true; s.ShowDownloads = DownloadsBox.IsChecked == true; s.ShowRecycleBin = RecycleBinBox.IsChecked == true; App.MainPanel.ApplySystemVisibility(); App.MainPanel.RefreshRows(); App.MainPanel.RefreshLinearDisplay(); }
    private void ShowNames_Changed(object sender, RoutedEventArgs e) { if (!_initialized) return; App.Data.Settings.ShowIconNames = ShowNamesBox.IsChecked == true; App.MainPanel.RefreshRows(); App.MainPanel.RefreshLinearDisplay(); }
    private void LinearWidthSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e) { if (!_initialized) return; App.Data.Settings.LinearWidthMode = "Custom"; App.Data.Settings.LinearPanelWidth = e.NewValue; LinearWidthText.Text = LocalizationService.Format("MaxWidthPreview", e.NewValue); App.MainPanel.ApplyLinearWidth(); }
    protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
    {
        if (!_saved)
        {
            var s = App.Data.Settings; s.BackgroundOpacity = _originalOpacity; s.LinearPanelWidth = _originalLinearWidth; s.LinearWidthMode = _originalLinearWidthMode; s.ShowIconNames = _originalShowNames; s.ShowMyComputer = _originalSystemVisibility.Computer; s.ShowDownloads = _originalSystemVisibility.Downloads; s.ShowRecycleBin = _originalSystemVisibility.Recycle;
            App.MainPanel.ApplyPanelMode();
            App.MainPanel.RefreshRows(); App.MainPanel.RefreshLinearDisplay();
            foreach (var slot in App.Data.Slots) if (_visibilityBackup.TryGetValue(slot.Key, out var shown)) slot.IsVisible = shown;
        }
        base.OnClosing(e);
    }
    private static void UpdateStartup(bool enabled)
    {
        using var key = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Run", true); if (key == null) return;
        if (enabled) key.SetValue("StarLead", $"\"{Environment.ProcessPath}\" --startup"); else key.DeleteValue("StarLead", false);
    }
}
