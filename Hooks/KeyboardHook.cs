namespace OverlayLibrary.Hooks;
using static WinApi;

public sealed class KeyboardHook
{
    private readonly VK _registeredKey;
    
    private bool _shouldStop = false;
    
    public event EventHandler? KeyPressed;
    
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
            
            if ((GetAsyncKeyState(_registeredKey) & 0x01) == 1 )
            {
                OnKeyPressed();
            }
            
            Thread.Sleep(10);
        }
        
    }

    private void OnKeyPressed()
    {
        KeyPressed?.Invoke(this, EventArgs.Empty);
    }
    
    public void Stop()
    {
        _shouldStop = true;
    }
}