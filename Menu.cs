using System.Numerics;
using OverlayLibrary.Controls;
using Silk.NET.Input;
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
    
    
    private static readonly SKPaint MenuActivePinButtonPaint = new SKPaint()
    {
        Color = new SKColor(0, 255, 0, 255),
        Style = SKPaintStyle.Fill
    };
    private static readonly SKPaint MenuInactivePinButtonPaint = new SKPaint()
    {
        Color = new SKColor(153, 153, 153, 255),
        Style = SKPaintStyle.Fill
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



    #region Properties

    private readonly List<IControl> _menuControls = new();
    internal string Name { get; }
    
    internal bool IsPinned { get; set; }

    #endregion


    #region Positions

    
    internal SKRect MenuRect { get; private set; } 
    private SKRect _headerRect;
    private SKPoint _position;
    
    #endregion
    

    public Menu(string name, SKPoint position, bool startPinned = true)
    {
        Name = name;
        _position = position;
        IsPinned = startPinned;
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
    
    public void RemoveControlAt(int index)
    {
        _menuControls.RemoveAt(index);
        UpdateMenuRect();
    }
    
    public void ClearControls()
    {
        _menuControls.Clear();
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
        
        //Draw Menu buttons
        
        //Draw pin button
        
        
        
        skCanvas.DrawCircle(MenuRect.Right - HeaderPadding - HeaderHeight/2.5f, _position.Y, HeaderHeight/2.5f, IsPinned ? MenuActivePinButtonPaint : MenuInactivePinButtonPaint);
        
        
        
        //Draw Each control
        var currentY = _position.Y + HeaderHeight;
        //In case the controls are changed while drawing
        var localMenuControls = _menuControls.ToArray();
        foreach (var control in localMenuControls)
        {
            control.DrawControl(new SKPoint(_position.X, currentY), skCanvas);
            
            currentY += control.CalculateControlRect().Y + ControlBottomMargin;
        }
    }

    
    internal bool ContainsButton()
    {
        return _menuControls.Any(x => x is ButtonControl);
    }
    

    internal IControl? CheckForInteractiveControlClicked(Vector2 position)
    {
        
        var currentY = _position.Y + HeaderHeight;
        //In case the controls are changed while drawing
        var localMenuControls = _menuControls.ToArray();
        foreach (var control in localMenuControls)
        {
            if (!control.Interactive)
            {
                currentY += control.CalculateControlRect().Y + ControlBottomMargin;
                continue;
            }

            var controlSize = control.CalculateControlRect();
            var controlRect = new SKRect(_position.X, currentY, _position.X + controlSize.X, currentY + controlSize.Y);
            
            //Implementation for each interactive control when clicked
            switch (control)
            {
                case ButtonControl buttonControl:
                {
                    if (controlRect.Contains(position.X, position.Y))
                    {
                        buttonControl.OnClick();
                        //There should be only one button in this place
                        return null;
                    }
                    break;
                }
                case TextBoxControl textBoxControl:
                    if (controlRect.Contains(position.X, position.Y))
                    {
                        textBoxControl.isFocused = true;
                        return textBoxControl;
                    }
                    break;
                default:
                    throw new NotImplementedException("This control is not implemented");
            }
            
            
            

            
            currentY += control.CalculateControlRect().Y + ControlBottomMargin;
        }
        //If no control was clicked, unfocus textboxes
        foreach (var control in localMenuControls)
        {
            if (control is TextBoxControl textBoxControl)
            {
                textBoxControl.isFocused = false;
            }
        }

        return null;
    }
    
    internal bool CheckIfHeaderClicked(Vector2 position) => _headerRect.Contains(position.X, position.Y) && !CheckIfPinClicked(position);

    internal bool CheckIfPinClicked(Vector2 position) =>
        //X
        !(position.X < (MenuRect.Right - HeaderPadding - HeaderHeight / 2.5f) - HeaderHeight / 2.5f) &&
        !(position.X > (MenuRect.Right - HeaderPadding - HeaderHeight / 2.5f) + HeaderHeight / 2.5f) &&
        //Y
        !(position.Y < _position.Y - HeaderHeight / 2.5f) && 
        !(position.Y > _position.Y + HeaderHeight / 2.5f);

    internal Vector2 CalculateHeaderOffset(Vector2 position) => new Vector2(_position.X - position.X, _position.Y - position.Y);

    internal void UpdatePosition(Vector2 mousePosition)
    {
        
        _position = new SKPoint(mousePosition.X, mousePosition.Y);
        UpdateMenuRect();
    }
}