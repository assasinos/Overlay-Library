namespace OverlayLibrary.Hooks;
using static WinApi;

public class KeyboardHook
{
    private VK _registeredKey;

    public event EventHandler? KeyPressed;
    
    public KeyboardHook(VK registeredKey)
    {
        _registeredKey = registeredKey;
        var thread = new Thread(Start);
        thread.Start();
    }

    private void Start()
    {
        while (true)
        {
            
            if ((GetAsyncKeyState(_registeredKey) & 0x01) == 1 )
            {
                OnKeyPressed();
            }
            
            Thread.Sleep(10);
        }
        
    }

    protected virtual void OnKeyPressed()
    {
        KeyPressed?.Invoke(this, EventArgs.Empty);
    }
}