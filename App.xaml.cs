using System.Windows;
using Forms = System.Windows.Forms;

namespace StarLead;

public partial class App : System.Windows.Application
{
    public static AppDataService Data { get; private set; } = null!;
    public static MainWindow MainPanel { get; private set; } = null!;
    private static NotebookWindow? _notebook;
    private Forms.NotifyIcon? _trayIcon;

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);
        Data = new AppDataService();
        Data.Load();
        LocalizationService.Apply(Data.Settings.Language);
        ThemeService.Apply(Data.Settings.Theme, Data.Settings.VisualStyle);
        MainPanel = new MainWindow();
        CreateTrayIcon();
        MainPanel.Show();
        if (e.Args.Any(a => a.Equals("--startup", StringComparison.OrdinalIgnoreCase))) MainPanel.Hide();
        if (e.Args.Any(a => a.Equals("--notebook", StringComparison.OrdinalIgnoreCase))) { MainPanel.Hide(); ShowNotebook(); }
    }

    private void CreateTrayIcon()
    {
        _trayIcon = new Forms.NotifyIcon { Text = LocalizationService.Get("AppName"), Icon = System.Drawing.Icon.ExtractAssociatedIcon(Environment.ProcessPath!) ?? System.Drawing.SystemIcons.Application, Visible = true, ContextMenuStrip = new Forms.ContextMenuStrip() };
        _trayIcon.ContextMenuStrip.Items.Add(LocalizationService.Get("TrayOpen"), null, (_, _) => Dispatcher.Invoke(MainPanel.TogglePanel));
        _trayIcon.ContextMenuStrip.Items.Add(LocalizationService.Get("Notebook"), null, (_, _) => Dispatcher.Invoke(ShowNotebook));
        _trayIcon.ContextMenuStrip.Items.Add(LocalizationService.Get("Settings"), null, (_, _) => Dispatcher.Invoke(() => new SettingsWindow().ShowDialog()));
        _trayIcon.ContextMenuStrip.Items.Add(new Forms.ToolStripSeparator());
        _trayIcon.ContextMenuStrip.Items.Add(LocalizationService.Get("TrayExit"), null, (_, _) => Dispatcher.Invoke(ExitApplication));
        _trayIcon.DoubleClick += (_, _) => Dispatcher.Invoke(MainPanel.TogglePanel);
    }
    public void RefreshLanguage()
    {
        if (_trayIcon != null) { _trayIcon.Visible = false; _trayIcon.Dispose(); _trayIcon = null; }
        CreateTrayIcon();
        MainPanel.ApplyLanguage();
        _notebook?.RefreshLanguage();
    }

    public void ExitApplication()
    {
        Data.SaveAll();
        MainPanel.DisposeHotKey();
        if (_trayIcon != null) { _trayIcon.Visible = false; _trayIcon.Dispose(); }
        Shutdown();
    }
    public static void ShowNotebook()
    {
        if (_notebook == null) { _notebook = new NotebookWindow(); _notebook.Closed += (_, _) => _notebook = null; _notebook.Show(); }
        else { _notebook.Show(); _notebook.Activate(); }
    }
}
