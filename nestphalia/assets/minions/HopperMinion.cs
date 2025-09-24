using System.Numerics;
using Newtonsoft.Json.Linq;
using Raylib_cs;

namespace nestphalia;

public class HopperMinionTemplate : MinionTemplate
{
    public HopperMinionTemplate(JObject jObject) : base(jObject)
    {
    }
    
    public override Minion Instantiate(Team team, Vector3 position, NavPath? navPath)
    {
        Minion m = new HopperMinion(this, team, position, navPath);
        World.RegisterMinion(m);
        return m;
    }
}

public class HopperMinion : Minion
{
    public HopperMinion(MinionTemplate template, Team team, Vector3 position, NavPath? navPath) : base(template, team,
        position, navPath)
    {
    }

    public override void Update()
    {
        if (State is Move && !Status.Has("Glued"))
        {
            UpdateNextPos(); // we need to do this or else Move.Update() will switch to attack mode before we have the chance to intercept
            Int2D? ahead = NavPath.LookAhead(1);
            if (ahead != null)
            {
                Structure? structureAhead = World.GetTile((Int2D)ahead);

                if (structureAhead == null ||
                    (!structureAhead.PhysSolid(this) &&
                     (structureAhead.Team != Team || structureAhead is not Minefield)))
                {
                    Vector2 to = World.GetTileCenter((Int2D)ahead);
                    to += new Vector2(World.RandomInt(17) - 8, World.RandomInt(23) - 11);
                    SetState(new Jump(this, to, 0.25, 0.5, 0.25));
                }
            }
        }

        base.Update();
    }
}