using System.Numerics;
using SkiaSharp;

namespace OverlayLibrary.Controls;

public class ButtonControl : IControl
{
    
    private static readonly SKPaint BackgroundPaint = new SKPaint()
    {
        Color = new SKColor(70, 70, 70, 255),
        Style = SKPaintStyle.Fill
    };
    private static readonly SKPaint BorderPaint = new SKPaint()
    {
        Color = new SKColor(0, 0, 0, 255),
        Style = SKPaintStyle.Stroke
    };
    
    
    public event EventHandler? Click;
    private string Text { get; set; }
    private SKPaint Paint { get; set; }
    
    public ButtonControl(string text, SKPaint paint)
    {
        Text = text;
        Paint = paint;
    }
    
    private const float ButtonPadding = 10f;
    private const float ButtonRadius = 5f;
    
    
    public void DrawControl(SKPoint point, SKCanvas skCanvas)
    {
        var textRect = CalculateControlRect();
        //Draw button background
        skCanvas.DrawRoundRect(new SKRoundRect(new SKRect(point.X, point.Y, point.X + textRect.X + ButtonPadding, point.Y + textRect.Y +ButtonPadding), ButtonRadius, ButtonRadius), BackgroundPaint);
        
        //Draw button border
        skCanvas.DrawRoundRect(new SKRoundRect(new SKRect(point.X, point.Y, point.X + textRect.X + ButtonPadding, point.Y + textRect.Y +ButtonPadding), ButtonRadius, ButtonRadius), BorderPaint);
        
        skCanvas.DrawText(Text, point.X + ButtonPadding, point.Y + textRect.Y - ButtonPadding/2, Paint);
        
    }

    public Vector2 CalculateControlRect()
    {
        var bound = new SKRect();
        Paint.MeasureText(Text, ref bound);
        return new Vector2()
        {
            X = bound.Width + ButtonPadding,
            Y = bound.Height + ButtonPadding
        };
    }

    internal virtual void OnClick()
    {
        Click?.Invoke(this, EventArgs.Empty);
    }
}