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
            if (Position == Target)
            {
                Retarget();
            }
        }
    }


    // public virtual void Update()
    // {
    //     // if the next tile in our path is adjacent and solid, then attack it
    //     
    //     // else, move towards next tile on path.
    //     
    //     // if we're at our final destination, ask for a new path. (Don't ask for a new path if we already have)
    //
    //     if (Position.X < -24 || Position.X > World.BoardWidth * 24 + 24 || Position.Y < -24 || Position.Y > World.BoardHeight * 24 + 24)
    //     {
    //         Console.WriteLine($"{Template.Name} had an adventure and was returned to board center.");
    //         Position = new Vector2(World.BoardWidth * 12, World.BoardHeight * 12);
    //     }
    //     
    //     double adjustedSpeed = Template.Speed;
    //     Structure? structure = World.GetTileAtPos(Position);
    //     if (Glued || (!IsFlying() && structure?.Team == Team && structure is Minefield)) adjustedSpeed *= 0.5;
    //
    //     if (!IsFlying()) Target = World.GetTileCenter(NavPath.NextTile(Position));
    //     
    //     Structure? t = World.GetTileAtPos(Position.MoveTowards(Target, Template.Range)); // TODO: make this respect minion range
    //     if (IsFlying() && t != World.GetTile(NavPath.Destination)) t = null; // cheeky intercept so flyers ignore all but their target.
    //
    //     if (t != null && t.PhysSolid(Team) && t.Team != Team)
    //     {
    //         if (Time.Scaled - _lastFiredTime > 60/Template.RateOfFire)
    //         {
    //             Template.Projectile.Instantiate(t, this);
    //             _lastFiredTime = Time.Scaled;
    //         }
    //     }
    //     else if (IsFlying())
    //     {
    //         Target = World.GetTileCenter(NavPath.Destination);
    //         Position = Position.MoveTowards(Target, adjustedSpeed * Time.DeltaTime);
    //         if (Position == Target)
    //         {
    //             Retarget();
    //         }
    //     }
    //     else
    //     {
    //         if (NavPath.Found && NavPath.TargetReached(Position))
    //         {
    //             Retarget();
    //         }
    //         Position = Position.MoveTowards(Target, adjustedSpeed * Time.DeltaTime);
    //     }
    //     Z = Position.Y + (IsFlying() ? 240 : 0);
    // }
}