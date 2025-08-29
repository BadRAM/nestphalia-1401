using Newtonsoft.Json.Linq;

namespace nestphalia;

public class Rubble : Structure
{
    private const string jsonTemplate = @"{""ID"": ""rubble"", ""Name"": ""rubble"", ""Texture"": ""rubble""}";
    public static readonly StructureTemplate RubbleTemplate = new StructureTemplate(JObject.Parse(jsonTemplate));
    public StructureTemplate DestroyedStructure;
    
    public Rubble(StructureTemplate template, Team team, int x, int y) : base(RubbleTemplate, team, x, y)
    {
        DestroyedStructure = template;
        //zOffset = -24;
    }
    
    public override void Hurt(double damage) { }
    
    public override bool NavSolid(Team team)
    {
        return false;
    }
    
    public override bool PhysSolid(Minion minion)
    {
        return false;
    }
}