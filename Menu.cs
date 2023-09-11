using System.Numerics;
using OverlayLibrary.Controls;
using SkiaSharp;
using static OverlayLibrary.WinApi;

namespace OverlayLibrary;

public class Menu
{
    private const int MenuPadding = 10;
    private const int ControlBottomMargin = 10;
    
    
    
    public string Name { get; set; }
    
    public RECT MenuRect { get; set; }

    private readonly List<IControl> _menuControls = new();
    
    private SKPoint _position;

    public Menu(string name, SKPoint position)
    {
        Name = name;
        _position = position;
    }


    public void AddControl(IControl control)
    {
        _menuControls.Add(control);
    }
    
    public void RemoveControl(IControl control)
    {
        _menuControls.Remove(control);
    }
    
    /// <summary>
    /// Calculates the size of the controls
    /// </summary>
    /// <returns>
    /// Minimal width and height of the menu to fit all controls
    /// </returns>
    private Vector2 CalculateAllControlsRect()
    {
        var vec = new Vector2();

        
        
        foreach (var controlRect in _menuControls.Select(control => control.CalculateControlRect()))
        {
            //add some margin on bottom
            vec.Y += controlRect.Y + ControlBottomMargin;
            vec.X = Math.Max(vec.X, controlRect.X);
        }
        
        
        return vec;
    }

    public void Draw(SKCanvas skCanvas)
    {
        var allControlsRect = CalculateAllControlsRect();
        //Draw Menu background
        
        var menuRect = new SKRect(_position.X - MenuPadding, _position.Y - MenuPadding, _position.X + allControlsRect.X + MenuPadding * 2, _position.Y + allControlsRect.Y + MenuPadding * 2);
        
        skCanvas.DrawRect(
        menuRect,
            new SKPaint()
        {
            Color = new SKColor(0, 0, 0, 150),
            Style = SKPaintStyle.Fill
        });
        
        
        //Draw Menu Border
        skCanvas.DrawRect(
            menuRect,
            new SKPaint()
            {
                Color = new SKColor(0, 0, 0, 255),
                Style = SKPaintStyle.Stroke
            });
        
        //Draw Menu Header with name
        
        
        //Draw Each control
        var currentY = _position.Y;
        foreach (var control in _menuControls)
        {
            control.DrawControl(new SKPoint(_position.X, currentY), skCanvas);
            
            currentY += control.CalculateControlRect().Y + ControlBottomMargin;
        }
    }
}