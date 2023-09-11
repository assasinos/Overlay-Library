using System.Runtime.InteropServices;
using SkiaSharp;

namespace OverlayLibrary;

public class WinApi
{
    [DllImport("user32.dll", SetLastError = true)]
    public static extern int GetWindowLong(IntPtr hWnd, int nIndex);
    [DllImport("user32.dll")]
    public static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);
    public const int GWL_EXSTYLE = -20;
    public const int WS_EX_LAYERED = 0x80000;
    public const int WS_EX_TRANSPARENT = 0x20;
    
    
    
    [DllImport("user32.dll", SetLastError=true)]
    public static extern bool GetWindowRect(IntPtr hwnd, out RECT lpRect);
    
    [StructLayout(LayoutKind.Sequential)]
    public struct RECT
    {
        public int Left;        // x position of upper-left corner
        public int Top;         // y position of upper-left corner
        public int Right;       // x position of lower-right corner
        public int Bottom;      // y position of lower-right corner

        public override bool Equals(object? obj)
        {
            if (obj is not RECT other)
                return false;

            return other.Left == Left && other.Top == Top && other.Right == Right && other.Bottom == Bottom;
        }

        public bool Equals(RECT other)
        {
            return Left == other.Left && Top == other.Top && Right == other.Right && Bottom == other.Bottom;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Left, Top, Right, Bottom);
        }
        public static bool operator ==(RECT left, RECT right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(RECT left, RECT right)
        {
            return !(left == right);
        }
        
    } 
    
    

[DllImport("user32.dll", SetLastError=true)]
public static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, UInt32 uFlags);
    
    
    
}