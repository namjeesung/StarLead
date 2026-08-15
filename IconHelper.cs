using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace StarLead;

public static class IconHelper
{
    private static readonly Guid ImageFactoryId = new("BCC18B79-BA16-442F-80C4-8A59C30C463B");

    public static Task<ImageSource?> LoadAsync(string path, bool folder) => Task.Run<ImageSource?>(() =>
    {
        IShellItemImageFactory? factory = null;
        IntPtr bitmapHandle = IntPtr.Zero;
        try
        {
            var iid = ImageFactoryId;
            if (SHCreateItemFromParsingName(path, IntPtr.Zero, ref iid, out factory) == 0 &&
                factory.GetImage(new NativeSize { Width = 128, Height = 128 }, ImageFlags.IconOnly | ImageFlags.BiggerSizeOk | ImageFlags.ScaleUp, out bitmapHandle) == 0 &&
                bitmapHandle != IntPtr.Zero)
            {
                var source = Imaging.CreateBitmapSourceFromHBitmap(bitmapHandle, IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
                source.Freeze(); return source;
            }
        }
        catch { }
        finally
        {
            if (bitmapHandle != IntPtr.Zero) DeleteObject(bitmapHandle);
            if (factory != null && Marshal.IsComObject(factory)) Marshal.FinalReleaseComObject(factory);
        }

        try
        {
            using var icon = Icon.ExtractAssociatedIcon(path);
            if (icon == null) return null;
            var source = Imaging.CreateBitmapSourceFromHIcon(icon.Handle, Int32Rect.Empty, BitmapSizeOptions.FromWidthAndHeight(64, 64));
            source.Freeze(); return source;
        }
        catch { return null; }
    });

    [StructLayout(LayoutKind.Sequential)]
    private struct NativeSize { public int Width; public int Height; }

    [Flags]
    private enum ImageFlags : uint
    {
        BiggerSizeOk = 0x1,
        IconOnly = 0x4,
        ScaleUp = 0x100
    }

    [ComImport, Guid("BCC18B79-BA16-442F-80C4-8A59C30C463B"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    private interface IShellItemImageFactory
    {
        [PreserveSig] int GetImage(NativeSize size, ImageFlags flags, out IntPtr bitmapHandle);
    }

    [DllImport("shell32.dll", CharSet = CharSet.Unicode, PreserveSig = true)]
    private static extern int SHCreateItemFromParsingName(string path, IntPtr bindContext, ref Guid interfaceId, out IShellItemImageFactory factory);

    [DllImport("gdi32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool DeleteObject(IntPtr objectHandle);
}
