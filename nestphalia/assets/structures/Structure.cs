using System.Numerics;
using Newtonsoft.Json.Linq;
using Raylib_cs;

namespace nestphalia;

public class StructureTemplate : JsonAsset
{
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
        Basic,
        Utility,
        Defense,
        Nest,
        Secret
    }
    
    public StructureTemplate(JObject jObject) : base(jObject)
    {
        Name = jObject.Value<string?>("Name") ?? throw new ArgumentNullException();
        Description = jObject.Value<string?>("Description") ?? "";
        Texture = Resources.GetTextureByName(jObject.Value<string?>("Texture") ?? "");
        MaxHealth = jObject.Value<double?>("MaxHealth") ?? 0;
        Price = jObject.Value<double?>("Price") ?? 0;
        LevelRequirement = jObject.Value<int?>("LevelRequirement") ?? 0;
        BaseHate = jObject.Value<double?>("BaseHate") ?? 0;
        Class = Enum.Parse<StructureClass>(jObject.Value<string?>("Class") ?? "Basic");
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

    public virtual void Draw(Vector2 pos, Color tint)
    {
        int x = (int)(pos.X - 12);
        int y = (int)(pos.Y - (Texture.Height - 12));
        Raylib.DrawTexture(Texture, x, y, tint);
    }
}

public class Structure : ISprite, IMortal
{
    public StructureTemplate Template;
    protected int X;
    protected int Y;
    public readonly Vector3 Position;
    public double Health { get; set; }
    public Team Team { get; }
    public Int2D Origin { get; }


    private SoundResource _deathSound;

    //protected double _zOffset;

    public Structure(StructureTemplate template, Team team, int x, int y)
    {
        Template = template;
        X = x;
        Y = y;
        Position = new Vector3(x*24+12, y*24+20, 0);
        Health = template.MaxHealth;
        Team = team;
        Origin = new Int2D(X, Y);
        _deathSound = Resources.GetSoundByName("shovel");
    }

    public virtual void Update() { }

    public virtual void Draw()
    {
        int t = 127 + (int)Math.Clamp(127 * (Health / Template.MaxHealth), 0, 128);
        if (Template.MaxHealth == 0) t = 255;
        int x = (int)(Position.X - 12);
        int y = (int)(Position.Y - (Template.Texture.Height - 12));
        Raylib.DrawTexture(Template.Texture, x, y, new Color(t,t,t,255));
    }

    // Structures can use _zOffset to push themselves behind things that walk over them.
    // Alternatively, we can just flatten tiles that aren't solid, so bugs can step on them.
    // even more alternatively, we can just use the top edge of the grid square.
    public double GetDrawOrder()
    {
        // return position.Y + _zOffset;
        // return position.Y - (PhysSolid() ? 24 : 0);
        return Position.Y - 12;
    }

    public virtual bool NavSolid(Team team)
    {
        return true;
    }

    public virtual bool PhysSolid(Minion minion)
    {
        return true;
    }

    public virtual void Hurt(double damage, Attack? damageSource = null, bool ignoreArmor = false, bool minDamage = true)
    {
        if (Health <= 0) { return; } // Guard against dying multiple times
        Health -= damage;
        if (Health <= 0)
        {
            Destroy();
        }
    }

    public Vector3 GetPos()
    {
        return Position;
    }

    public virtual void Destroy()
    {
        _deathSound.PlayRandomPitch(SoundResource.WorldToPan(Position.X));
        World.DestroyTile(X, Y);
    }

    public Int2D GetTilePos()
    {
        return new Int2D(X, Y);
    }
}