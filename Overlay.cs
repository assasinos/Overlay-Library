using System.Diagnostics;
using System.Numerics;
using OverlayLibrary.Controls;
using OverlayLibrary.Hooks;
using Silk.NET.Input;
using Silk.NET.Maths;
using Silk.NET.Windowing;
using Silk.NET.Windowing.Glfw;
using SkiaSharp;


namespace OverlayLibrary;

public class Overlay : IDisposable, IAsyncDisposable
{
    private readonly Process _overlaidProcess;
    private readonly IWindow _window;
    private readonly GRContext _grContext;
    private SKSurface _skSurface;
    private SKCanvas _skCanvas;
    private readonly Process _thisProcess = Process.GetCurrentProcess();
    
    private int _defaultWindowLong;
    private bool _isDragging;
    private bool _isOverlayActive;
    
    
    private IControl? _activeControl = null;
    
    
    //Expose if someone would need to do something with it
    public KeyboardHook KeyboardHook = null!;
    
    public bool isDisposed;
    
    private readonly List<Menu> _menus = new();

    public Overlay(Process overlaidProcess, WinApi.VK overlayKey = WinApi.VK.INSERT)
    {
        _overlaidProcess = overlaidProcess;
        
        

        var options = WindowOptions.Default;
        
        //Set the size of the window to the size of the process
        WinApi.GetWindowRect(_overlaidProcess.MainWindowHandle, out var rect);
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
            
            Task.Run(async () =>
            {
                await UpdatePosition();
            });

            #region Mouse
            
            
            if ( _window.CreateInput().Mice[0] is { } mouse)
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

            #endregion

            #region Keyboard

            if (_window.CreateInput().Keyboards[0] is { } keyboard )
            {
                //Keyboard hook, works even if the overlay is not focused
                KeyboardHook = new KeyboardHook(overlayKey);
            
                KeyboardHook.KeyPressed += KeyboardHookOnKeyPressed;

            
                //Keyboard hook, works only if the overlay is focused
                keyboard.KeyChar += KeyboardOnKeyChar;
                keyboard.KeyDown += KeyboardOnKeyDown;
            }



            
            #endregion
            
            
            _defaultWindowLong = WinApi.GetWindowLong(_thisProcess.MainWindowHandle, WinApi.GWL_EXSTYLE);
            MakeWindowTransparent();
            
        };
        
        _window.Initialize();
        
        using var grGlInterface = GRGlInterface.Create((name => _window.GLContext!.TryGetProcAddress(name, out var addr) ? addr : 0));
        grGlInterface.Validate();

        _grContext = GRContext.CreateGl(grGlInterface);
        var renderTarget = new GRBackendRenderTarget(size.X, size.Y, 0, 8, new GRGlFramebufferInfo(0, 0x8058)); // 0x8058 = GL_RGBA8`
        _skSurface = SKSurface.Create(_grContext, renderTarget, GRSurfaceOrigin.BottomLeft, SKColorType.Rgba8888);
        _skCanvas = _skSurface.Canvas;


    }

    private void KeyboardOnKeyChar(IKeyboard keyboard, char c)
    {
        if (_activeControl is null) return;

        switch (_activeControl)
        {
            case TextBoxControl textBoxControl:
                textBoxControl.InsertCharacter(c.ToString());
                break;
            default:
                throw new NotImplementedException($" {_activeControl.GetType()} OnKeyChar case is not implemented");
        }
    }

    private async void KeyboardOnKeyDown(IKeyboard keyboard, Key key, int arg2)
    {
        if (_activeControl is null) return;

        switch (_activeControl)
        {
            case TextBoxControl textBoxControl:
                if (key != Key.Backspace) return;
                
                //IsKeyPressed of IKeyboard is not working properly
                //Seems to always return true
                while (WinApi.IsKeyDown(WinApi.VK.BACK))
                {
                    textBoxControl.RemoveCharacter();
                    await Task.Delay(100);
                }
                break;
            default:
                throw new NotImplementedException($" {_activeControl.GetType()} OnKeyDown case is not implemented");
        }
        
    }

    #region Events
    
    
    private void KeyboardHookOnKeyPressed(object? sender, EventArgs e)
    {
        switch (_isOverlayActive)
        {
            case true:
                MakeWindowTransparent();
                break;
            default:
                RevertWindowTransparency();
                break;
        }

        _isOverlayActive = !_isOverlayActive;
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

        foreach (var menu in _menus.Where(menu => menu.CheckIfPinClicked(position)))
        {
            menu.IsPinned = !menu.IsPinned;
        }

        foreach (var menu in _menus.Where(menu => menu.ContainsButton()))
        {
            _activeControl = menu.CheckForInteractiveControlClicked(position);
        }
        
    }
    
    #endregion
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

            #region Minimized

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


            #endregion

            
            if (overlaidRect != overlayRect)
            {
                var size = CalculateWindowSize(overlaidRect);
                
                ChangeWindowSize(overlaidRect,size);
            }
            
            
                
            await Task.Delay(UpdateInterval);
        }
    }
    
    
    

    
    




    #region Draw

    private void DrawOneFrame(double delta)
    {
        _skCanvas.Clear(SKColors.Transparent);
        foreach (var menu in _menus)
        {
            if (menu.IsPinned || _isOverlayActive)
            {
                menu.Draw(_skCanvas);
            }
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


    #region Overlay Functions

    //Just Expose Run function
    public void Run()
    {
        _window.Run();
    }

    
    
    private Vector2D<int> CalculateWindowSize(WinApi.RECT rect)
    {
        var width = rect.Right - rect.Left;
        var height = rect.Bottom - rect.Top;
        return new Vector2D<int>(width, height);
    }
    
    
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
        _skCanvas = _skSurface.Canvas;
    }
    
    private void MakeWindowTransparent()
    {
        WinApi.SetWindowLong(_thisProcess.MainWindowHandle,WinApi.GWL_EXSTYLE , _defaultWindowLong | WinApi.WS_EX_LAYERED | WinApi.WS_EX_TRANSPARENT);
        WinApi.SetForegroundWindow(_overlaidProcess.MainWindowHandle);
    }
    
    private void RevertWindowTransparency()
    {
        WinApi.SetWindowLong(_thisProcess.MainWindowHandle,WinApi.GWL_EXSTYLE , _defaultWindowLong);
        WinApi.SetForegroundWindow(_thisProcess.MainWindowHandle);
    }

    #endregion


    public async ValueTask DisposeAsync()
    {
        await CastAndDispose(_overlaidProcess);
        _window.Close();
        await CastAndDispose(_window);
        await CastAndDispose(_grContext);
        await CastAndDispose(_skSurface);
        await CastAndDispose(_skCanvas);
        await CastAndDispose(_thisProcess);
        isDisposed = true;

        return;

        static async ValueTask CastAndDispose(IDisposable resource)
        {
            if (resource is IAsyncDisposable resourceAsyncDisposable)
                await resourceAsyncDisposable.DisposeAsync();
            else
                resource.Dispose();
        }
    }
    public void Dispose()
    {
        _overlaidProcess.Dispose();
        _window.Close();
        _window.Dispose();
        _grContext.Dispose();
        _skSurface.Dispose();
        _skCanvas.Dispose();
        KeyboardHook.Stop();
        isDisposed = true;
        
    }
}