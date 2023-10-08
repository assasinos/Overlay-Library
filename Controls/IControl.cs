using System.Numerics;
using SkiaSharp;

namespace OverlayLibrary.Controls;

public interface IControl
{

    internal bool Interactive { get; set; }
    public void DrawControl(SKPoint point, SKCanvas skCanvas);
    
    public Vector2 CalculateControlRect();
}