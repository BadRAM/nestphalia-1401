using WrenNET;

namespace nestphalia;

public class StructureDestroyedEvent : BattleEvent
{
    public Int2D StructurePosition;
    
    public StructureDestroyedEvent(Int2D pos, WrenHandle handle) : base(handle)
    {
        StructurePosition = pos;
    }

    public override bool Update()
    {
        Structure? s = World.GetTile(StructurePosition);
        if (s == null || s is Rubble)
        {
            Invoke();
            return true;
        }
        return false;
    }
}