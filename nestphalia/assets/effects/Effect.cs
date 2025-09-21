using System.Numerics;
using Newtonsoft.Json.Linq;
using Raylib_cs;

namespace nestphalia;

public abstract class EffectTemplate : JsonAsset
{
    public EffectTemplate(JObject jObject) : base(jObject)
    {
    }
    
    // public virtual void Instantiate(object target, object source, Vector3 position)
    // {
    //     Effect p = new Effect(this, position);
    //     World.Effects.Add(p);
    //     World.Sprites.Add(p);
    // }
}

public abstract class Effect : ISprite
{
    public EffectTemplate Template;
    public Vector3 Position;

    public Effect(EffectTemplate template, Vector3 position)
    {
        Template = template;
        Position = position;
    }
    
    public virtual void Update(){}
    
    public virtual void Destroy()
    {
        World.EffectsToRemove.Add(this);
    }

    public virtual void Draw()
    {
    }

    public virtual double GetDrawOrder()
    {
        return Position.Y;
    }
}