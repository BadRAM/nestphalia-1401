using System.Numerics;
using Newtonsoft.Json.Linq;

namespace nestphalia;

public abstract class AttackTemplate : EffectTemplate
{
    public double Damage;
    
    protected AttackTemplate(JObject jObject) : base(jObject)
    {
        Damage = jObject.Value<double?>("Damage") ?? 0;
    }

    public abstract Attack Instantiate(IMortal target, IMortal? source, Vector3 position);
    public abstract Attack Instantiate(Vector3 target, IMortal? source, Vector3 position);
}

public abstract class Attack : Effect
{
    private AttackTemplate _template;
    public Vector3 TargetPos;
    public IMortal? TargetEntity;
    public IMortal? Source;

    public Attack(AttackTemplate template, Vector3 position, IMortal targetEntity, IMortal? source) : base(template, position)
    {
        _template = template;
        TargetEntity = targetEntity;
        TargetPos = targetEntity.GetPos();
        Source = source;
    }
    
    public Attack(AttackTemplate template, Vector3 position, Vector3 targetPos, IMortal? source) : base(template, position)
    {
        _template = template;
        TargetPos = targetPos;
        Source = source;
    }

    public double GetDamageModified()
    {
        return _template.Damage * 1.5 - _template.Damage * World.RandomDouble();
    }
}