using System.Numerics;
using System.Threading.Tasks.Dataflow;
using Newtonsoft.Json.Linq;
using Raylib_cs;

namespace nestphalia;

public class LightningBoltTemplate : ProjectileTemplate
{
    public LightningBoltTemplate(JObject jObject) : base(jObject)
    {
    }

    public override void Instantiate(object target, object source, Vector3 position)
    {
        LightningBolt p = new LightningBolt(this, position, target, source);
        World.Projectiles.Add(p);
        World.Sprites.Add(p);
    }
}

public class LightningBolt : Projectile
{
    private double _timeFired;
    private List<Vector3> _points;
    private double _duration = 0.4;
    
    public LightningBolt(LightningBoltTemplate template, Vector3 position, object target, object source) : base(template, position, target, source)
    {
        _timeFired = Time.Scaled;
        _points = new List<Vector3>();
        _points.Add(position);
        Vector3 targetPos = position;
        if (target is Minion m)
        {
            targetPos = m.Position;
            m.Hurt(Template.Damage*1.5 - Template.Damage*World.RandomDouble(), this);
        }
        for (int i = 0; i < 8; i++)
        {
            float r = (3 - Math.Abs(i - 4)) * 10;
            Vector3 point = Vector3.Lerp(position, targetPos, (float)i / 8);
            point += new Vector3((Random.Shared.NextSingle() - 0.5f) * r, 
                                 (Random.Shared.NextSingle() - 0.5f) * r, 0f);
            _points.Add(point);
        }
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
            Vector2 start = _points[i].XY();
            start.Y -= _points[i].Z;
            Vector2 end = _points[i+1].XY();
            end.Y -= _points[i+1].Z;
            Raylib.DrawLineEx(start, end, 1, c);
        }
    }

    // Make sure lightning draws on top of everything around it.
    public override double GetDrawOrder()
    {
        return Position.Y + 80;
    }
}