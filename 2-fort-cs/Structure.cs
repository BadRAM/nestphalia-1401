using System.Numerics;
using ZeroElectric.Vinculum;

namespace _2_fort_cs;

public class StructureTemplate
{
    public string Name;
    public Texture Texture;
    public double MaxHealth;
    public int LevelRequirement;
    public double Price;
    
    public StructureTemplate(string name, Texture texture, double maxHealth, double price, int levelRequirement = 0)
    {
        Name = name;
        Texture = texture;
        MaxHealth = maxHealth;
        Price = price;
        LevelRequirement = levelRequirement;
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
    
    public double Health;
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

    public virtual void PreWaveEffect() { }
    
    public virtual void WaveEffect() { }

    public virtual void Draw(int x, int y)
    {
        int t = 127 + (int)(128 * (Health / Template.MaxHealth));
        Raylib.DrawTexture(Template.Texture, x, y, new Color(t,t,t,255));
    }

    public virtual bool IsSolid()
    {
        return true;
    }

    public TeamName GetTeam()
    {
        return Team;
    }

    public virtual void Hurt(double damage)
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