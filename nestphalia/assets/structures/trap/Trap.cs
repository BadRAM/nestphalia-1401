using Newtonsoft.Json.Linq;

namespace nestphalia;

public class TrapTemplate : StructureTemplate
{
    public TrapTemplate(JObject jObject) : base(jObject)
    {
    }
    
    public override Trap Instantiate(Team team, int x, int y)
    {
        return new Trap(this, team, x, y);
    }
}

public class Trap : Structure
{
    public Trap(StructureTemplate template, Team team, int x, int y) : base(template, team, x, y)
    {
    }
}