using System.Numerics;
using SkiaSharp;

namespace OverlayLibrary.Controls;

public class TextBoxControl : IControl
{
    
    public string Text { get; set; }
    private SKPaint Paint { get; set; }
    
    private int _cursorPosition = 0;
    
    public TextBoxControl(string text, SKPaint paint)
    {
        Text = text;
        Paint = paint;
    }
    
    private const float ButtonPadding = 10f;
    private const float ButtonRadius = 5f;
    
    private static readonly SKPaint BackgroundPaint = new SKPaint()
    {
        Color = new SKColor(20, 20, 50, 230),
        Style = SKPaintStyle.Fill
    };
    private static readonly SKPaint BorderPaint = new SKPaint()
    {
        Color = new SKColor(0, 0, 0, 255),
        Style = SKPaintStyle.Stroke
    };
    
    
    public void DrawControl(SKPoint point, SKCanvas skCanvas)
    {
        var textRect = CalculateControlRect();
        //Draw background
        skCanvas.DrawRoundRect(new SKRoundRect(new SKRect(point.X, point.Y, point.X + textRect.X + ButtonPadding, point.Y + textRect.Y +ButtonPadding /2), ButtonRadius, ButtonRadius), BackgroundPaint);
        
        //Draw border
        skCanvas.DrawRoundRect(new SKRoundRect(new SKRect(point.X, point.Y, point.X + textRect.X + ButtonPadding, point.Y + textRect.Y +ButtonPadding/2), ButtonRadius, ButtonRadius), BorderPaint);
        
        skCanvas.DrawText(Text, point.X + ButtonPadding, point.Y + textRect.Y - ButtonPadding/4, Paint);
        
    }

    public Vector2 CalculateControlRect()
    {
        var bound = new SKRect();
        Paint.MeasureText(Text, ref bound);
        return new Vector2()
        {
            X = bound.Width + ButtonPadding,
            Y = bound.Height + ButtonPadding/2
        };
    }
    
    
    
    
}