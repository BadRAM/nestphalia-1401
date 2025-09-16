using WrenNET;

namespace nestphalia;

// Triggers once, when team's health percentage drops below threshold
public class TeamHealthBelowEvent : BattleEvent
{
    public Team Team;
    public double HealthThreshold;

    public TeamHealthBelowEvent(Team team, double healthThreshold, WrenHandle handle, WrenCommand wrenCommand) : base(handle, wrenCommand)
    {
        Team = team;
        HealthThreshold = healthThreshold;
    }

    public override bool Update()
    {
        if (Team.Health / Team.MaxHealth < HealthThreshold)
        {
            Invoke();
            return true;
        }
        return false;
    }
}