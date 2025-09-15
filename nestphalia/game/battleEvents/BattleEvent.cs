using WrenNET;

namespace nestphalia;

// WrenEvents are objects that check for a given condition, and trigger a wren function when it occurs
public abstract class BattleEvent
{
    public WrenHandle Handle;

    public BattleEvent(WrenHandle handle)
    {
        Handle = handle;
    }

    ~BattleEvent()
    {
        WrenCommand.ReleaseWrenHandle(Handle);
    }
    
    // Return true to destroy event
    public abstract bool Update();

    public void Invoke()
    {
        WrenCommand.InvokeCallOnWrenHandle(Handle);
    }
}