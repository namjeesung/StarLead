using System.Collections.Specialized;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace StarLead;

public partial class NotebookWindow : Window
{
    private const double GridWidth = 190, GridHeight = 140, OriginX = 25, OriginY = 25;
    private NotebookNote? _dragNote;
    private Border? _dragCard;
    private Point _dragOffset;
    private bool _didDrag;

    public NotebookWindow()
    {
        InitializeComponent();
        NotesItems.ItemsSource = App.Data.NotebookNotes;
        App.Data.NotebookNotes.CollectionChanged += Notes_CollectionChanged;
        Loaded += (_, _) => { if (IsAutoMode) AutoArrange(); UpdateState(); };
        SizeChanged += (_, _) => { if (IsAutoMode) AutoArrange(); };
        Closed += (_, _) => { App.Data.NotebookNotes.CollectionChanged -= Notes_CollectionChanged; App.Data.SaveAll(); };
    }

    private bool IsAutoMode => App.Data.Settings.NotebookLayoutMode == "Auto";
    private void Notes_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e) => UpdateState();
    private void UpdateState()
    {
        EmptyHint.Visibility = App.Data.NotebookNotes.Count == 0 ? Visibility.Visible : Visibility.Collapsed;
        FreeButton.Background = !IsAutoMode ? (Brush)FindResource("AccentSoftBrush") : (Brush)FindResource("CardBrush");
        AutoButton.Background = IsAutoMode ? (Brush)FindResource("AccentSoftBrush") : (Brush)FindResource("CardBrush");
        int selected = App.Data.NotebookNotes.Count(n => n.IsSelected);
        SelectionText.Text = selected > 0 ? LocalizationService.Format("SelectedCount", selected) : LocalizationService.Get("MultiSelectHint");
        UpdateBoardExtent();
    }
    public void RefreshLanguage() => UpdateState();

    private void FreeMode_Click(object sender, RoutedEventArgs e) { App.Data.Settings.NotebookLayoutMode = "Free"; App.Data.SaveAll(); UpdateState(); }
    private void AutoMode_Click(object sender, RoutedEventArgs e) { App.Data.Settings.NotebookLayoutMode = "Auto"; AutoArrange(); App.Data.SaveAll(); UpdateState(); }
    private void Arrange_Click(object sender, RoutedEventArgs e) { AutoArrange(); App.Data.SaveAll(); }
    private void New_Click(object sender, RoutedEventArgs e) => CreateNote();
    private void Board_MouseDown(object sender, MouseButtonEventArgs e)
    {
        if (FindCardAncestor(e.OriginalSource as DependencyObject) != null) return;
        if (e.ClickCount == 2) { e.Handled = true; CreateNote(); return; }
        ClearSelection(); UpdateState();
    }

    private void CreateNote()
    {
        var position = FindNextGridPosition();
        var note = new NotebookNote { X = position.X, Y = position.Y, Title = LocalizationService.Get("NewNote") };
        ClearSelection(); note.IsSelected = true;
        App.Data.NotebookNotes.Add(note);
        if (IsAutoMode) AutoArrange();
        App.Data.SaveAll();
        OpenEditor(note);
    }
    private void OpenEditor(NotebookNote note)
    {
        var editor = new NoteEditorWindow(note) { Owner = this };
        editor.ShowDialog();
        if (IsAutoMode) AutoArrange();
        App.Data.SaveAll(); UpdateState();
    }

    private void Card_MouseDown(object sender, MouseButtonEventArgs e)
    {
        if (sender is not Border { DataContext: NotebookNote note } card) return;
        SelectNote(note, Keyboard.Modifiers.HasFlag(ModifierKeys.Control));
        if (e.ClickCount == 2) { e.Handled = true; OpenEditor(note); return; }
        if (IsAutoMode) return;
        _dragNote = note; _dragCard = card; _dragOffset = e.GetPosition(card); _didDrag = false;
        card.CaptureMouse(); card.Opacity = 0.72; Panel.SetZIndex(card, 1000); e.Handled = true;
    }
    private void Card_MouseMove(object sender, MouseEventArgs e)
    {
        if (_dragNote == null || _dragCard == null || e.LeftButton != MouseButtonState.Pressed) return;
        var point = e.GetPosition(BoardSurface); _didDrag = true;
        _dragNote.X = Math.Max(0, point.X - _dragOffset.X); _dragNote.Y = Math.Max(0, point.Y - _dragOffset.Y);
        UpdateBoardExtent(); e.Handled = true;
    }
    private void Card_MouseUp(object sender, MouseButtonEventArgs e)
    {
        if (_dragNote == null || _dragCard == null) return;
        if (_didDrag)
        {
            _dragNote.X = Snap(_dragNote.X, OriginX, GridWidth); _dragNote.Y = Snap(_dragNote.Y, OriginY, GridHeight);
            App.Data.SaveAll(); e.Handled = true;
        }
        _dragCard.ReleaseMouseCapture(); _dragCard.Opacity = 1; Panel.SetZIndex(_dragCard, 0);
        _dragNote = null; _dragCard = null; _didDrag = false; UpdateBoardExtent();
    }
    private static double Snap(double value, double origin, double step) => Math.Max(origin, origin + Math.Round((value - origin) / step) * step);

    private void Card_RightClick(object sender, MouseButtonEventArgs e)
    {
        if (sender is not Border { DataContext: NotebookNote note } card) return;
        if (!note.IsSelected) { ClearSelection(); note.IsSelected = true; UpdateState(); }
        e.Handled = true; var menu = new ContextMenu();
        menu.Items.Add(NoteMenuItem(LocalizationService.Get("Edit"), () => OpenEditor(note)));
        menu.Items.Add(NoteMenuItem(LocalizationService.Get("CopyText"), () => Clipboard.SetText(note.Content ?? "")));
        menu.Items.Add(NoteMenuItem(LocalizationService.Get(note.IsPinned ? "Unpin" : "Pin"), () => { note.IsPinned = !note.IsPinned; if (IsAutoMode) AutoArrange(); App.Data.SaveAll(); }));
        var colorMenu = NoteSubmenu(LocalizationService.Get("SetColor"));
        colorMenu.Items.Add(NoteMenuItem(LocalizationService.Get("DefaultColor"), () => ApplyColor("Default")));
        colorMenu.Items.Add(NoteMenuItem(LocalizationService.Get("Yellow"), () => ApplyColor("Yellow")));
        colorMenu.Items.Add(NoteMenuItem(LocalizationService.Get("Blue"), () => ApplyColor("Blue")));
        colorMenu.Items.Add(NoteMenuItem(LocalizationService.Get("Green"), () => ApplyColor("Green")));
        colorMenu.Items.Add(NoteMenuItem(LocalizationService.Get("Pink"), () => ApplyColor("Pink")));
        colorMenu.Items.Add(NoteMenuItem(LocalizationService.Get("Purple"), () => ApplyColor("Purple")));
        menu.Items.Add(colorMenu);
        menu.Items.Add(new Separator());
        menu.Items.Add(NoteMenuItem(LocalizationService.Get("Delete"), () => DeleteNote(note)));
        menu.PlacementTarget = card; menu.IsOpen = true;
    }
    private void DeleteNote(NotebookNote note)
    {
        if (MessageBox.Show(LocalizationService.Format("DeleteNoteQuestion", note.Title), LocalizationService.Get("NotebookTitle"), MessageBoxButton.YesNo, MessageBoxImage.Question) != MessageBoxResult.Yes) return;
        App.Data.NotebookNotes.Remove(note); if (IsAutoMode) AutoArrange(); App.Data.SaveAll();
    }
    private static MenuItem NoteMenuItem(string text, Action action)
    {
        var label = new TextBlock { Text = text, Foreground = new SolidColorBrush(Color.FromRgb(17, 24, 39)), FontFamily = new FontFamily("Microsoft YaHei UI"), FontWeight = FontWeights.SemiBold };
        var item = new MenuItem { Header = label, Foreground = label.Foreground, Background = Brushes.White }; item.Click += (_, _) => action(); return item;
    }
    private static MenuItem NoteSubmenu(string text)
    {
        var label = new TextBlock { Text = text, Foreground = new SolidColorBrush(Color.FromRgb(17, 24, 39)), FontFamily = new FontFamily("Microsoft YaHei UI"), FontWeight = FontWeights.SemiBold };
        return new MenuItem { Header = label, Foreground = label.Foreground, Background = Brushes.White };
    }
    private void ApplyColor(string colorKey)
    {
        var selected = App.Data.NotebookNotes.Where(n => n.IsSelected).ToList();
        if (selected.Count == 0) return;
        foreach (var note in selected) note.ColorKey = colorKey;
        App.Data.SaveAll(); UpdateState();
    }
    private void SelectNote(NotebookNote note, bool toggle)
    {
        if (toggle) note.IsSelected = !note.IsSelected;
        else { if (!note.IsSelected || App.Data.NotebookNotes.Count(n => n.IsSelected) > 1) { ClearSelection(); note.IsSelected = true; } }
        UpdateState();
    }
    private void ClearSelection() { foreach (var item in App.Data.NotebookNotes) item.IsSelected = false; }

    private void AutoArrange()
    {
        int columns = Math.Max(1, (int)Math.Floor((Math.Max(900, BoardScroll.ActualWidth) - OriginX * 2) / GridWidth));
        int index = 0;
        foreach (var note in App.Data.NotebookNotes.OrderByDescending(n => n.IsPinned).ThenByDescending(n => n.ModifiedAt))
        {
            note.X = OriginX + index % columns * GridWidth; note.Y = OriginY + index / columns * GridHeight; index++;
        }
        UpdateBoardExtent();
    }
    private Point FindNextGridPosition()
    {
        int columns = Math.Max(1, (int)Math.Floor((Math.Max(900, BoardScroll.ActualWidth) - OriginX * 2) / GridWidth));
        var occupied = App.Data.NotebookNotes.Select(n => ($"{Math.Round((n.X-OriginX)/GridWidth)}", $"{Math.Round((n.Y-OriginY)/GridHeight)}")).ToHashSet();
        for (int i = 0; ; i++) { int col = i % columns, row = i / columns; if (!occupied.Contains(($"{col}", $"{row}"))) return new Point(OriginX + col * GridWidth, OriginY + row * GridHeight); }
    }
    private void UpdateBoardExtent()
    {
        BoardSurface.Width = Math.Max(900, Math.Max(BoardScroll.ActualWidth - 4, App.Data.NotebookNotes.Select(n => n.X + 210).DefaultIfEmpty(900).Max()));
        BoardSurface.Height = Math.Max(560, Math.Max(BoardScroll.ActualHeight - 4, App.Data.NotebookNotes.Select(n => n.Y + 165).DefaultIfEmpty(560).Max()));
    }
    private static Border? FindCardAncestor(DependencyObject? node)
    {
        while (node != null) { if (node is Border { DataContext: NotebookNote }) return (Border)node; node = VisualTreeHelper.GetParent(node); }
        return null;
    }
}
