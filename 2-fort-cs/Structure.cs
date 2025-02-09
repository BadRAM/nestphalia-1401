using System.Numerics;
using ZeroElectric.Vinculum;

namespace _2_fort_cs;

public class StructureTemplate
{
    public string Name;
    public Texture Texture;
    public float MaxHealth;
    
    public StructureTemplate(string name, Texture texture, float maxHealth)
    {
        Name = name;
        Texture = texture;
        MaxHealth = maxHealth;
    }
    
    public virtual Structure Instantiate(int x, int y)
    {
        return new Structure(this, x, y);
    }
}

public class Structure
{
    public StructureTemplate Template;
    private protected int X;
    private protected int Y;
    private protected Vector2 position;
    
    public float Health;
    public TeamName Team;

    public Structure(StructureTemplate template, int x, int y)
    {
        Template = template;
        X = x;
        Y = y;
        position = new Vector2(x*24+12, y*24+20);
        
        Health = template.MaxHealth;
        Team = TeamName.Neutral;
        if (x <= 22) Team = TeamName.Player;
        if (x >= 26) Team = TeamName.Enemy;
    }

    public virtual void Update() { }

    public virtual void WaveEffect() { }

    public virtual void Draw(int x, int y)
    {
        Raylib.DrawTexture(Template.Texture, x, y, Raylib.WHITE);
    }

    public virtual bool IsSolid()
    {
        return true;
    }

    public TeamName GetTeam()
    {
        return Team;
    }

    public virtual void Hurt(float damage)
    {
        Health -= damage;
        if (Health <= 0)
        {
            World.SetTile(null, X, Y);
        }
    }
    
    public Vector2 GetCenter()
    {
        return position;
    }
}