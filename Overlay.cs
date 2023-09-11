using System.Diagnostics;
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
    private readonly SKSurface _skSurface;
    public readonly SKCanvas SkCanvas;
    private readonly Process _thisProcess = Process.GetCurrentProcess();
    
    private List<Menu> _menus = new();

    public Overlay(Process overlaidProcess)
    {
        _overlaidProcess = overlaidProcess;
        
        

        var options = WindowOptions.Default;
        
        //Set the size of the window to the size of the process
        var rect = new WinApi.RECT();
        WinApi.GetWindowRect(_overlaidProcess.MainWindowHandle, out rect);
        options.Size = CalculateWindowSize(rect);

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
            
            WinApi.SetWindowLong(_thisProcess.MainWindowHandle,WinApi.GWL_EXSTYLE , WinApi.GetWindowLong(_thisProcess.MainWindowHandle, WinApi.GWL_EXSTYLE) | WinApi.WS_EX_LAYERED | WinApi.WS_EX_TRANSPARENT);
        };
        
        _window.Initialize();
        
        using var grGlInterface = GRGlInterface.Create((name => _window.GLContext!.TryGetProcAddress(name, out var addr) ? addr : (IntPtr) 0));
        grGlInterface.Validate();

        _grContext = GRContext.CreateGl(grGlInterface);
        var renderTarget = new GRBackendRenderTarget(800, 600, 0, 8, new GRGlFramebufferInfo(0, 0x8058)); // 0x8058 = GL_RGBA8`
        _skSurface = SKSurface.Create(_grContext, renderTarget, GRSurfaceOrigin.BottomLeft, SKColorType.Rgba8888);
        SkCanvas = _skSurface.Canvas;


    }
    

    private const int UpdateInterval = 200;
    private async Task UpdatePosition()
    {
        while (true)
        {
            if (_overlaidProcess.HasExited)
            {
                Console.WriteLine("Exited");
                Dispose();
            }
            
            //Retrieve the position of the process
            var rect = new WinApi.RECT();
            WinApi.GetWindowRect(_overlaidProcess.MainWindowHandle, out rect);
            var size = CalculateWindowSize(rect);

            WinApi.SetWindowPos(
                _thisProcess.MainWindowHandle,
                //Optional
                IntPtr.Zero,
                rect.Left,
                rect.Top,
                size.X,
                size.Y,
                0
            );
            
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
    
    
    
}