using WrenNET;

namespace nestphalia;

// WrenEvents are objects that check for a given condition, and trigger a wren function when it occurs
public abstract class BattleEvent
{
    public WrenHandle Handle;
    private WrenCommand _wrenCommand;

    public BattleEvent(WrenHandle handle, WrenCommand wrenCommand)
    {
        Handle = handle;
        _wrenCommand = wrenCommand;
    }

    ~BattleEvent()
    {
        _wrenCommand.ReleaseWrenHandle(Handle);
    }
    
    // Return true to destroy event
    public abstract bool Update();

    public void Invoke()
    {
        _wrenCommand.InvokeCallOnWrenHandle(Handle);
    }
}