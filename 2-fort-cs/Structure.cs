using Raylib_cs;

namespace _2_fort_cs;

public class StructureTemplate : TileTemplate
{
    public float MaxHealth;

    public StructureTemplate(Texture2D texture, float maxHealth) : base(texture)
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
    private StructureTemplate _template;

    public Structure(StructureTemplate template, int x, int y) : base(template, x, y)
    {
        _template = template;
        Health = template.MaxHealth;
    }

    public override bool IsSolid()
    {
        return true;
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