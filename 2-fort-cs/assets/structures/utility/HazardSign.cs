using ZeroElectric.Vinculum;

namespace _2_fort_cs;

public class HazardSignTemplate : StructureTemplate
{
    public HazardSignTemplate(string id, string name, string description, Texture texture, double maxHealth, double price, int levelRequirement, double baseHate) : base(id, name, description, texture, maxHealth, price, levelRequirement, baseHate) {}

    public override Structure Instantiate(Team team, int x, int y)
    {
        return new HazardSign(this, team, x, y);
    }
}

public class HazardSign : Structure
{
    public HazardSign(StructureTemplate template, Team team, int x, int y) : base(template, team, x, y)
    {
    }

    public override bool NavSolid(Team team)
    {
        return Team == team;
    }

    public override bool PhysSolid(Team team)
    {
        return false;
    }
}