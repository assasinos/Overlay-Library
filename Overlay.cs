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
    public readonly SKCanvas _skCanvas;
    


    public Overlay(Process overlaidProcess)
    {
        _overlaidProcess = overlaidProcess;

        var options = WindowOptions.Default;
        
        //Set the size of the window to the size of the process
        var rect = new WinApi.RECT();
        WinApi.GetWindowRect(_overlaidProcess.MainWindowHandle, out rect);
        options.Size = new(rect.Right - rect.Left, rect.Bottom - rect.Top);

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
            _window.Render += d =>
            {
                _skCanvas.Clear(SKColors.Transparent);
                
                _skCanvas.Flush();
            };
        };
        
        _window.Initialize();
        
        using var grGlInterface = GRGlInterface.Create((name => _window.GLContext!.TryGetProcAddress(name, out var addr) ? addr : (IntPtr) 0));
        grGlInterface.Validate();

        _grContext = GRContext.CreateGl(grGlInterface);
        var renderTarget = new GRBackendRenderTarget(800, 600, 0, 8, new GRGlFramebufferInfo(0, 0x8058)); // 0x8058 = GL_RGBA8`
        _skSurface = SKSurface.Create(_grContext, renderTarget, GRSurfaceOrigin.BottomLeft, SKColorType.Rgba8888);
        _skCanvas = _skSurface.Canvas;
        

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
        _window?.Dispose();
        _grContext?.Dispose();
        _skSurface?.Dispose();
        _skCanvas?.Dispose();
    }

}