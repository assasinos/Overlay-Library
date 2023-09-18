using System.Diagnostics;
using System.Numerics;
using Silk.NET.Input;
using Silk.NET.Maths;
using Silk.NET.Windowing;
using Silk.NET.Windowing.Glfw;
using SkiaSharp;


namespace OverlayLibrary;

public class Overlay : IDisposable
{
    private readonly Process _overlaidProcess;
    private readonly IWindow _window;
    private readonly GRContext _grContext;
    private SKSurface _skSurface;
    public SKCanvas SkCanvas;
    private readonly Process _thisProcess = Process.GetCurrentProcess();

    private int _defaultWindowLong;
    private bool _isDragging;
    
    private List<Menu> _menus = new();

    public Overlay(Process overlaidProcess)
    {
        _overlaidProcess = overlaidProcess;
        
        

        var options = WindowOptions.Default;
        
        //Set the size of the window to the size of the process
        var rect = new WinApi.RECT();
        WinApi.GetWindowRect(_overlaidProcess.MainWindowHandle, out rect);
        var size = CalculateWindowSize(rect);
        options.Size = size;

        options.Position = new(rect.Left, rect.Top);
        
        options.Title = "Overlay";
        
        //Display above all other windows
        options.TopMost = true;
        //To enable transparency
        options.TransparentFramebuffer = true;
        
        //Should be enough
        options.PreferredStencilBufferBits = 8;
        options.PreferredBitDepth = new Vector4D<int>(8, 8, 8, 8);
        
        options.WindowBorder = WindowBorder.Hidden;
        
        
        
        GlfwWindowing.Use();
        _window = Window.Create(options);

        _window.Load += () =>
        {
            _window.Render += DrawOneFrame;
            UpdatePosition();
            
            var mouse = _window.CreateInput()?.Mice[0];

            
            if (mouse is not null)
            {

                mouse.MouseDown += MouseOnDown;
                mouse.MouseUp += (_, mouseButton) =>
                {
                    if (mouseButton == MouseButton.Left)
                    {
                        _isDragging = false;
                    }
                };

            }
            
            
            _defaultWindowLong = WinApi.GetWindowLong(_thisProcess.MainWindowHandle, WinApi.GWL_EXSTYLE);
            //MakeWindowTransparent();
            
        };
        
        _window.Initialize();
        
        using var grGlInterface = GRGlInterface.Create((name => _window.GLContext!.TryGetProcAddress(name, out var addr) ? addr : (IntPtr) 0));
        grGlInterface.Validate();

        _grContext = GRContext.CreateGl(grGlInterface);
        var renderTarget = new GRBackendRenderTarget(size.X, size.Y, 0, 8, new GRGlFramebufferInfo(0, 0x8058)); // 0x8058 = GL_RGBA8`
        _skSurface = SKSurface.Create(_grContext, renderTarget, GRSurfaceOrigin.BottomLeft, SKColorType.Rgba8888);
        SkCanvas = _skSurface.Canvas;


    }

    private async void MouseOnDown(IMouse mouse, MouseButton mouseButton)
    {
        if (mouseButton != MouseButton.Left) return;

        var position = mouse.Position;
        
        foreach (var menu in _menus.Where(menu => menu.CheckIfHeaderClicked(position)))
        {
            _isDragging = true;
            await DragMenu(mouse, menu, menu.CalculateHeaderOffset(position));
            break;
        }
    }
    

    private async Task DragMenu(IMouse mouse, Menu menu, Vector2 offset)
    {
        while (_isDragging)
        {
            var newPosition = new Vector2()
            {
                X= Math.Clamp(mouse.Position.X + offset.X, 20 - menu.MenuRect.Width,_window.Size.X),
                Y= Math.Clamp(mouse.Position.Y + offset.Y, 0,_window.Size.Y),
            };
            
            menu.UpdatePosition(newPosition);
            
            
            await Task.Delay(10);
        }
    }


    private const int UpdateInterval = 200;
    private async Task UpdatePosition()
    {
        while (true)
        {
            if (_overlaidProcess.HasExited)
            {
                Dispose();
            }
            
            //Retrieve the position of the process
            WinApi.GetWindowRect(_overlaidProcess.MainWindowHandle, out var overlaidRect);
            WinApi.GetWindowRect(_thisProcess.MainWindowHandle, out var overlayRect);
            
            //Check if window is minimized
            
            //Make it better and work with menu drag
            
            // var activeWindow = WinApi.GetForegroundWindow();
            // if ( activeWindow != _overlaidProcess.MainWindowHandle)
            // {
            //     if (overlayRect.Top == -3200)
            //     {
            //         await Task.Delay(UpdateInterval);
            //         continue;
            //     }
            //     WinApi.SetWindowPos(
            //         _thisProcess.MainWindowHandle,
            //         //Optional
            //         IntPtr.Zero,
            //         -3200,
            //         -3200,
            //         0,
            //         0,
            //         0
            //     );
            //     await Task.Delay(UpdateInterval);
            //     continue;
            // }
            //
            


            if (overlaidRect != overlayRect)
            {
                var size = CalculateWindowSize(overlaidRect);
                
                ChangeWindowSize(overlaidRect,size);
            }

            


            
                
            await Task.Delay(UpdateInterval);
        }
    }
    
    
    
    //Just Expose Run function
    public async Task Run()
    {
        _window.Run();
    }
    
    
    //Expose Render event
    public event Action<double> Render
    {
        add
        {
            if (_window is null)
            {
                throw new Exception("Window is not initialized");
            }
            _window.Render += value;
        }
        remove
        {
            if (_window is null)
            {
                throw new Exception("Window is not initialized");
            }
            _window.Render -= value;
        }
    }


    
    
    
    public void Dispose()
    {
        _overlaidProcess.Dispose();
        _window?.Close();
        _window?.Dispose();
        _grContext?.Dispose();
        _skSurface?.Dispose();
        SkCanvas?.Dispose();
    }

    private Vector2D<int> CalculateWindowSize(WinApi.RECT rect)
    {
        var width = rect.Right - rect.Left;
        var height = rect.Bottom - rect.Top;
        return new Vector2D<int>(width, height);
    }

    #region Draw

    private void DrawOneFrame(double delta)
    {
        SkCanvas.Clear(SKColors.Transparent);
        foreach (var menu in _menus)
        {
            menu.Draw(SkCanvas);
        }
        _skSurface.Flush();
    }

    #endregion
    
    

    #region Menu

    public void AddMenu(Menu menu)
    {
        _menus.Add(menu);
    }
    
    public void RemoveMenu(Menu menu)
    {
        _menus.Remove(menu);
    }

    public void RemoveMenuByName(string name)
    {
        _menus.RemoveAll(m => m.Name == name);
    }
    #endregion
    
    private void ChangeWindowSize(WinApi.RECT position,Vector2D<int> size)
    {
        WinApi.SetWindowPos(
            _thisProcess.MainWindowHandle,
            //Optional
            IntPtr.Zero,
            position.Left,
            position.Top,
            size.X,
            size.Y,
            0
        );
                
        var renderTarget = new GRBackendRenderTarget(size.X, size.Y, 0, 8, new GRGlFramebufferInfo(0, 0x8058));
        _skSurface = SKSurface.Create(_grContext, renderTarget, GRSurfaceOrigin.BottomLeft, SKColorType.Rgba8888);
        SkCanvas = _skSurface.Canvas;
    }
    
    private void MakeWindowTransparent()
    {
        WinApi.SetWindowLong(_thisProcess.MainWindowHandle,WinApi.GWL_EXSTYLE , _defaultWindowLong | WinApi.WS_EX_LAYERED | WinApi.WS_EX_TRANSPARENT);
    }
    
    private void RevertWindowTransparency()
    {
        WinApi.SetWindowLong(_thisProcess.MainWindowHandle,WinApi.GWL_EXSTYLE , _defaultWindowLong);
    }
    
    
}