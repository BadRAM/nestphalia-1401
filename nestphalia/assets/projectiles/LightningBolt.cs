using System.Numerics;
using Raylib_cs;

namespace nestphalia;

public class LightningBoltTemplate : ProjectileTemplate
{
    public LightningBoltTemplate(string id, double damage) : base(id, Resources.MissingTexture, damage, 0)
    {
    }

    public override void Instantiate(object target, object source, Vector2 position)
    {
        LightningBolt p = new LightningBolt(this, position, target, source);
        World.Projectiles.Add(p);
        World.Sprites.Add(p);
    }
}

public class LightningBolt : Projectile
{
    private double _timeFired;
    private List<Vector2> _points;
    private double _duration = 0.4;
    
    public LightningBolt(LightningBoltTemplate template, Vector2 position, object target, object source) : base(template, position, target, source)
    {
        _timeFired = Time.Scaled;
        _points = new List<Vector2>();
        _points.Add(position);
        Vector2 targetPos = position;
        if (target is Minion m)
        {
            targetPos = m.Position;
            m.Hurt(Template.Damage*1.5 - Template.Damage*World.RandomDouble(), this);
        }
        for (int i = 0; i < 8; i++)
        {
            float r = (3 - Math.Abs(i - 4)) * 10;
            _points.Add(Vector2.Lerp(position, targetPos, (float)i / 8) + new Vector2((Random.Shared.NextSingle()-0.5f) * r, (Random.Shared.NextSingle()-0.5f) * r));
        }
        Z = position.Y + 80;
    }

    public override void Update()
    {
        if (Time.Scaled - _timeFired >= _duration)
        {
            Destroy();
        }
    }

    public override void Draw()
    {
        Color c = Color.White;
        if (Time.Scaled - _timeFired >= 0.1) c = Color.SkyBlue;
        if (Time.Scaled - _timeFired >= _duration/2) c = Color.Black;
        for (int i = 0; i < _points.Count-1; i++)
        {
            Raylib.DrawLineEx(_points[i], _points[i+1], 1, c);
        }
    }
}