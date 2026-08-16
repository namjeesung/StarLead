using Microsoft.Win32;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using System.Windows.Media.Animation;
using System.Windows.Media;
using System.Windows.Media.Effects;
using System.Runtime.InteropServices;
using System.Windows.Interop;

namespace StarLead;

public partial class MainWindow : Window
{
    [DllImport("shell32.dll", CharSet = CharSet.Unicode)]
    private static extern int SHEmptyRecycleBin(IntPtr hwnd, string? rootPath, uint flags);
    [DllImport("shell32.dll", CharSet = CharSet.Unicode, PreserveSig = true)]
    private static extern int SHQueryRecycleBin(string? rootPath, ref RecycleBinInfo info);

    [StructLayout(LayoutKind.Sequential, Pack = 8)]
    private struct RecycleBinInfo
    {
        public uint Size;
        public long TotalSize;
        public long ItemCount;
    }
    private readonly HotKeyService _hotKey = new();
    private System.Windows.Point _dragStart;
    private DateTime _ignoreClicksUntil;
    private ActionSlot? _internalDragSource;
    private Button? _internalDragButton;
    private Button? _dragHoverButton;
    private bool _isInternalDragging;
    private bool _linearDragSource;
    private int _linearHighlightedIndex = -1;
    private double _linearScrollTarget;
    private List<ActionSlot> _linearDisplayItems = [];

    public MainWindow()
    {
        InitializeComponent();
        CompositionTarget.Rendering += SmoothLinearScroll;
        SourceInitialized += (_, _) => RegisterHotKey();
        Loaded += async (_, _) => { Topmost = App.Data.Settings.AlwaysOnTop; RefreshRows(); RefreshLinearItems(); UpdateQuickControls(); ApplyPanelMode(); await Task.WhenAll(LoadAllIconsAsync(), LoadFixedEntryIconsAsync(), LoadLinearIconsAsync()); };
        _hotKey.Pressed += TogglePanel;
    }

    public void RegisterHotKey() => _hotKey.Register(this, App.Data.Settings);
    public void DisposeHotKey() => _hotKey.Dispose();
    public async void TogglePanel()
    {
        if (IsVisible) Hide();
        else
        {
            PlacePanel();
            Show();
            Activate();
            await RefreshRecycleBinIconAsync();
        }
    }
    private void CenterOnPrimary() { var area = SystemParameters.WorkArea; Left = area.Left + (area.Width - Width) / 2; Top = area.Top + (area.Height - Height) / 2; }
    private void PlacePanel()
    {
        var s = App.Data.Settings;
        var linear = s.PanelMode == "Linear";
        var storedLeft = linear ? s.LinearLeft : s.PanelLeft;
        var storedTop = linear ? s.LinearTop : s.PanelTop;
        if (s.PositionMode == "Remember" && storedLeft is double savedLeft && storedTop is double savedTop && double.IsFinite(savedLeft) && double.IsFinite(savedTop))
        {
            var area = SystemParameters.WorkArea;
            Left = Math.Clamp(savedLeft, area.Left, Math.Max(area.Left, area.Right - Width));
            Top = Math.Clamp(savedTop, area.Top, Math.Max(area.Top, area.Bottom - Height));
        }
        else CenterOnPrimary();
    }

    public void ApplyPanelMode()
    {
        var linear = App.Data.Settings.PanelMode == "Linear";
        PanelShell.Visibility = linear ? Visibility.Collapsed : Visibility.Visible;
        LinearShell.Visibility = linear ? Visibility.Visible : Visibility.Collapsed;
        if (linear) { Height = 104; Width = CalculateLinearWidth(); RefreshLinearItems(); }
        else { Width = 1120; Height = 700; }
        ApplySystemVisibility(); ApplyBackgroundOpacity(); PlacePanel();
    }
    public void ApplyBackgroundOpacity()
    {
        var opacity = Math.Clamp(App.Data.Settings.BackgroundOpacity / 100.0, 0, 1);
        var source = (FindResource("PanelBrush") as SolidColorBrush)?.Color ?? Colors.White;
        var brush = new SolidColorBrush(Color.FromArgb((byte)Math.Round(255 * opacity), source.R, source.G, source.B));
        PanelShell.Background = brush; LinearShell.Background = brush;
        if (opacity <= 0)
        {
            PanelShell.BorderThickness = new Thickness(0);
            PanelShell.Effect = null;
            LinearShell.BorderThickness = new Thickness(0);
            LinearShell.Effect = null;
        }
        else
        {
            PanelShell.BorderThickness = new Thickness(1);
            PanelShell.SetResourceReference(Border.BorderBrushProperty, "BorderBrush");
            PanelShell.Effect = new DropShadowEffect { BlurRadius = 28, ShadowDepth = 6, Opacity = 0.26 };
            LinearShell.BorderThickness = new Thickness(1);
            LinearShell.SetResourceReference(Border.BorderBrushProperty, "BorderBrush");
            LinearShell.Effect = new DropShadowEffect { BlurRadius = 22, ShadowDepth = 4, Opacity = 0.22 };
        }
    }
    public void ApplySystemVisibility()
    {
        var s = App.Data.Settings;
        MyComputerButton.Visibility = s.ShowMyComputer ? Visibility.Visible : Visibility.Collapsed;
        DownloadsButton.Visibility = s.ShowDownloads ? Visibility.Visible : Visibility.Collapsed;
        RecycleBinButton.Visibility = s.ShowRecycleBin ? Visibility.Visible : Visibility.Collapsed;
        SystemEntriesPanel.Visibility = s.ShowMyComputer || s.ShowDownloads || s.ShowRecycleBin ? Visibility.Visible : Visibility.Collapsed;
    }

    public void RefreshRows()
    {
        foreach (var slot in App.Data.Slots) slot.ShowLabel = App.Data.Settings.ShowIconNames;
        var slots = App.Data.Slots.Where(s => s.IsVisible).ToList();
        foreach (var slot in slots.Where(s => s.Kind == TargetKind.Notebook)) slot.DisplayName = LocalizationService.Get("Notebook");
        Row1.ItemsSource = slots.Where(s => "1234567890".Contains(s.Key)).OrderBy(s => "1234567890".IndexOf(s.Key));
        Row2.ItemsSource = slots.Where(s => "QWERTYUIOP".Contains(s.Key)).OrderBy(s => "QWERTYUIOP".IndexOf(s.Key));
        Row3.ItemsSource = slots.Where(s => "ASDFGHJKL".Contains(s.Key)).OrderBy(s => "ASDFGHJKL".IndexOf(s.Key));
        Row4.ItemsSource = slots.Where(s => "ZXCVBNM".Contains(s.Key)).OrderBy(s => "ZXCVBNM".IndexOf(s.Key));
    }
    private void UpdateQuickControls()
    {
        NameToggleButton.ToolTip = LocalizationService.Get(App.Data.Settings.ShowIconNames ? "NameOn" : "NameOff");
        NameToggleButton.Background = App.Data.Settings.ShowIconNames ? (System.Windows.Media.Brush)FindResource("AccentSoftBrush") : (System.Windows.Media.Brush)FindResource("CardBrush");
        PinButton.ToolTip = LocalizationService.Get(App.Data.Settings.AlwaysOnTop ? "PinOn" : "PinOff");
        PinButton.Background = App.Data.Settings.AlwaysOnTop ? (System.Windows.Media.Brush)FindResource("AccentSoftBrush") : (System.Windows.Media.Brush)FindResource("CardBrush");
    }

    private async Task LoadAllIconsAsync() { foreach (var slot in App.Data.Slots.Where(s => !s.IsEmpty)) await LoadIconAsync(slot); }
    private async Task LoadLinearIconsAsync() { foreach (var item in App.Data.LinearItems) await LoadIconAsync(item); }
    private void RefreshLinearItems()
    {
        App.Data.RefreshLinearKeys();
        _linearDisplayItems = App.Data.LinearItems.ToList();
        for (var i = 0; i < _linearDisplayItems.Count; i++) _linearDisplayItems[i].Key = LinearKeyAt(i);
        foreach (var item in App.Data.LinearItems) item.ShowLabel = App.Data.Settings.ShowIconNames;
        foreach (var item in App.Data.LinearItems.Where(s => s.Kind == TargetKind.Notebook)) item.DisplayName = LocalizationService.Get("Notebook");
        var halfSpacing = Math.Clamp(App.Data.Settings.LinearItemSpacing, 0, 40) / 2;
        foreach (var item in App.Data.LinearItems) item.LinearMargin = new Thickness(halfSpacing, 0, halfSpacing, 0);
        if (_linearHighlightedIndex >= _linearDisplayItems.Count) _linearHighlightedIndex = _linearDisplayItems.Count - 1;
        for (var i = 0; i < _linearDisplayItems.Count; i++) _linearDisplayItems[i].IsHighlighted = i == _linearHighlightedIndex;
        LinearItemsControl.ItemsSource = null; LinearItemsControl.ItemsSource = _linearDisplayItems;
        if (App.Data.Settings.PanelMode == "Linear") Width = CalculateLinearWidth();
    }
    private static string LinearKeyAt(int index) => index < 9 ? (index + 1).ToString() : index == 9 ? "0" : (index + 1).ToString();
    private double CalculateLinearWidth()
    {
        var itemExtent = 80 + Math.Clamp(App.Data.Settings.LinearItemSpacing, 0, 40);
        var displayCount = App.Data.LinearItems.Count;
        var contentWidth = Math.Max(104, 20 + displayCount * itemExtent);
        var screenLimit = Math.Max(200, SystemParameters.WorkArea.Width - 40);
        var chosenLimit = Math.Clamp(App.Data.Settings.LinearPanelWidth, 200, Math.Min(1800, screenLimit));
        return App.Data.Settings.LinearWidthMode == "Custom" ? chosenLimit : Math.Min(contentWidth, screenLimit);
    }
    public void ApplyLinearWidth()
    {
        if (App.Data.Settings.PanelMode != "Linear") return;
        Width = CalculateLinearWidth(); PlacePanel();
    }
    public void RefreshLinearDisplay() => RefreshLinearItems();
    private void SmoothLinearScroll(object? sender, EventArgs e)
    {
        if (LinearShell.Visibility != Visibility.Visible) return;
        _linearScrollTarget = Math.Clamp(_linearScrollTarget, 0, LinearScroll.ScrollableWidth);
        var current = LinearScroll.HorizontalOffset; var delta = _linearScrollTarget - current;
        if (Math.Abs(delta) < 0.35) { if (Math.Abs(delta) > 0) LinearScroll.ScrollToHorizontalOffset(_linearScrollTarget); return; }
        LinearScroll.ScrollToHorizontalOffset(current + delta * 0.18);
    }
    private async Task LoadFixedEntryIconsAsync()
    {
        MyComputerIcon.Source = await IconHelper.LoadAsync("::{20D04FE0-3AEA-1069-A2D8-08002B30309D}", false);
        DownloadsIcon.Source = await IconHelper.LoadAsync(KnownFolders.Downloads, true);
        await RefreshRecycleBinIconAsync();
        var notepadPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.System), "notepad.exe");
        var notebookIcon = await IconHelper.LoadAsync(notepadPath, false);
        foreach (var slot in App.Data.Slots.Concat(App.Data.LinearItems).Where(s => s.Kind == TargetKind.Notebook)) slot.Icon = notebookIcon;
    }
    private async Task LoadIconAsync(ActionSlot slot)
    {
        if (string.IsNullOrWhiteSpace(slot.TargetPath)) return;
        slot.Icon = await IconHelper.LoadAsync(slot.TargetPath, slot.Kind == TargetKind.Folder);
    }

    private void Slot_Click(object sender, RoutedEventArgs e)
    {
        // PreviewMouseLeftButtonUp 会在 Click 之前完成拖拽判断。
        // 若刚发生过拖拽，短暂拦截 Click，避免交换键位后误开程序。
        if (DateTime.UtcNow < _ignoreClicksUntil) return;
        if (sender is not Button { DataContext: ActionSlot slot } || slot.IsEmpty) return;
        LaunchSlot(slot);
    }
    private void LaunchSlot(ActionSlot slot)
    {
        try
        {
            if (slot.Kind == TargetKind.Notebook) { Hide(); App.ShowNotebook(); return; }
            if (slot.Kind == TargetKind.RecycleBin)
                Process.Start(new ProcessStartInfo("explorer.exe", "shell:RecycleBinFolder") { UseShellExecute = true });
            else
                Process.Start(new ProcessStartInfo(slot.TargetPath!) { UseShellExecute = true });
            Hide();
        }
        catch (Exception ex) { MessageBox.Show(LocalizationService.Get("CannotOpen") + ex.Message, LocalizationService.Get("AppName")); }
    }

    private void Slot_DoubleClick(object sender, MouseButtonEventArgs e)
    {
        if (DateTime.UtcNow < _ignoreClicksUntil || sender is not Button { DataContext: ActionSlot slot }) return;
        e.Handled = true;
        if (!slot.IsEmpty) { LaunchSlot(slot); return; }
        var menu = new ContextMenu();
        menu.Items.Add(MenuItem(LocalizationService.Get("BindProgram"), () => BindFile(slot, "Programs (*.exe)|*.exe", TargetKind.Program)));
        menu.Items.Add(MenuItem(LocalizationService.Get("BindFile"), () => BindFile(slot, "All files (*.*)|*.*", TargetKind.File)));
        menu.Items.Add(MenuItem(LocalizationService.Get("BindFolder"), () => BindFolder(slot)));
        menu.Items.Add(MenuItem(LocalizationService.Get("BindNotebook"), () => BindNotebook(slot)));
        menu.PlacementTarget = (Button)sender; HoldWhileMenuOpen(menu); menu.IsOpen = true;
    }
    private void Slot_RightClick(object sender, MouseButtonEventArgs e)
    {
        if (sender is not Button { DataContext: ActionSlot slot } button || slot.IsEmpty) return;
        e.Handled = true; var menu = new ContextMenu();
        menu.Items.Add(MenuItem(LocalizationService.Get("Open"), () => LaunchSlot(slot)));
        menu.Items.Add(MenuItem(LocalizationService.Get("RebindProgram"), () => BindFile(slot, "Programs (*.exe)|*.exe", TargetKind.Program)));
        menu.Items.Add(MenuItem(LocalizationService.Get("RebindFile"), () => BindFile(slot, "All files (*.*)|*.*", TargetKind.File)));
        menu.Items.Add(MenuItem(LocalizationService.Get("RebindFolder"), () => BindFolder(slot)));
        menu.Items.Add(MenuItem(LocalizationService.Get("RebindNotebook"), () => BindNotebook(slot)));
        menu.Items.Add(new Separator());
        menu.Items.Add(MenuItem(LocalizationService.Get("DeleteBinding"), () => { slot.Kind = TargetKind.None; slot.TargetPath = null; slot.DisplayName = null; slot.Icon = null; App.Data.SaveAll(); }));
        menu.PlacementTarget = button; HoldWhileMenuOpen(menu); menu.IsOpen = true;
    }
    private void HoldWhileMenuOpen(ContextMenu menu) { menu.Closed += (_, _) => { if (IsVisible) Activate(); }; }
    private static MenuItem MenuItem(string text, Action action)
    {
        var label = new TextBlock
        {
            Text = text,
            Foreground = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(17, 24, 39)),
            FontFamily = new System.Windows.Media.FontFamily("Microsoft YaHei UI"),
            FontSize = 13,
            FontWeight = FontWeights.SemiBold
        };
        var item = new MenuItem { Header = label, Foreground = label.Foreground, Background = System.Windows.Media.Brushes.White };
        item.Click += (_, _) => action();
        return item;
    }
    private async void BindFile(ActionSlot slot, string filter, TargetKind kind)
    {
        var dialog = new OpenFileDialog { Filter = filter, CheckFileExists = true };
        if (dialog.ShowDialog(this) == true) { slot.TargetPath = dialog.FileName; slot.DisplayName = Path.GetFileNameWithoutExtension(dialog.FileName); slot.Kind = kind; await LoadIconAsync(slot); App.Data.SaveAll(); }
        Activate();
    }
    private async void BindFolder(ActionSlot slot)
    {
        var dialog = new OpenFolderDialog { Multiselect = false };
        if (dialog.ShowDialog(this) == true) { slot.TargetPath = dialog.FolderName; slot.DisplayName = new DirectoryInfo(dialog.FolderName).Name; slot.Kind = TargetKind.Folder; await LoadIconAsync(slot); App.Data.SaveAll(); }
        Activate();
    }
    private async void BindNotebook(ActionSlot slot)
    {
        slot.TargetPath = null; slot.DisplayName = LocalizationService.Get("Notebook"); slot.Kind = TargetKind.Notebook;
        var notepadPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.System), "notepad.exe");
        slot.Icon = await IconHelper.LoadAsync(notepadPath, false);
        App.Data.SaveAll(); Activate();
    }

    private void Slot_MouseDown(object sender, MouseButtonEventArgs e)
    {
        if (sender is not Button { DataContext: ActionSlot slot } button) return;
        _linearDragSource = false;
        _dragStart = e.GetPosition(this); _internalDragSource = slot; _internalDragButton = button; _isInternalDragging = false;
    }
    private void LinearItem_MouseDown(object sender, MouseButtonEventArgs e)
    {
        if (sender is not Button { DataContext: ActionSlot slot } button) return;
        _linearDragSource = true; _dragStart = e.GetPosition(this); _internalDragSource = slot; _internalDragButton = button; _isInternalDragging = false;
    }
    private void LinearItem_Click(object sender, RoutedEventArgs e)
    {
        if (DateTime.UtcNow < _ignoreClicksUntil) return;
        if (sender is Button { DataContext: ActionSlot item }) LaunchSlot(item);
    }
    private void LinearItem_RightClick(object sender, MouseButtonEventArgs e)
    {
        if (sender is not Button { DataContext: ActionSlot item } button) return;
        e.Handled = true; var menu = new ContextMenu();
        menu.Items.Add(MenuItem(LocalizationService.Get("Open"), () => LaunchSlot(item)));
        menu.Items.Add(MenuItem(LocalizationService.Get("Delete"), () => { App.Data.LinearItems.Remove(item); RefreshLinearItems(); App.Data.SaveAll(); }));
        menu.PlacementTarget = button; HoldWhileMenuOpen(menu); menu.IsOpen = true;
    }
    private void LinearShell_MouseWheel(object sender, MouseWheelEventArgs e)
    {
        if (_linearDisplayItems.Count == 0) return;
        if (_linearHighlightedIndex < 0) _linearHighlightedIndex = e.Delta < 0 ? 0 : _linearDisplayItems.Count - 1;
        else _linearHighlightedIndex = Math.Clamp(_linearHighlightedIndex + (e.Delta < 0 ? 1 : -1), 0, _linearDisplayItems.Count - 1);
        for (var i = 0; i < _linearDisplayItems.Count; i++) _linearDisplayItems[i].IsHighlighted = i == _linearHighlightedIndex;
        var itemExtent = 80 + Math.Clamp(App.Data.Settings.LinearItemSpacing, 0, 40);
        var centeredOffset = _linearHighlightedIndex * itemExtent - (LinearScroll.ViewportWidth - itemExtent) / 2;
        _linearScrollTarget = Math.Clamp(centeredOffset, 0, LinearScroll.ScrollableWidth);
        e.Handled = true;
    }
    private void LinearRightResize_DragDelta(object sender, System.Windows.Controls.Primitives.DragDeltaEventArgs e) => ResizeLinearFromEdge(e.HorizontalChange, false);
    private void LinearLeftResize_DragDelta(object sender, System.Windows.Controls.Primitives.DragDeltaEventArgs e) => ResizeLinearFromEdge(e.HorizontalChange, true);
    private void ResizeLinearFromEdge(double horizontalChange, bool fromLeft)
    {
        var oldWidth = Width;
        var desired = fromLeft ? oldWidth - horizontalChange : oldWidth + horizontalChange;
        var newWidth = Math.Clamp(desired, 200, Math.Max(200, Math.Min(1800, SystemParameters.WorkArea.Width - 40)));
        if (fromLeft) Left += oldWidth - newWidth;
        App.Data.Settings.LinearWidthMode = "Custom"; App.Data.Settings.LinearPanelWidth = newWidth;
        Width = newWidth; App.Data.SaveAll();
    }
    private void LinearShell_RightClick(object sender, MouseButtonEventArgs e)
    {
        if (FindButtonAncestor(e.OriginalSource as DependencyObject)?.DataContext is ActionSlot) return;
        e.Handled = true; var menu = new ContextMenu();
        menu.Items.Add(MenuItem(LocalizationService.Get("KeyboardPanel"), () => SetPanelMode("Keyboard")));
        menu.Items.Add(MenuItem(LocalizationService.Get("Settings"), () => Settings_Click(this, new RoutedEventArgs())));
        menu.Items.Add(new Separator());
        menu.Items.Add(MenuItem((App.Data.Settings.LinearWidthMode == "Auto" ? "✓ " : "") + LocalizationService.Get("WidthAuto"), () => { App.Data.Settings.LinearWidthMode = "Auto"; ApplyLinearWidth(); App.Data.SaveAll(); }));
        menu.Items.Add(MenuItem((App.Data.Settings.LinearWidthMode == "Custom" ? "✓ " : "") + LocalizationService.Get("WidthCustom"), () => { App.Data.Settings.LinearWidthMode = "Custom"; App.Data.Settings.LinearPanelWidth = Width; ApplyLinearWidth(); App.Data.SaveAll(); }));
        menu.Items.Add(new Separator());
        var spacingPanel = new StackPanel { Width = 230 };
        var spacingText = new TextBlock { Text = LocalizationService.Format("IconSpacing", App.Data.Settings.LinearItemSpacing), Foreground = new SolidColorBrush(Color.FromRgb(17, 24, 39)), FontFamily = new FontFamily("Microsoft YaHei UI"), FontWeight = FontWeights.SemiBold };
        var slider = new Slider { Minimum = 0, Maximum = 40, Value = App.Data.Settings.LinearItemSpacing, TickFrequency = 1, Margin = new Thickness(0, 8, 0, 2) };
        spacingPanel.Children.Add(spacingText); spacingPanel.Children.Add(slider);
        var spacingItem = new MenuItem { Header = spacingPanel, StaysOpenOnClick = true, Background = Brushes.White };
        slider.ValueChanged += (_, args) => { App.Data.Settings.LinearItemSpacing = args.NewValue; spacingText.Text = LocalizationService.Format("IconSpacing", args.NewValue); RefreshLinearItems(); App.Data.SaveAll(); };
        menu.Items.Add(spacingItem); menu.PlacementTarget = LinearShell; HoldWhileMenuOpen(menu); menu.IsOpen = true;
    }
    private void LinearShell_DragOver(object sender, DragEventArgs e) { e.Effects = e.Data.GetDataPresent(DataFormats.FileDrop) ? DragDropEffects.Copy : DragDropEffects.None; e.Handled = true; }
    private async void LinearShell_Drop(object sender, DragEventArgs e)
    {
        if (!e.Data.GetDataPresent(DataFormats.FileDrop) || e.Data.GetData(DataFormats.FileDrop) is not string[] paths) return;
        foreach (var path in paths.Where(p => File.Exists(p) || Directory.Exists(p)))
        {
            var item = new ActionSlot { TargetPath = path, Kind = Directory.Exists(path) ? TargetKind.Folder : Path.GetExtension(path).Equals(".exe", StringComparison.OrdinalIgnoreCase) ? TargetKind.Program : TargetKind.File, DisplayName = Directory.Exists(path) ? new DirectoryInfo(path).Name : Path.GetFileNameWithoutExtension(path) };
            App.Data.LinearItems.Add(item); await LoadIconAsync(item);
        }
        RefreshLinearItems(); App.Data.SaveAll(); Activate(); e.Handled = true;
    }
    private void Slot_Drop(object sender, System.Windows.DragEventArgs e)
    {
        if (sender is not Button { DataContext: ActionSlot target }) return;
        if (e.Data.GetDataPresent(DataFormats.FileDrop) && e.Data.GetData(DataFormats.FileDrop) is string[] { Length: > 0 } paths)
        { BindDroppedPath(target, paths[0]); Activate(); e.Effects = DragDropEffects.Copy; e.Handled = true; }
    }
    private void Slot_DragOver(object sender, System.Windows.DragEventArgs e)
    {
        e.Effects = e.Data.GetDataPresent(DataFormats.FileDrop) ? DragDropEffects.Copy : DragDropEffects.None;
        e.Handled = true;
    }
    private async void BindDroppedPath(ActionSlot slot, string path)
    {
        if (!File.Exists(path) && !Directory.Exists(path)) return;
        slot.TargetPath = path;
        slot.Kind = Directory.Exists(path) ? TargetKind.Folder : Path.GetExtension(path).Equals(".exe", StringComparison.OrdinalIgnoreCase) ? TargetKind.Program : TargetKind.File;
        slot.DisplayName = Directory.Exists(path) ? new DirectoryInfo(path).Name : Path.GetFileNameWithoutExtension(path);
        await LoadIconAsync(slot); App.Data.SaveAll();
    }

    private void Window_PreviewKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
    {
        if (e.Key == Key.Escape) { e.Handled = true; Hide(); return; }
        if (e.IsRepeat || Keyboard.Modifiers != ModifierKeys.None) return;
        string? keyName = e.Key switch
        {
            >= Key.A and <= Key.Z => e.Key.ToString(),
            >= Key.D0 and <= Key.D9 => ((int)e.Key - (int)Key.D0).ToString(),
            >= Key.NumPad0 and <= Key.NumPad9 => ((int)e.Key - (int)Key.NumPad0).ToString(),
            _ => null
        };
        if (keyName == null) return;
        var slot = App.Data.Settings.PanelMode == "Linear"
            ? _linearDisplayItems.Take(10).FirstOrDefault(s => s.Key == keyName)
            : App.Data.Slots.FirstOrDefault(s => s.Key == keyName && s.IsVisible && !s.IsEmpty);
        if (slot == null) return;
        e.Handled = true; LaunchSlot(slot);
    }
    private void Settings_Click(object sender, RoutedEventArgs e) { new SettingsWindow { Owner = this }.ShowDialog(); RefreshRows(); UpdateQuickControls(); ApplyPanelMode(); RegisterHotKey(); Activate(); }
    private void KeyboardMode_Click(object sender, RoutedEventArgs e) => SetPanelMode("Keyboard");
    private void LinearMode_Click(object sender, RoutedEventArgs e) => SetPanelMode("Linear");
    private void SetPanelMode(string mode)
    {
        if (App.Data.Settings.PanelMode == mode) return;
        App.Data.Settings.PanelMode = mode;
        ApplyPanelMode();
        App.Data.SaveAll();
        Activate();
    }
    public void ApplyLanguage() { RefreshRows(); RefreshLinearItems(); UpdateQuickControls(); }
    private void NameToggle_Click(object sender, RoutedEventArgs e)
    {
        App.Data.Settings.ShowIconNames = !App.Data.Settings.ShowIconNames; RefreshRows(); RefreshLinearItems(); UpdateQuickControls(); App.Data.SaveAll();
    }
    private void Pin_Click(object sender, RoutedEventArgs e)
    {
        App.Data.Settings.AlwaysOnTop = !App.Data.Settings.AlwaysOnTop; Topmost = App.Data.Settings.AlwaysOnTop; UpdateQuickControls(); App.Data.SaveAll();
    }
    private void Notebook_Click(object sender, RoutedEventArgs e) { Hide(); App.ShowNotebook(); }
    private void MyComputer_Click(object sender, RoutedEventArgs e)
    {
        e.Handled = true;
        LaunchFixedExplorer("shell:MyComputerFolder");
    }
    private void Downloads_Click(object sender, RoutedEventArgs e)
    {
        e.Handled = true;
        try { LaunchFixedExplorer(KnownFolders.Downloads); }
        catch (Exception ex) { MessageBox.Show(LocalizationService.Get("CannotOpenDownloads") + ex.Message, LocalizationService.Get("AppName")); }
    }
    private void RecycleBin_Click(object sender, RoutedEventArgs e)
    {
        e.Handled = true;
        LaunchFixedExplorer("shell:RecycleBinFolder");
    }
    private void RecycleBin_RightClick(object sender, MouseButtonEventArgs e)
    {
        if (sender is not Button button) return;
        e.Handled = true;
        var menu = new ContextMenu
        {
            PlacementTarget = button,
            FontFamily = new FontFamily("Microsoft YaHei UI, Segoe UI"),
            FontSize = 13,
            Foreground = new SolidColorBrush(Color.FromRgb(17, 24, 39)),
            Background = Brushes.White
        };
        var emptyItem = MenuItem(LocalizationService.Get("EmptyRecycleBin"), EmptyRecycleBin);
        emptyItem.IsEnabled = !TryGetRecycleBinItemCount(out var itemCount) || itemCount > 0;
        menu.Items.Add(emptyItem);
        HoldWhileMenuOpen(menu);
        menu.IsOpen = true;
    }
    private async void EmptyRecycleBin()
    {
        if (TryGetRecycleBinItemCount(out var itemCount) && itemCount == 0)
        {
            await RefreshRecycleBinIconAsync();
            Activate();
            return;
        }

        var result = SHEmptyRecycleBin(new WindowInteropHelper(this).Handle, null, 0);
        const int Cancelled = unchecked((int)0x800704C7);
        if (result != 0 && result != Cancelled)
            MessageBox.Show(LocalizationService.Get("EmptyRecycleBinFailed") + $"0x{result:X8}", LocalizationService.Get("AppName"), MessageBoxButton.OK, MessageBoxImage.Warning);
        await RefreshRecycleBinIconAsync();
        Activate();
    }
    private static bool TryGetRecycleBinItemCount(out long itemCount)
    {
        var info = new RecycleBinInfo { Size = (uint)Marshal.SizeOf<RecycleBinInfo>() };
        var result = SHQueryRecycleBin(null, ref info);
        itemCount = result == 0 ? info.ItemCount : 0;
        return result == 0;
    }
    private async Task RefreshRecycleBinIconAsync()
    {
        var isEmpty = TryGetRecycleBinItemCount(out var itemCount) && itemCount == 0;
        RecycleBinIcon.Source = await IconHelper.LoadRecycleBinAsync(isEmpty)
            ?? await IconHelper.LoadAsync("::{645FF040-5081-101B-9F08-00AA002F954E}", false);
    }
    private void LaunchFixedExplorer(string destination)
    {
        try
        {
            var arguments = destination.StartsWith("shell:", StringComparison.OrdinalIgnoreCase)
                ? destination
                : $"\"{destination}\"";
            Process.Start(new ProcessStartInfo("explorer.exe", arguments) { UseShellExecute = true });
            Hide();
        }
        catch (Exception ex) { MessageBox.Show(LocalizationService.Get("CannotOpen") + ex.Message, LocalizationService.Get("AppName")); }
    }
    private void Window_PreviewMouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
    {
        if (IsInsideInteractiveControl(e.OriginalSource as DependencyObject) || e.LeftButton != MouseButtonState.Pressed) return;
        // 大面积透明窗口拖动时，实时阴影是最昂贵的渲染项；移动期间暂时关闭，释放后恢复。
        PanelShell.Effect = null; LinearShell.Effect = null;
        try { DragMove(); } finally { ApplyBackgroundOpacity(); }
        var s = App.Data.Settings;
        if (s.PanelMode == "Linear") { s.LinearLeft = Left; s.LinearTop = Top; } else { s.PanelLeft = Left; s.PanelTop = Top; }
        s.PositionMode = "Remember"; App.Data.SaveAll();
    }
    private void Window_PreviewMouseMove(object sender, System.Windows.Input.MouseEventArgs e)
    {
        if (_internalDragSource == null || e.LeftButton != MouseButtonState.Pressed) return;
        var point = e.GetPosition(this);
        if (!_isInternalDragging && (Math.Abs(point.X - _dragStart.X) > SystemParameters.MinimumHorizontalDragDistance || Math.Abs(point.Y - _dragStart.Y) > SystemParameters.MinimumVerticalDragDistance))
        {
            _isInternalDragging = true;
            if (_internalDragButton != null) _internalDragButton.Opacity = 0.38;
            Mouse.OverrideCursor = Cursors.Hand;
        }
        if (_isInternalDragging)
        {
            var hit = InputHitTest(point) as DependencyObject;
            var candidate = FindButtonAncestor(hit);
            if (candidate?.DataContext is not ActionSlot candidateSlot || ReferenceEquals(candidate, _internalDragButton) ||
                (_linearDragSource && !App.Data.LinearItems.Contains(candidateSlot))) candidate = null;
            UpdateDragHover(candidate);
        }
    }
    private async void Window_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
    {
        if (_internalDragSource == null) return;
        Button? completedSource = null, completedTarget = null;
        if (_isInternalDragging)
        {
            var hit = InputHitTest(e.GetPosition(this)) as DependencyObject;
            var targetButton = FindButtonAncestor(hit);
            if (targetButton?.DataContext is ActionSlot target && !ReferenceEquals(target, _internalDragSource))
            {
                if (_linearDragSource && App.Data.LinearItems.Contains(target))
                {
                    var oldIndex = App.Data.LinearItems.IndexOf(_internalDragSource); var newIndex = App.Data.LinearItems.IndexOf(target);
                    if (oldIndex >= 0 && newIndex >= 0) App.Data.LinearItems.Move(oldIndex, newIndex);
                    RefreshLinearItems();
                }
                else SwapBindings(_internalDragSource, target);
                App.Data.SaveAll(); completedSource = _internalDragButton; completedTarget = targetButton;
            }
            _ignoreClicksUntil = DateTime.UtcNow.AddMilliseconds(250);
        }
        if (_internalDragButton != null) _internalDragButton.Opacity = 1;
        ResetDragHover(); Mouse.OverrideCursor = null;
        // Button 自己需要保留鼠标捕获直到正常 MouseUp，才能产生 Click。
        // 只有进入内部拖拽后才主动释放，避免吞掉普通单击。
        if (_isInternalDragging) Mouse.Capture(null);
        _internalDragSource = null; _internalDragButton = null; _isInternalDragging = false; _linearDragSource = false;
        if (completedSource != null && completedTarget != null) await FlashSwapAsync(completedSource, completedTarget);
    }
    private void UpdateDragHover(Button? button)
    {
        if (ReferenceEquals(button, _dragHoverButton)) return;
        ResetDragHover(); _dragHoverButton = button;
        if (button == null) return;
        button.BorderBrush = (System.Windows.Media.Brush)FindResource("AccentBrush"); button.BorderThickness = new Thickness(3);
        button.Background = (System.Windows.Media.Brush)FindResource("AccentSoftBrush");
    }
    private void ResetDragHover()
    {
        if (_dragHoverButton == null) return;
        _dragHoverButton.SetResourceReference(Control.BorderBrushProperty, "BorderBrush"); _dragHoverButton.BorderThickness = new Thickness(1);
        _dragHoverButton.SetResourceReference(Control.BackgroundProperty, "CardBrush");
        _dragHoverButton = null;
    }
    private async Task FlashSwapAsync(Button source, Button target)
    {
        var accent = (System.Windows.Media.Brush)FindResource("AccentBrush"); var soft = (System.Windows.Media.Brush)FindResource("AccentSoftBrush");
        foreach (var button in new[] { source, target }) { button.BorderBrush = accent; button.BorderThickness = new Thickness(3); button.Background = soft; }
        source.BeginAnimation(OpacityProperty, new DoubleAnimation(0.45, 1, TimeSpan.FromMilliseconds(360)));
        target.BeginAnimation(OpacityProperty, new DoubleAnimation(0.45, 1, TimeSpan.FromMilliseconds(360)));
        await Task.Delay(420);
        foreach (var button in new[] { source, target }) { button.SetResourceReference(Control.BorderBrushProperty, "BorderBrush"); button.BorderThickness = new Thickness(1); button.SetResourceReference(Control.BackgroundProperty, "CardBrush"); }
    }
    private static Button? FindButtonAncestor(DependencyObject? node)
    {
        while (node != null) { if (node is Button button) return button; node = System.Windows.Media.VisualTreeHelper.GetParent(node); }
        return null;
    }
    private static void SwapBindings(ActionSlot source, ActionSlot target)
    {
        (source.Kind, target.Kind) = (target.Kind, source.Kind);
        (source.TargetPath, target.TargetPath) = (target.TargetPath, source.TargetPath);
        (source.DisplayName, target.DisplayName) = (target.DisplayName, source.DisplayName);
        (source.Icon, target.Icon) = (target.Icon, source.Icon);
    }
    private static bool IsInsideInteractiveControl(DependencyObject? node)
    {
        while (node != null) { if (node is Button or System.Windows.Controls.Primitives.Thumb or Slider) return true; node = System.Windows.Media.VisualTreeHelper.GetParent(node); }
        return false;
    }
    protected override void OnClosing(System.ComponentModel.CancelEventArgs e) { e.Cancel = true; Hide(); }
}
