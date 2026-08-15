using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text.Json.Serialization;
using System.Windows;
using System.Windows.Media;

namespace StarLead;

public enum TargetKind { None, Program, File, Folder, RecycleBin, Notebook }

public sealed class ActionSlot : INotifyPropertyChanged
{
    private string? _targetPath;
    private string? _displayName;
    private TargetKind _kind;
    private bool _isVisible = true;
    private bool _showLabel = true;
    private bool _isHighlighted;
    private Thickness _linearMargin = new(2, 0, 2, 0);
    public string Key { get; set; } = "";
    public TargetKind Kind { get => _kind; set { _kind = value; OnChanged(); OnChanged(nameof(IsEmpty)); } }
    public string? TargetPath { get => _targetPath; set { _targetPath = value; OnChanged(); } }
    public string? DisplayName { get => _displayName; set { _displayName = value; OnChanged(); } }
    public bool IsVisible { get => _isVisible; set { _isVisible = value; OnChanged(); } }
    [JsonIgnore] public bool ShowLabel { get => _showLabel; set { _showLabel = value; OnChanged(); } }
    [JsonIgnore] public bool IsHighlighted { get => _isHighlighted; set { _isHighlighted = value; OnChanged(); } }
    [JsonIgnore] public Thickness LinearMargin { get => _linearMargin; set { _linearMargin = value; OnChanged(); } }
    [JsonIgnore] public bool IsEmpty => Kind == TargetKind.None;
    private ImageSource? _icon;
    [JsonIgnore] public ImageSource? Icon { get => _icon; set { _icon = value; OnChanged(); } }
    public event PropertyChangedEventHandler? PropertyChanged;
    public void OnChanged([CallerMemberName] string? name = null) => PropertyChanged?.Invoke(this, new(name));
}

public sealed class AppSettings
{
    public string Language { get; set; } = "zh-CN";
    public string Theme { get; set; } = "Light";
    public string VisualStyle { get; set; } = "LiquidGlass";
    public bool StartWithWindows { get; set; }
    public string HotKeyModifier { get; set; } = "Ctrl";
    public string HotKeyKey { get; set; } = "Space";
    public string PositionMode { get; set; } = "Remember";
    public bool ShowIconNames { get; set; } = true;
    public bool AlwaysOnTop { get; set; } = true;
    public string NotebookLayoutMode { get; set; } = "Free";
    public double? PanelLeft { get; set; }
    public double? PanelTop { get; set; }
    public string PanelMode { get; set; } = "Keyboard";
    public double BackgroundOpacity { get; set; } = 92;
    public bool ShowMyComputer { get; set; } = true;
    public bool ShowDownloads { get; set; } = true;
    public bool ShowRecycleBin { get; set; } = true;
    public bool ShowNotebook { get; set; } = true;
    public bool NotebookEntriesInitialized { get; set; }
    public string NotebookKeyboardKey { get; set; } = "0";
    public int NotebookLinearIndex { get; set; }
    public double? LinearLeft { get; set; }
    public double? LinearTop { get; set; }
    public double LinearPanelWidth { get; set; } = 1200;
    public string LinearWidthMode { get; set; } = "Auto";
    public double LinearItemSpacing { get; set; } = 4;
}

public sealed class StoredData
{
    public List<ActionSlot> Slots { get; set; } = [];
    public List<ActionSlot> LinearItems { get; set; } = [];
    public List<NotebookNote> NotebookNotes { get; set; } = [];
    public AppSettings Settings { get; set; } = new();
}

public sealed class NotebookNote : INotifyPropertyChanged
{
    private string _title = "新笔记";
    private string _content = "";
    private bool _isPinned;
    private double _x;
    private double _y;
    private DateTime _modifiedAt = DateTime.Now;
    private string _colorKey = "Default";
    private bool _isSelected;
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Title { get => _title; set { _title = value; Touch(); OnChanged(); } }
    public string Content { get => _content; set { _content = value; Touch(); OnChanged(); } }
    public bool IsPinned { get => _isPinned; set { _isPinned = value; Touch(); OnChanged(); } }
    public double X { get => _x; set { _x = value; OnChanged(); } }
    public double Y { get => _y; set { _y = value; OnChanged(); } }
    public DateTime ModifiedAt { get => _modifiedAt; set { _modifiedAt = value; OnChanged(); } }
    public string ColorKey { get => _colorKey; set { _colorKey = value; OnChanged(); } }
    [JsonIgnore] public bool IsSelected { get => _isSelected; set { _isSelected = value; OnChanged(); } }
    private void Touch() { _modifiedAt = DateTime.Now; OnChanged(nameof(ModifiedAt)); }
    public event PropertyChangedEventHandler? PropertyChanged;
    private void OnChanged([CallerMemberName] string? name = null) => PropertyChanged?.Invoke(this, new(name));
}
