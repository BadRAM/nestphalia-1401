using Newtonsoft.Json.Linq;
using Raylib_cs;

namespace nestphalia;

public class GluePaperTemplate : StructureTemplate
{
    public GluePaperTemplate(JObject jObject) : base(jObject)
    {
        Class = Enum.Parse<StructureClass>(jObject.Value<string?>("Class") ?? "Defense");
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
    }

    public override void Update()
    {
        base.Update();
        
        foreach (Minion minion in World.GetMinionsInRegion(new Int2D(X,Y), 1))
        {
            if (minion.Team != Team && !minion.IsFlying)
            {
                minion.Status.Add(new StatusEffect("Glued", "Glued", 10, new Color(255,255,142)));
            }
        }
    }

    public override bool NavSolid(Team team)
    {
        return team == Team;
    }

    public override bool PhysSolid(Minion minion)
    {
        return false;
    }
}