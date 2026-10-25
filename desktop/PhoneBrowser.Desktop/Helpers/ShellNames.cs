namespace PhoneBrowser.Desktop.Helpers;

using System.IO;
using System.Runtime.InteropServices;

/// <summary>
/// Provides localized, display-only names of file system folders, as Windows Explorer shows them
/// (e.g. "Pictures -> "Obrazy" on Polish system).
/// </summary>
/// <remarks>
/// <list type="bullet">
/// <item><see href="https://learn.microsoft.com/en-us/windows/win32/api/shellapi/nf-shellapi-shgetfileinfow">SHGetFileInfoW (Microsoft Learn)</see></item>
/// <item><see href="https://www.pinvoke.net/default.aspx/shell32/SHGetFileInfo.html">SHGetFileInfo (pinvoke.net)</see></item>
/// </list>
/// </remarks>
internal static class ShellNames
{
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    private struct SHFILEINFO
    {
        public SHFILEINFO()
        {
            hIcon = IntPtr.Zero;
            iIcon = 0;
            dwAttributes = 0;
            szDisplayName = "";
            szTypeName = "";
        }

        public IntPtr hIcon;
        public int iIcon;
        public uint dwAttributes;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260)]
        public string szDisplayName;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 80)]
        public string szTypeName;
    }

    [Flags]
    private enum SHGFI : int
    {
        // Returns the name Explorer would show (e.g. "Obrazy" for "Pictures")
        DisplayName = 0x000000200,
    }

    public static string? GetDisplayName(string path)
    {
        SHFILEINFO info = new();
        int cbFileInfo = Marshal.SizeOf(info);

        var result = SHGetFileInfo(path, 0, out info, (uint)cbFileInfo, SHGFI.DisplayName);

        return result != 0 ? info.szDisplayName : null;
    }

    public static string GetDisplayPath(string path)
    {
        var fullPath = Path.GetFullPath(path);
        var root = Path.GetPathRoot(fullPath) ?? "";

        var segments = fullPath[root.Length..].Split(Path.DirectorySeparatorChar, StringSplitOptions.RemoveEmptyEntries);

        var result = root;
        var current = root;

        foreach(var segment in segments)
        {
            current = Path.Combine(current, segment);
            var localized = GetDisplayName(current);

            result = Path.Combine(result, localized ?? segment);
        }

        return result;
    }

    [DllImport("shell32.dll", CharSet = CharSet.Unicode)]
    private static extern int SHGetFileInfo(string pszPath, int dwFileAttributes, out SHFILEINFO psfi, uint cbFileInfo, SHGFI uFlags);
}

