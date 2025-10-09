using System.Numerics;
using Newtonsoft.Json.Linq;
using Raylib_cs;

namespace nestphalia;

public class MinefieldTemplate : StructureTemplate
{
    public int MaxCharges;
    public SubAsset<AttackTemplate> Bomb;
    public double Range;
    public double Cooldown;
    
    public MinefieldTemplate(JObject jObject) : base(jObject)
    {
        MaxCharges = jObject.Value<int?>("MaxCharges") ?? throw new ArgumentNullException();
        Bomb = new SubAsset<AttackTemplate>(jObject.GetValue("Bomb") ?? throw new ArgumentNullException());
        Range = jObject.Value<double?>("Range") ?? throw new ArgumentNullException();
        Cooldown = jObject.Value<double?>("Cooldown") ?? throw new ArgumentNullException();
        Class = Enum.Parse<StructureClass>(jObject.Value<string?>("Class") ?? "Defense");
    }

    public override Minefield Instantiate(Team team, int x, int y)
    {
        return new Minefield(this, team, x, y);
    }
    
    public override void Draw(Vector2 pos, Color tint)
    {
        Raylib.DrawTexture(Texture, (int)(pos.X - 10), (int)(pos.Y - 10), tint);
        Raylib.DrawTexture(Texture, (int)(pos.X + 10 - Texture.Width), (int)(pos.Y - 10), tint);
        Raylib.DrawTexture(Texture, (int)(pos.X - 10), (int)(pos.Y + 10 - Texture.Height), tint);
        Raylib.DrawTexture(Texture, (int)(pos.X + 10 - Texture.Width), (int)(pos.Y + 10 - Texture.Height), tint);
    }
}

public class Minefield : Structure
{
    private MinefieldTemplate _template;
    private int _chargesLeft;
    private double _timeLastTriggered;
    private Vector2[] _drawPoints = new []{new Vector2(6,6), new Vector2(-6,-6), new Vector2(-6,6), new Vector2(6,-6)};
    private SoundResource _armSound;
    
    public Minefield(MinefieldTemplate template, Team team, int x, int y) : base(template, team, x, y)
    {
        _template = template;
        _chargesLeft = template.MaxCharges;
        _armSound = Resources.GetSoundByName("primed");
        for (int i = 0; i < _drawPoints.Length; i++)
        {
            _drawPoints[i] += new Vector2(World.RandomInt(-2, 2), World.RandomInt(-2, 2));
        }
    }

    public override void Update()
    {
        base.Update();
        
        if (Time.Scaled - _timeLastTriggered < _template.Cooldown) return;
        if (World.GetMinionsInRadius(Position.XY(), (float)_template.Range, false, Team).Count != 0)
        {
            Trigger();
        }
    }
    
    public override void Draw()
    {
        for (int i = 0; i < Math.Min(_chargesLeft, _drawPoints.Length); i++)
        {
            int x = (int)(_drawPoints[i].X + Position.X - _template.Texture.Width/2);
            int y = (int)(_drawPoints[i].Y + Position.Y - _template.Texture.Height/2);
            Raylib.DrawTexture(_template.Texture, x, y, Color.White);
        }
    }

    private void Trigger()
    {
        _armSound.PlayRandomPitch(SoundResource.WorldToPan(Position.X));
        _chargesLeft--;
        _template.Bomb.Asset.Instantiate((Position.XY() + _drawPoints[_chargesLeft]).XYZ(), this, (Position.XY() + _drawPoints[_chargesLeft]).XYZ());
        _timeLastTriggered = Time.Scaled;
        Health = Math.Max(_chargesLeft, Health);
        if (_chargesLeft <= 0) Destroy();
    }

    public override void Hurt(double damage, Attack? damageSource = null, bool ignoreArmor = false, bool minDamage = true)
    {
        base.Hurt(damage, damageSource, ignoreArmor, minDamage);
        Trigger();
    }

    public override bool NavSolid(Team team)
    {
        return team == Team;
    }

    public override bool PhysSolid(Minion minion)
    {
        return false;
    }
}