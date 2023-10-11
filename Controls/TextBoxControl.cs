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
        //TODO: Make this dynamic
        _rect = new();
        paint.MeasureText("WWWWWWWW", ref _rect);
        
        
        _cursorPosition = text.Length;
        _indexOfLastCharacterToDisplay = _cursorPosition;
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
    
    internal void MoveCursorLeft()
    {
        if (_cursorPosition == 0) return;
        _cursorPosition--;
        if (_cursorPosition < _indexOfLastCharacterToDisplay - 8)
        {
            _indexOfLastCharacterToDisplay--;
        }
    }
    
    internal void MoveCursorRight()
    {
        if (_cursorPosition == Text.Length) return;
        _cursorPosition++;
        if (_cursorPosition > _indexOfLastCharacterToDisplay)
        {
            _indexOfLastCharacterToDisplay++;
        }
    }

    
    
    
    
    private bool _cursorBlink = false;
    private int _cursorBlinkTimer = 0;
    
    private int _indexOfLastCharacterToDisplay;

    public void DrawControl(SKPoint point, SKCanvas skCanvas)
    {
        var textRect = CalculateControlRect();
        //Draw background
        skCanvas.DrawRoundRect(new SKRoundRect(new SKRect(point.X, point.Y, point.X + textRect.X, point.Y + textRect.Y ), TextBoxRadius, TextBoxRadius), BackgroundPaint);
        
        //Draw border
        skCanvas.DrawRoundRect(new SKRoundRect(new SKRect(point.X, point.Y, point.X + textRect.X , point.Y + textRect.Y ), TextBoxRadius, TextBoxRadius), BorderPaint);
        
        //Display 8 characters of text based on cursor position

        var textToDisplay = Text[(_indexOfLastCharacterToDisplay-8).._indexOfLastCharacterToDisplay];
        skCanvas.DrawText(textToDisplay, point.X + TextBoxPadding, point.Y + textRect.Y - TextBoxPadding/4, Paint);
        

        if (!isFocused) return;
        
        var cursorPosition = _cursorPosition;
        if (cursorPosition > Text.Length)
        {
            cursorPosition = Text.Length;
        }

        CursorPaint.Color = _cursorBlink ? SKColors.White : SKColors.Transparent;
        //TODO: Get cursor position
        //skCanvas.DrawLine(cursorX, point.Y + TextBoxPadding/2, cursorX, point.Y + textRect.Y, CursorPaint);
        
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