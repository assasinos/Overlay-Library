namespace OverlayLibrary.Hooks;
using static WinApi;

public sealed class KeyboardHook
{
    private readonly VK _registeredKey;
    
    private bool _shouldStop = false;
    
    public event EventHandler? KeyDown;
    public event EventHandler? KeyUp;
    
    
    public KeyboardHook(VK registeredKey)
    {
        _registeredKey = registeredKey;
        var thread = new Thread(Start)
        {
            IsBackground = true,
            Name = "KeyboardHook"
        };
        thread.Start();
    }
    private void Start()
    {
        while (!_shouldStop)
        {

            if (!IsKeyDown(_registeredKey))
            {
                Thread.Sleep(10);
                continue;
            }
            OnKeyDown();

            while (IsKeyDown(_registeredKey))
            {
                Thread.Sleep(10);
            }
            OnKeyUp();
            
            Thread.Sleep(10);
        }
        
    }
    
    

    private void OnKeyDown()
    {
        KeyDown?.Invoke(this, EventArgs.Empty);
    }
    private void OnKeyUp()
    {
        KeyUp?.Invoke(this, EventArgs.Empty);
    }
    
    public void Stop()
    {
        _shouldStop = true;
    }
}