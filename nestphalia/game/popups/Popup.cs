namespace nestphalia;

public abstract class Popup
{
    // Every popup should offer a closeAction to handle consequences of the popup. The base class does not implement it
    // because different classes want actions with different arguments, so they can return relevant data.
    
    // This is supplied by implementations of Popup to be called when they are active.
    public abstract void Draw();

    // Remember to invoke closeActions AFTER calling Close(), because if the closeAction opens a new popup, Close will
    // close the new popup instead of the old one.
    protected virtual void Close()
    {
        PopupManager.Remove(this);
        Time.Paused = false;
    }
}