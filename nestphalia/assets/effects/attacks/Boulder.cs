using System.Numerics;
using Newtonsoft.Json.Linq;
using Raylib_cs;

namespace nestphalia;

public class BoulderTemplate : AttackTemplate
{
    public float BlastRadius;
    public bool DamageBuildings;
    public Texture2D Texture;
    
    public BoulderTemplate(JObject jObject) : base(jObject)
    {
        BlastRadius = jObject.Value<float?>("BlastRadius") ?? 0;
        DamageBuildings = jObject.Value<bool?>("DamageBuildings") ?? false;
        Texture = Resources.GetTextureByName(jObject.Value<string?>("Texture") ?? "");
    }

    public override Attack Instantiate(IMortal target, IMortal? source, Vector3 position)
    {
        return Register(new Boulder(this, position, target, source));
    }

    public override Attack Instantiate(Vector3 target, IMortal? source, Vector3 position)
    {
        return Register(new Boulder(this, position, target, source));
    }
}

public class Boulder : Attack
{
    private BoulderTemplate _template;
    
    public Boulder(BoulderTemplate template, Vector3 position, IMortal targetEntity, IMortal? source) : base(template, position, targetEntity, source)
    { 
        _template = template;
        Init();
    }
    
    public Boulder(BoulderTemplate template, Vector3 position, Vector3 targetPos, IMortal? source) : base(template, position, targetPos, source)
    {
        _template = template;
        Init();
    }

    private void Init()
    {
        Explode();
    }

    private void Explode()
    {
        Structure? targetStructure = World.GetTileAtPos(TargetPos);
        if (targetStructure != null && targetStructure is not Rubble)
        {
            targetStructure.Hurt(_template.Damage);
            return;
        }
        
        World.SetTile(Assets.Get<StructureTemplate>("wall_stone"), World.NeutralTeam,  World.PosToTilePos(TargetPos));
        
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
        World.EffectsToRemove.Add(this);
    }
}