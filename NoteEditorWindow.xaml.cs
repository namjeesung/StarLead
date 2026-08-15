using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;

namespace StarLead;

public partial class NoteEditorWindow : Window
{
    private readonly NotebookNote _note;
    private bool _loading = true;
    private readonly DispatcherTimer _saveTimer = new() { Interval = TimeSpan.FromMilliseconds(450) };
    public NoteEditorWindow(NotebookNote note)
    {
        InitializeComponent(); _note = note; TitleBox.Text = note.Title; ContentBox.Text = note.Content; _loading = false;
        _saveTimer.Tick += (_, _) => { _saveTimer.Stop(); App.Data.SaveAll(); };
        Loaded += (_, _) => ContentBox.Focus();
        Closing += (_, _) => { SaveNow(); };
    }
    private void Editor_TextChanged(object sender, TextChangedEventArgs e)
    {
        if (_loading) return; _note.Title = string.IsNullOrWhiteSpace(TitleBox.Text) ? LocalizationService.Get("Untitled") : TitleBox.Text; _note.Content = ContentBox.Text; _saveTimer.Stop(); _saveTimer.Start();
    }
    private void Done_Click(object sender, RoutedEventArgs e) => Close();
    private void SaveNow() { _saveTimer.Stop(); if (!_loading) { _note.Title = string.IsNullOrWhiteSpace(TitleBox.Text) ? LocalizationService.Get("Untitled") : TitleBox.Text; _note.Content = ContentBox.Text; App.Data.SaveAll(); } }
}
