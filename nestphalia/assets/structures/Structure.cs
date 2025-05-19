using System.Numerics;
using Raylib_cs;

namespace nestphalia;

public class StructureTemplate
{
    public string ID;
    public string Name;
    public string Description;
    public Texture2D Texture;
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
    
    public StructureTemplate(string id, string name, string description, Texture2D texture, double maxHealth, double price, int levelRequirement, double baseHate)
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
               $"HP: {MaxHealth}\n" +
               $"{Description}";
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

    private SoundResource _deathSound;

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

        _deathSound = Resources.GetSoundByName("shovel");
    }

    public virtual void Update() { }

    public virtual void PreWaveEffect() { }
    
    public virtual void WaveEffect() { }

    public virtual void Draw()
    {
        int t = 127 + (int)Math.Clamp(127 * (Health / Template.MaxHealth), 0, 128);
        if (Template.MaxHealth == 0) t = 255;
        int x = (int)(position.X - 12);
        int y = (int)(position.Y - (Template.Texture.Height - 12));
        Raylib.DrawTexture(Template.Texture, x, y, new Color(t,t,t,255));
    }


    public virtual bool NavSolid(Team team)
    {
        return true;
    }
    
    public virtual bool PhysSolid()
    {
        return true;
    }

    public virtual void Hurt(double damage)
    {
        if (Health <= 0) { return; } // Guard against dying multiple times
        Health -= damage;
        if (Health <= 0)
        {
            Destroy();
        }
    }

    public virtual void Destroy()
    {
        _deathSound.PlayRandomPitch(SoundResource.WorldToPan(position.X));
        World.DestroyTile(X, Y);
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