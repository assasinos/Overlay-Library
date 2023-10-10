using System.Numerics;
using SkiaSharp;

namespace OverlayLibrary.Controls;

public class TextBoxControl : IControl
{
    
    public bool Interactive { get; set; } = true;
    
    
    public string Text { get; set; }
    private SKPaint Paint { get; set; }

    private int _cursorPosition;
    
    internal bool isFocused = false;

    private SKRect _rect;
    
    public TextBoxControl(string text, SKPaint paint)
    {
        Text = text;
        Paint = paint;
        //Calculate size to fit 8 characters
        _rect = new();
        paint.MeasureText("WWWWWWWW", ref _rect);
        _cursorPosition = text.Length;
    }
    
    private const float TextBoxPadding = 10f;
    private const float TextBoxRadius = 5f;
    
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

    private static readonly SKPaint CursorPaint = new SKPaint()
    {
        Color = SKColors.White
    };
    
    
    
    internal void RemoveCharacter()
    {
        if (_cursorPosition == 0) return;
        Text = Text.Remove(_cursorPosition - 1, 1);
        _cursorPosition--;
    }

    internal void InsertCharacter(string character)
    {
        Text = Text.Insert(_cursorPosition, character);
        _cursorPosition++;
    }

    
    
    
    
    private bool _cursorBlink = false;
    private int _cursorBlinkTimer = 0;

    public void DrawControl(SKPoint point, SKCanvas skCanvas)
    {
        var textRect = CalculateControlRect();
        //Draw background
        skCanvas.DrawRoundRect(new SKRoundRect(new SKRect(point.X, point.Y, point.X + textRect.X, point.Y + textRect.Y ), TextBoxRadius, TextBoxRadius), BackgroundPaint);
        
        //Draw border
        skCanvas.DrawRoundRect(new SKRoundRect(new SKRect(point.X, point.Y, point.X + textRect.X , point.Y + textRect.Y ), TextBoxRadius, TextBoxRadius), BorderPaint);
        
        skCanvas.DrawText(Text, point.X, point.Y + textRect.Y - TextBoxPadding/4, Paint);

        if (!isFocused) return;
        
        var cursorPosition = _cursorPosition;
        if (cursorPosition > Text.Length)
        {
            cursorPosition = Text.Length;
        }

        CursorPaint.Color = _cursorBlink ? SKColors.White : SKColors.Transparent;
        var cursorX = point.X + Paint.MeasureText(Text[..cursorPosition]);
        skCanvas.DrawLine(cursorX, point.Y + TextBoxPadding/2, cursorX, point.Y + textRect.Y, CursorPaint);
        
        _cursorBlinkTimer++;

        if (_cursorBlinkTimer <= 30) return;
        _cursorBlinkTimer = 0;
        _cursorBlink = !_cursorBlink;

    }

    //TODO: Add a Min and max width Or just static width
    public Vector2 CalculateControlRect()
    {
        return new Vector2()
        {
            X = _rect.Width + TextBoxPadding,
            Y = _rect.Height + TextBoxPadding/2
        };
    }
    
    
    
    
}