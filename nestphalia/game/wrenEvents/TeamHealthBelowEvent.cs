namespace nestphalia;

// Triggers once, when team's health percentage drops below threshold
public class TeamHealthBelowEvent : BattleEvent
{
    public Team Team;
    public double HealthThreshold;

    public TeamHealthBelowEvent(Team team, double healthThreshold, Action triggerEvent) : base(triggerEvent)
    {
        Team = team;
        HealthThreshold = healthThreshold;
    }

    public override bool Update()
    {
        if (Team.Health / Team.MaxHealth < HealthThreshold)
        {
            Event.Invoke();
            return true;
        }
        return false;
    }
}