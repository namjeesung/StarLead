using System.Collections.ObjectModel;
using System.IO;
using System.Text.Json;

namespace StarLead;

public sealed class AppDataService
{
    private readonly string _folder = !string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("STARLEAD_DATA_DIR"))
        ? Environment.GetEnvironmentVariable("STARLEAD_DATA_DIR")!
        : Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "StarLead");
    private string DataFile => Path.Combine(_folder, "data.json");
    private readonly JsonSerializerOptions _json = new() { WriteIndented = true };
    public ObservableCollection<ActionSlot> Slots { get; } = [];
    public ObservableCollection<ActionSlot> LinearItems { get; } = [];
    public ObservableCollection<NotebookNote> NotebookNotes { get; } = [];
    public AppSettings Settings { get; private set; } = new();

    public void Load()
    {
        Directory.CreateDirectory(_folder);
        StoredData? stored = null;
        try { if (File.Exists(DataFile)) stored = JsonSerializer.Deserialize<StoredData>(File.ReadAllText(DataFile), _json); } catch { }
        var prior = stored?.Slots.ToDictionary(s => s.Key, StringComparer.OrdinalIgnoreCase) ?? [];
        foreach (var key in "1234567890QWERTYUIOPASDFGHJKLZXCVBNM".Select(c => c.ToString()))
            Slots.Add(prior.TryGetValue(key, out var slot) ? slot : new ActionSlot { Key = key });
        Settings = stored?.Settings ?? new();
        foreach (var item in stored?.LinearItems ?? []) LinearItems.Add(item);
        if (!Settings.NotebookEntriesInitialized)
        {
            if (!Slots.Any(s => s.Kind == TargetKind.Notebook))
            {
                var preferred = Slots.FirstOrDefault(s => s.Key == Settings.NotebookKeyboardKey && s.IsEmpty)
                    ?? Slots.FirstOrDefault(s => s.IsEmpty);
                if (preferred != null) { preferred.Kind = TargetKind.Notebook; preferred.DisplayName = "笔记本"; }
            }
            if (!LinearItems.Any(s => s.Kind == TargetKind.Notebook))
            {
                var index = Math.Clamp(Settings.NotebookLinearIndex, 0, LinearItems.Count);
                LinearItems.Insert(index, new ActionSlot { Kind = TargetKind.Notebook, DisplayName = "笔记本" });
            }
            Settings.NotebookEntriesInitialized = true;
        }
        RefreshLinearKeys();
        foreach (var note in stored?.NotebookNotes ?? []) NotebookNotes.Add(note);
    }

    public void SaveAll()
    {
        Directory.CreateDirectory(_folder);
        RefreshLinearKeys();
        var data = new StoredData { Slots = Slots.ToList(), LinearItems = LinearItems.ToList(), NotebookNotes = NotebookNotes.ToList(), Settings = Settings };
        var temp = DataFile + ".tmp";
        File.WriteAllText(temp, JsonSerializer.Serialize(data, _json));
        File.Move(temp, DataFile, true);
    }

    public void RefreshLinearKeys()
    {
        for (var i = 0; i < LinearItems.Count; i++)
            LinearItems[i].Key = i < 9 ? (i + 1).ToString() : i == 9 ? "0" : (i + 1).ToString();
    }
}
