using WrenNET;

namespace nestphalia;

public class BattleEndEvent : BattleEvent
{
    public string WinnerFilter;
    
    public BattleEndEvent(string winnerFilter, WrenHandle handle, WrenCommand wrenCommand) : base(handle, wrenCommand)
    {
        WinnerFilter = winnerFilter;
    }

    public override bool Update()
    {
        if (WinnerFilter == "" || (World.GetTeam(WinnerFilter)?.Health ?? -1) > 0)
        {
            Invoke();
            return true;
        }
        return false;
    }
}