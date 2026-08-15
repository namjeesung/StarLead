using System.Runtime.InteropServices;

namespace StarLead;

internal static class KnownFolders
{
    private static readonly Guid DownloadsId = new("374DE290-123F-4565-9164-39C4925E467B");

    public static string Downloads
    {
        get
        {
            var id = DownloadsId;
            var result = SHGetKnownFolderPath(ref id, 0, IntPtr.Zero, out var pathPointer);
            if (result != 0 || pathPointer == IntPtr.Zero)
                throw new InvalidOperationException($"无法获取 Windows 下载目录（0x{result:X8}）。");

            try { return Marshal.PtrToStringUni(pathPointer)!; }
            finally { Marshal.FreeCoTaskMem(pathPointer); }
        }
    }

    [DllImport("shell32.dll", CharSet = CharSet.Unicode, PreserveSig = true)]
    private static extern int SHGetKnownFolderPath(ref Guid folderId, uint flags, IntPtr token, out IntPtr path);
}
