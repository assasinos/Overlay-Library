using System.Numerics;
using OverlayLibrary;
using SkiaSharp;
using static OverlayLibrary.WinApi;

namespace OverlayLibrary;

public interface IControl
{


    public void DrawControl(SKPoint point, SKCanvas skCanvas);
    
    public Vector2 CalculateControlRect();
}