using System.Numerics;
using Newtonsoft.Json.Linq;
using Raylib_cs;

namespace nestphalia;

public class LightningBoltTemplate : AttackTemplate
{
    public LightningBoltTemplate(JObject jObject) : base(jObject)
    {
    }

    public override Attack Instantiate(IMortal target, IMortal? source, Vector3 position)
    {
        return Register(new LightningBolt(this, position, target, source));
    }

    public override Attack Instantiate(Vector3 target, IMortal? source, Vector3 position)
    {
        return Register(new LightningBolt(this, position, target, source));
    }
}

public class LightningBolt : Attack
{
    private double _timeFired;
    private List<Vector3> _points = new List<Vector3>();
    private double _duration = 0.4;
    
    public LightningBolt(LightningBoltTemplate template, Vector3 position, IMortal targetEntity, IMortal? source) : base(template, position, targetEntity, source)
    {
        Init();
        targetEntity.Hurt(GetDamageModified(), this);
    }
    
    public LightningBolt(LightningBoltTemplate template, Vector3 position, Vector3 targetPos, IMortal? source) : base(template, position, targetPos, source)
    {
        Init();
    }

    private void Init()
    {
        _timeFired = Time.Scaled;
        _points.Add(Position);
        for (int i = 0; i < 8; i++)
        {
            float r = (3 - Math.Abs(i - 4)) * 10;
            Vector3 point = Vector3.Lerp(Position, TargetPos, (float)i / 8);
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