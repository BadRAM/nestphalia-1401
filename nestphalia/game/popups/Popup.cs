namespace nestphalia;

public abstract class Popup
{
    // Every popup should offer a closeAction to handle consequences of the popup. The base class does not implement it
    // because different classes want actions with different arguments, so they can return relevant data.
    
    // This is supplied by implementations of Popup to be called when they are active.
    public abstract void Draw();

    // Remember to invoke closeActions AFTER calling Close(), because if the closeAction opens a new popup, Close will clear the new popup.
    protected virtual void Close()
    {
        PopupManager.Clear();
        Time.TimeScale = 1;
        Input.SetSuppressed(Input.SuppressionSource.Popup, false);
    }
}