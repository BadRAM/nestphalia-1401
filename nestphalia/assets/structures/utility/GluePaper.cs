using System.Numerics;
using System.Security.Cryptography;
using Raylib_cs;

namespace nestphalia;

public class GluePaperTemplate : StructureTemplate
{
    public GluePaperTemplate(string id, string name, string description, Texture2D texture, double maxHealth, double price, int levelRequirement, double baseHate) : base(id, name, description, texture, maxHealth, price, levelRequirement, baseHate)
    {
        //Class = StructureClass.Tower;
    }

    public override GluePaper Instantiate(Team team, int x, int y)
    {
        return new GluePaper(this, team, x, y);
    }
}


public class GluePaper : Structure
{
    private GluePaperTemplate _template;
    
    public GluePaper(GluePaperTemplate template, Team team, int x, int y) : base(template, team, x, y)
    {
        _template = template;
        Z = position.Y - 24;
    }

    public override void Update()
    {
        base.Update();
        
        foreach (Minion minion in World.GetMinionsInRegion(new Int2D(X,Y), 2))
        {
            if (minion.Team != Team 
                && !minion.IsFlying
                && !minion.Glued
                && World.PosToTilePos(minion.Position) == new Int2D(X,Y))
            {
                minion.Glued = true;
                Health -= minion.Health/2.0;
                if (Health <= 0)
                {
                    Destroy();
                }
            }
        }
    }

    public override bool NavSolid(Team team)
    {
        return team == Team;
    }

    public override bool PhysSolid()
    {
        return false;
    }
}