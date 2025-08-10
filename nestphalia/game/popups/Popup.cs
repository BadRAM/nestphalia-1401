namespace nestphalia;

public abstract class Popup(Action closeAction)
{
    private Action _closeAction = closeAction;
    
    // This is supplied by implementations of Popup to be called when they are active.
    public abstract void Draw();

    protected virtual void Close()
    {
        PopupManager.Clear();
        Time.TimeScale = 1;
        Input.SetSuppressed(Input.SuppressionSource.Popup, false);
        _closeAction.Invoke();
    }
}