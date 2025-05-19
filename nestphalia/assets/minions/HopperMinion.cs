using System.Numerics;
using Raylib_cs;

namespace nestphalia;

public class HopperMinionTemplate : MinionTemplate
{
    public HopperMinionTemplate(string id, string name, string description, Texture2D texture, double maxHealth, double armor, double damage, double speed, float physicsRadius, double attackCooldown = 1) : base(id, name, description, texture, maxHealth, armor, damage, speed, physicsRadius, attackCooldown)
    {
    }
    
    public override void Instantiate(Team team, Vector2 position, NavPath? navPath)
    {
        Register(new HopperMinion(this, team, position, navPath));
    }
}
    
public class HopperMinion : Minion
{
    public HopperMinion(MinionTemplate template, Team team, Vector2 position, NavPath? navPath) : base(template, team, position, navPath)
    {
    }
    
    public override void Update()
    {
        if (State is Move)
        {
            UpdateNextPos(); // we need to do this or else Move.Update() will switch to attack mode before we have the chance to intercept
            Int2D? ahead = NavPath.LookAhead(1);
            if (ahead != null)
            {
                Structure? structureAhead = World.GetTile((Int2D)ahead);

                if (structureAhead == null || 
                    (!structureAhead.PhysSolid() && 
                     (structureAhead.Team != Team || structureAhead is not Minefield)))
                {
                    Vector2 to = World.GetTileCenter((Int2D)ahead);
                    to += new Vector2(World.Random.Next(17) - 8, World.Random.Next(23) - 11);
                    State = new Jump(this, to, 0.25, 0.5, 0.25);
                }
            }
        }
        
        base.Update();
    }
}