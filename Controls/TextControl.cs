using System.Numerics;
using SkiaSharp;

namespace OverlayLibrary.Controls;

public class TextControl : IControl
{
    public bool Interactive { get; set; } = false;
    
    
    private string Text { get; set; }

    private SKPaint Paint { get; set; }
    
    public TextControl(string text, SKPaint paint)
    {
        Text = text;
        Paint = paint;
    }

    public void UpdateText(string text)
    {
        Text = text;
    }
    
    public string GetText()
    {
        return Text;
    }
    
    public void UpdatePaint(SKPaint paint)
    {
        Paint = paint;
    }

    

    public void DrawControl(SKPoint point, SKCanvas skCanvas)
    {

        var textRect = CalculateControlRect();
        
        
        skCanvas.DrawText(Text, point.X, point.Y + textRect.Y, Paint);
    }

    
    /// <summary>
    /// Calculates the size of the control
    /// </summary>
    /// <returns>
    /// Width and height of the control
    /// </returns>
    public Vector2 CalculateControlRect()
    {
        var bound = new SKRect();
        Paint.MeasureText(Text, ref bound);
        return new Vector2()
        {
            X = bound.Width,
            Y = bound.Height
        };
    }
}