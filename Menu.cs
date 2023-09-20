using System.Numerics;
using OverlayLibrary.Controls;
using SkiaSharp;

namespace OverlayLibrary;

public class Menu
{
    #region Consts

    private const float MenuPadding = 10;
    private const float ControlBottomMargin = 10;
    private const float HeaderHeight = 20;
    private const float HeaderPadding = 5;

    private static readonly SKPaint MenuBorderPaint = new SKPaint()
    {
        Color = new SKColor(0, 0, 0, 255),
        Style = SKPaintStyle.Stroke
    };

    private static readonly SKPaint MenuRectPaint = new SKPaint()
    {
        Color = new SKColor(0, 0, 0, 150),
        Style = SKPaintStyle.Fill
    };
    
    private static readonly SKPaint MenuHeaderNamePaint = new SKPaint()
    {
        Color = SKColors.White,
        TextSize = 20
    };

    #endregion


    internal string Name { get; set; }

    #region Positions

    
    internal SKRect MenuRect { get; private set; }
    private SKRect _headerRect;
    private SKPoint _position;
    
    #endregion

    
    
    
    private readonly List<IControl> _menuControls = new();
    


    public Menu(string name, SKPoint position)
    {
        Name = name;
        _position = position;
        UpdateMenuRect();
    }
    
    public void AddControl(IControl control)
    {
        _menuControls.Add(control);
        UpdateMenuRect();
    }
    
    public void RemoveControl(IControl control)
    {
        _menuControls.Remove(control);
        UpdateMenuRect();
    }
    
    /// <summary>
    /// Calculates the size of the controls
    /// </summary>
    private Vector2 GetAllControlsRect()
    {
        
        
        var vec = new Vector2
        {
            //Todo: Maybe change it if the name is too long
            X = MenuHeaderNamePaint.MeasureText(Name)
        };

        foreach (var controlRect in _menuControls.Select(control => control.CalculateControlRect()))
        {
            //add some margin on bottom
            vec.Y += controlRect.Y + ControlBottomMargin;
            vec.X = Math.Max(vec.X, controlRect.X);
        }
        
        
        return vec;
    }

    private void UpdateMenuRect()
    {
        var allControlsRect = GetAllControlsRect();
        MenuRect = new SKRect(_position.X - MenuPadding, _position.Y - MenuPadding, _position.X + allControlsRect.X + MenuPadding * 2, _position.Y + allControlsRect.Y + MenuPadding * 2);

        _headerRect = new SKRect(MenuRect.Left, MenuRect.Top, MenuRect.Right, MenuRect.Top + HeaderHeight);
    }

    
    internal void Draw(SKCanvas skCanvas)
    {
        //Draw Menu background
        skCanvas.DrawRect(
            MenuRect,
            MenuRectPaint);
        
        
        //Draw Menu Border
        skCanvas.DrawRect(
            MenuRect,
            MenuBorderPaint
            );
        
        //Draw Menu Header
        
        //Draw header border
        skCanvas.DrawRect(
            _headerRect,
            MenuBorderPaint
            );
        
        //Draw menu name
        skCanvas.DrawText(Name, MenuRect.Left + HeaderPadding, 
            //Add some margin on top
            _position.Y + HeaderHeight/2.5f,
            MenuHeaderNamePaint
            );
        
        //Draw Each control
        var currentY = _position.Y + HeaderHeight;
        foreach (var control in _menuControls)
        {
            control.DrawControl(new SKPoint(_position.X, currentY), skCanvas);
            
            currentY += control.CalculateControlRect().Y + ControlBottomMargin;
        }
    }

    
    internal bool CheckIfHeaderClicked(Vector2 position) => _headerRect.Contains(position.X, position.Y);

    internal Vector2 CalculateHeaderOffset(Vector2 position) => new Vector2(_position.X - position.X, _position.Y - position.Y);

    internal void UpdatePosition(Vector2 mousePosition)
    {
        
        _position = new SKPoint(mousePosition.X, mousePosition.Y);
        UpdateMenuRect();
    }
}