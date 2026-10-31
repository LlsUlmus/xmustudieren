# Use a compact console font for this window only (so 150 columns fit on screen for the screenshot).
Add-Type @'
using System; using System.Runtime.InteropServices;
public class ConFont {
  [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
  public struct CONSOLE_FONT_INFOEX {
    public uint cbSize; public uint nFont; public short X; public short Y; public int FontFamily; public int FontWeight;
    [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)] public string FaceName;
  }
  [DllImport("kernel32.dll", SetLastError = true)] public static extern IntPtr GetStdHandle(int n);
  [DllImport("kernel32.dll", SetLastError = true)] public static extern bool SetCurrentConsoleFontEx(IntPtr h, bool max, ref CONSOLE_FONT_INFOEX f);
  public static bool Set(string face, short height) {
    var f = new CONSOLE_FONT_INFOEX(); f.cbSize = (uint)Marshal.SizeOf(f);
    f.FaceName = face; f.Y = height; f.X = 0; f.FontFamily = 54; f.FontWeight = 400;
    return SetCurrentConsoleFontEx(GetStdHandle(-11), false, ref f);
  }
}
'@
[ConFont]::Set('Consolas', 20) | Out-Null
