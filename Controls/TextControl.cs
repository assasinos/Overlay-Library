using SkiaSharp;

namespace OverlayLibrary;

public class TextControl : IControl
{
    
    private string Text { get; set; }

    private SKPaint Paint { get; set; }
    
    public TextControl(string text, SKPaint paint)
    {
        Text = text;
        Paint = paint;
    }

    void UpdateText(string text)
    {
        Text = text;
    }
    
    void UpdatePaint(SKPaint paint)
    {
        Paint = paint;
    }
    

    
    public WinApi.RECT CalculateControlRect()
    {

        var bound = new SKRect();
        Paint.MeasureText(Text, ref bound);
        return new WinApi.RECT()
        {
            Left = (int)bound.Left,
            Top = (int)bound.Top,
            Right = (int)bound.Right,
            Bottom = (int)bound.Bottom
        };
    }
}