using Newtonsoft.Json.Linq;

namespace nestphalia;

public class HazardSignTemplate : StructureTemplate
{
    public HazardSignTemplate(JObject jObject) : base(jObject)
    {
        Class = Enum.Parse<StructureClass>(jObject.Value<string?>("Class") ?? "Utility");
    }

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

    public override bool PhysSolid(Minion minion)
    {
        return false;
    }
}