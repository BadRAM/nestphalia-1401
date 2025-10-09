using System.Numerics;
using Newtonsoft.Json.Linq;
using Raylib_cs;

namespace nestphalia;

public class ExplosionTemplate : AttackTemplate
{
    public float BlastRadius;
    public bool DamageBuildings;
    public Texture2D Texture;
    public SoundResource? SoundEffect;
    
    public ExplosionTemplate(JObject jObject) : base(jObject)
    {
        BlastRadius = jObject.Value<float?>("BlastRadius") ?? 0;
        DamageBuildings = jObject.Value<bool?>("DamageBuildings") ?? false;
        Texture = Resources.GetTextureByName(jObject.Value<string?>("Texture") ?? "");
        string soundID = jObject.Value<string?>("SoundEffect") ?? "";
        SoundEffect = soundID != "" ? Resources.GetSoundByName(soundID) : null;
    }

    public override Attack Instantiate(IMortal target, IMortal? source, Vector3 position)
    {
        return Register(new Explosion(this, position, target, source));
    }

    public override Attack Instantiate(Vector3 target, IMortal? source, Vector3 position)
    {
        return Register(new Explosion(this, position, target, source));
    }
}

public class Explosion : Attack
{
    private ExplosionTemplate _template;
    private double _startTime = 0;
    
    public Explosion(ExplosionTemplate template, Vector3 position, IMortal targetEntity, IMortal? source) : base(template, position, targetEntity, source)
    { 
        _template = template;
        Init();
    }
    
    public Explosion(ExplosionTemplate template, Vector3 position, Vector3 targetPos, IMortal? source) : base(template, position, targetPos, source)
    {

        _template = template;
        Init();
    }

    private void Init()
    {
        _startTime = Time.Scaled;
        Explode();
    }
    
    public override void Update()
    {
        if (Time.Scaled - _startTime > 1)
        {
            World.EffectsToRemove.Add(this);
        }
        return;
    }

    private void Explode()
    {
        _template.SoundEffect?.PlayRandomPitch(SoundResource.WorldToPan(Position.X));
        if (_template.BlastRadius == 0)
        {
            if (_template.DamageBuildings && TargetEntity is Structure)
            {
                TargetEntity.Hurt(GetDamageModified());
            }
            return;
        }
        
        foreach (Minion minion in World.GetMinionsInRadius3d(Position, _template.BlastRadius, Source?.Team))
        {
            float dist = Vector3.Distance(Position, minion.Position);
            double falloff = Math.Clamp(2 * (_template.BlastRadius - dist) / _template.BlastRadius, 0, 1);
            minion.Hurt(_template.Damage * falloff, this);
            // Knockback if max damage dealt
            if (falloff >= 0.99)
            {
                double blastFactor = 3 * _template.Damage * (_template.BlastRadius - dist / _template.BlastRadius) / (minion.Template.MaxHealth + 40) ;
                if (blastFactor > 12)
                {
                    Vector2 target = minion.Position.XY() + (minion.Position.XY() - Position.XY()).Normalized() * (float)blastFactor;
                    minion.SetState(new Minion.Jump(minion, target, 0, blastFactor / 80, 0, blastFactor/2));
                    // GameConsole.WriteLine($"Launched a {minion.Template.ID} with force {blastFactor:N3}");
                }
            }
        }

        if (_template.DamageBuildings)
        {
            Int2D center = World.PosToTilePos(Position);
            int radius = (int)(_template.BlastRadius / 24) + 2;
            for (int x = Math.Max(center.X - radius, 0); x <= Math.Min(center.X + radius, World.BoardHeight-1); x++)
            for (int y = Math.Max(center.Y - radius, 0); y <= Math.Min(center.Y + radius, World.BoardHeight-1); y++)
            {
                double dist = Vector2.Distance(World.GetTileCenter(x, y), Position.XY());
                if (dist > _template.BlastRadius) continue;
                Structure? struc = World.GetTile(x, y);
                if (struc == null || struc.Team == Source?.Team) continue;
                
                double falloff = Math.Clamp(2 * (_template.BlastRadius - dist) / _template.BlastRadius, 0, 1);
                struc.Hurt(_template.Damage * falloff, this);
            }
        }
    }

    public override void Draw()
    {
        int frame = (int)Math.Floor((Time.Scaled - _startTime) / 0.1);
        Rectangle src = new Rectangle(frame * 32, 0, 32, 32);
        if (src.X >= _template.Texture.Width)
        {
            return;
        }
        Vector2 pos = Position.XYZ2D() - new Vector2(16, 16);
        Raylib.DrawTextureRec(_template.Texture, src, pos, Color.White);
    }
}