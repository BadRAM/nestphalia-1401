using System.Numerics;
using Raylib_cs;

namespace nestphalia;

public class BasicMinionTemplate : MinionTemplate
{
    public BasicMinionTemplate(string id, string name, string description, Texture2D texture, double maxHealth, double armor, double damage, double speed, float physicsRadius, double attackCooldown = 1) : base(id, name, description, texture, maxHealth, armor, damage, speed, physicsRadius, attackCooldown)
    {
    }

    public override void Instantiate(Team team, Vector2 position, NavPath? navPath)
    {
        Register(new BasicMinion(this, team, position, navPath));
    }
}

public class BasicMinion : Minion
{
    public BasicMinion(MinionTemplate template, Team team, Vector2 position, NavPath? navPath) : base(template, team, position, navPath) { }

    public override void Update()
    {
        UpdateNextPos();
        
        // if the next tile in our path is adjacent and solid, then attack it
        if (!TryAttack() && NavPath.Found)
        {
            // if we're at our final destination, ask for a new path. (Don't ask for a new path if we already have)
            if (NavPath.TargetReached(Position))
            {
                Retarget();
                PathFinder.RequestPath(NavPath);
            }
            // else, move towards next tile on path.
            Position = Position.MoveTowards(NextPos, AdjustedSpeed() * Time.DeltaTime);
        }
        
        Frenzy = false;
    }
}