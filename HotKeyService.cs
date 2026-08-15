using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;

namespace StarLead;

public sealed class HotKeyService : IDisposable
{
    private const int Id = 0x51A7, WM_HOTKEY = 0x0312;
    private HwndSource? _source; private IntPtr _handle;
    public event Action? Pressed;
    [DllImport("user32.dll")] private static extern bool RegisterHotKey(IntPtr hWnd, int id, uint modifiers, uint virtualKey);
    [DllImport("user32.dll")] private static extern bool UnregisterHotKey(IntPtr hWnd, int id);

    public bool Register(Window window, AppSettings settings)
    {
        Unregister(); _handle = new WindowInteropHelper(window).Handle; _source = HwndSource.FromHwnd(_handle); _source?.AddHook(WndProc);
        uint mod = settings.HotKeyModifier switch { "无" or "None" => 0, "Alt" => 1, "Shift" => 4, "Ctrl+Alt" => 3, "Ctrl+Shift" => 6, _ => 2 };
        var key = ParseKey(settings.HotKeyKey);
        return RegisterHotKey(_handle, Id, mod | 0x4000, (uint)KeyInterop.VirtualKeyFromKey(key));
    }
    private static Key ParseKey(string value) => value switch
    {
        "`" => Key.Oem3,
        "-" => Key.OemMinus,
        "=" => Key.OemPlus,
        "[" => Key.OemOpenBrackets,
        "]" => Key.OemCloseBrackets,
        "\\" => Key.Oem5,
        ";" => Key.Oem1,
        "'" => Key.OemQuotes,
        "," => Key.OemComma,
        "." => Key.OemPeriod,
        "/" => Key.Oem2,
        _ => Enum.TryParse<Key>(value, true, out var parsed) ? parsed : Key.Space
    };
    private IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wp, IntPtr lp, ref bool handled) { if (msg == WM_HOTKEY && wp.ToInt32() == Id) { handled = true; Pressed?.Invoke(); } return IntPtr.Zero; }
    public void Unregister() { if (_handle != IntPtr.Zero) UnregisterHotKey(_handle, Id); if (_source != null) _source.RemoveHook(WndProc); _source = null; }
    public void Dispose() => Unregister();
}
