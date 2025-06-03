using System.Reflection.Metadata.Ecma335;
using Raylib_cs;

namespace nestphalia;

public class Rubble : Structure
{
    public static readonly StructureTemplate RubbleTemplate = new StructureTemplate("rubble", "rubble", "", Resources.GetTextureByName("rubble"), 0, 0, 0, 0);
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
    
    public override bool PhysSolid()
    {
        return false;
    }
}