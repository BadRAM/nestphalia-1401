using ZeroElectric.Vinculum;

namespace _2_fort_cs;

public class StructureTemplate : TileTemplate
{
    public float MaxHealth;

    public StructureTemplate(string name, Texture texture, float maxHealth) : base(name, texture)
    {
        MaxHealth = maxHealth;
    }
    
    public override Structure Instantiate(int x, int y)
    {
        return new Structure(this, x, y);
    }
}

public class Structure : Tile
{
    public float Health;
    public TeamName Team;
    private StructureTemplate _template;

    public Structure(StructureTemplate template, int x, int y) : base(template, x, y)
    {
        _template = template;
        Health = template.MaxHealth;
        Team = TeamName.Neutral;
        if (x <= 22) Team = TeamName.Player;
        if (x >= 26) Team = TeamName.Enemy;
    }

    public override bool IsSolid()
    {
        return true;
    }

    public override TeamName GetTeam()
    {
        return Team;
    }

    public override void Hurt(float damage)
    {
        Health -= damage;
        if (Health <= 0)
        {
            World.SetTile(Assets.Tiles[0], X, Y);
        }
    }
}