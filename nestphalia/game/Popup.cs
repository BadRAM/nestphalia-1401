namespace nestphalia;

public abstract class Popup(Action closeAction)
{
    private Action _closeAction = closeAction;

    public abstract void Draw();

    public virtual void Close()
    {
        _closeAction.Invoke();
    }
}