using SkiaSharp;
using static OverlayLibrary.WinApi;

namespace OverlayLibrary;

public class Menu
{
    
    public string Name { get; set; }
    
    public RECT MenuRect { get; set; }

    private readonly List<IControl> _menuControls;
    
    private SKPoint _position;

    public Menu(string name, List<IControl> menuControls, SKPoint position)
    {
        Name = name;
        _menuControls = menuControls;
        _position = position;
    }


    public RECT CalculateAllControlsRect()
    {
        var rect = new RECT();
        
        foreach (var control in _menuControls)
        {
            var controlRect = control.CalculateControlRect();
            rect.Left = Math.Min(rect.Left, controlRect.Left);
            rect.Top = Math.Min(rect.Top, controlRect.Top);
            rect.Right = Math.Max(rect.Right, controlRect.Right);
            //Add 10 pixels to bottom to make it look better
            rect.Bottom = Math.Max(rect.Bottom, controlRect.Bottom + 10);
        }

        return rect;
    }
}