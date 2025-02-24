using System.Numerics;
using ZeroElectric.Vinculum;

namespace _2_fort_cs;

public class StructureTemplate
{
    public string ID;
    public string Name;
    public string Description;
    public Texture Texture;
    public double MaxHealth;
    public double Price;
    public int LevelRequirement;
    public double BaseHate;
    public StructureClass Class;
    
    public enum StructureClass
    {
        Utility,
        Tower,
        Nest
    }
    
    public StructureTemplate(string id, string name, string description, Texture texture, double maxHealth, double price, int levelRequirement, double baseHate)
    {
        Name = name;
        Texture = texture;
        MaxHealth = maxHealth;
        Price = price;
        LevelRequirement = levelRequirement;
        BaseHate = baseHate;
        ID = id;
        Description = description;
        Class = StructureClass.Utility;
    }
    
    public virtual Structure Instantiate(Team team, int x, int y)
    {
        return new Structure(this, team, x, y);
    }
    
    public virtual string GetStats()
    {
        return $"{Name}\n" +
               $"${Price}\n" +
               $"HP: {MaxHealth}";
    }
}

public class Structure : ISprite
{
    public StructureTemplate Template;
    private protected int X;
    private protected int Y;
    private protected Vector2 position;
    
    public double Health;
    public Team Team;

    public double Z { get; set; }

    public Structure(StructureTemplate template, Team team, int x, int y)
    {
        Template = template;
        X = x;
        Y = y;
        position = new Vector2(x*24+12, y*24+20);
        Z = position.Y;
        
        Health = template.MaxHealth;
        Team = team;
    }

    public virtual void Update() { }

    public virtual void PreWaveEffect() { }
    
    public virtual void WaveEffect() { }

    public virtual void Draw()
    {
        int t = 127 + (int)(128 * (Health / Template.MaxHealth));
        int x = (int)(position.X - 12);
        int y = (int)(position.Y - (Template.Texture.height - 12));
        Raylib.DrawTexture(Template.Texture, x, y, new Color(t,t,t,255));
    }


    public virtual bool NavSolid(Team team)
    {
        return true;
    }
    
    public virtual bool PhysSolid(Team team)
    {
        return true;
    }

    public virtual void Hurt(double damage)
    {
        Health -= damage;
        if (Health <= 0)
        {
            Destroy();
        }
    }

    public virtual void Destroy()
    {
        World.DestroyTile(X, Y);
        // World.SetTile(Rubble.RubbleTemplate, Team, X, Y);
    }
    
    public Vector2 GetCenter()
    {
        return position;
    }

    public Int2D GetTilePos()
    {
        return new Int2D(X, Y);
    }
}