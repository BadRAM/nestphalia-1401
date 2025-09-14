using WrenNET;

namespace nestphalia;

// WrenEvents are objects that check for a given condition, and trigger a wren function when it occurs
public abstract class BattleEvent
{
    public Action Event;

    public BattleEvent(Action triggerEvent)
    {
        Event = triggerEvent;
    }
    
    // Return true to destroy event
    public abstract bool Update();
}