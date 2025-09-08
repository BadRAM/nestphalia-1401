using System.Numerics;
using Newtonsoft.Json.Linq;
using Raylib_cs;

namespace nestphalia;

public class MinefieldTemplate : StructureTemplate
{
    public int MaxCharges;
    public string Bomb;
    public double Range;
    public double Cooldown;
    
    public MinefieldTemplate(JObject jObject) : base(jObject)
    {
        MaxCharges = jObject.Value<int?>("MaxCharges") ?? throw new ArgumentNullException();
        Bomb = jObject.Value<string?>("Bomb") ?? throw new ArgumentNullException();
        Range = jObject.Value<double?>("Range") ?? throw new ArgumentNullException();
        Cooldown = jObject.Value<double?>("Cooldown") ?? throw new ArgumentNullException();
        Class = jObject.Value<StructureClass?>("Class") ?? StructureClass.Tower;
    }

    public override Minefield Instantiate(Team team, int x, int y)
    {
        return new Minefield(this, team, x, y);
    }
}


public class Minefield : Structure
{
    private MinefieldTemplate _template;
    private ProjectileTemplate _bomb;
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
        _bomb = Assets.Get<ProjectileTemplate>(_template.Bomb);

        // _zOffset = position.Y - 24;
    }

    public override void Update()
    {
        base.Update();
        
        if (Time.Scaled - _timeLastTriggered < _template.Cooldown) return;
        foreach (Minion minion in World.GetMinionsInRegion(World.PosToTilePos(position), 2))
        {
            if (minion.Team != Team && !minion.IsFlying &&
                Raylib.CheckCollisionCircles(
                    position, (float)_template.Range, 
                    minion.Position.XY(),minion.Template.PhysicsRadius))
            {
                Trigger();
                break;
            }
        }
    }
    
    public override void Draw()
    {
        for (int i = 0; i < Math.Min(_chargesLeft, _drawPoints.Length); i++)
        {
            int x = (int)(_drawPoints[i].X + position.X - _bomb.Texture.Width/2);
            int y = (int)(_drawPoints[i].Y + position.Y - _bomb.Texture.Height/2);
            Raylib.DrawTexture(_bomb.Texture, x, y, Color.White);
        }
    }

    private void Trigger()
    {
        _armSound.PlayRandomPitch(SoundResource.WorldToPan(position.X));
        _chargesLeft--;
        _bomb.Instantiate((position + _drawPoints[_chargesLeft]).XYZ(), this, (position + _drawPoints[_chargesLeft]).XYZ());
        _timeLastTriggered = Time.Scaled;
        Health = Math.Max(_chargesLeft, Health);
        if (_chargesLeft <= 0) Destroy();
    }

    public override void Hurt(double damage)
    {
        base.Hurt(damage);
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