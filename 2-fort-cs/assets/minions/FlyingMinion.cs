using System.Numerics;
using ZeroElectric.Vinculum;

namespace _2_fort_cs;

public class FlyingMinionTemplate : MinionTemplate
{
    public FlyingMinionTemplate(string id, string name, string description, Texture texture, double maxHealth, double armor, double damage, double speed, float physicsRadius, double attackCooldown = 1) : base(id, name, description, texture, maxHealth, armor, damage, speed, physicsRadius, attackCooldown)
    {
    }
    
    public override void Instantiate(Vector2 position, Team team, NavPath? navPath)
    {
        FlyingMinion m = new FlyingMinion(this, team, position, navPath);
        World.Minions.Add(m);
        World.Sprites.Add(m);
    }
}

public class FlyingMinion : Minion
{
    public FlyingMinion(MinionTemplate template, Team team, Vector2 position, NavPath? navPath) : base(template, team, position, navPath)
    {
        IsFlying = true;
    }

    public override void Update()
    {
        
        // if the next tile in our path is adjacent and solid, then attack it
        if (!TryAttack())
        {
            Target = World.GetTileCenter(NavPath.Destination);
            Position = Position.MoveTowards(Target, AdjustedSpeed() * Time.DeltaTime);
            if (NavPath.TargetReached(Position))
            {
                Retarget();
            }
        }

        Frenzy = false;
    }

    public override void SetTarget(Int2D target)
    {
        NavPath.Reset();
        NavPath.Start = World.PosToTilePos(Position);
        NavPath.Destination = target;
    }
}