using System.Diagnostics;
using System.Numerics;
using Newtonsoft.Json.Linq;
using Raylib_cs;

namespace nestphalia;

public class TowerTemplate : StructureTemplate
{
    public enum TargetSelector
    {
        Nearest,
        Random,
        RandomFocus,
        Strongest,
        Weakest,
        Flyer
    }
    
    public double Range;
    public ProjectileTemplate Projectile;
    public double RateOfFire;
    public TargetSelector TargetMode;
    public bool CanHitFlying;
    public bool CanHitGround;
    public int ProjectileOriginZ;

    public TowerTemplate(JObject jObject) : base(jObject)
    {
        Name = jObject.Value<string?>("Name") ?? throw new ArgumentNullException();
        Range = jObject.Value<double?>("Range") ?? throw new ArgumentNullException();
        Projectile = Assets.LoadJsonAsset<ProjectileTemplate>(jObject.Value<JObject?>("Projectile"));
        ProjectileOriginZ = jObject.Value<int?>("ProjectileOriginZ") ?? 8;
        RateOfFire = jObject.Value<double?>("RateOfFire") ?? throw new ArgumentNullException();
        TargetMode = (TargetSelector)(jObject.Value<int?>("TargetMode") ?? throw new ArgumentNullException());
        CanHitGround = jObject.Value<bool?>("CanHitGround") ?? true;
        CanHitFlying = jObject.Value<bool?>("CanHitFlying") ?? true;
        Class = jObject.Value<StructureClass?>("Class") ?? StructureClass.Tower;
    }
    
    public override Tower Instantiate(Team team, int x, int y)
    {
        return new Tower(this, team, x, y);
    }
    
    public override string GetStats()
    {
        return $"{Name}\n" +
               $"${Price}\n" +
               $"HP: {MaxHealth}\n" +
               $"Damage: {Projectile.Damage} ({(Projectile.Damage * (RateOfFire/60)).ToString("N0")}/s)\n" +
               $"Range: {Range}\n" +
               $"{Description}\n";
    }
}

public class Tower : Structure
{
    private double _timeLastFired;
    private TowerTemplate _template;
    private Minion? _target;
    private SoundResource _shootSound;
    
    public Tower(TowerTemplate template, Team team, int x, int y) : base(template, team, x, y)
    {
        _template = template;
        _shootSound = Resources.GetSoundByName("shoot");
    }
    
    public override void Update()
    {
        base.Update();

        if (World.IsBattleOver()) return;
        
        if (Time.Scaled - _timeLastFired > 60/_template.RateOfFire)
        {
            switch (_template.TargetMode)
            {
                case TowerTemplate.TargetSelector.Nearest:
                    _target = FindTargetNearest();
                    break;
                case TowerTemplate.TargetSelector.Random:
                    _target = FindTargetRandom();
                    break;
                case TowerTemplate.TargetSelector.RandomFocus:
                    _target = FindTargetRandomFocus();
                    break;
                default:
                    throw new NotImplementedException();
            }
            
            if (_target != null && _target.Health <= 0)
            {
                _target = null;
            }

            if (_target != null)
            {
                _shootSound.PlayRandomPitch(SoundResource.WorldToPan(position.X), 0.15f);
                _template.Projectile.Instantiate(_target, this, position.XYZ(_template.ProjectileOriginZ));
                _timeLastFired = Time.Scaled;
            }
        }
    }

    public override void Draw()
    {
        base.Draw();

        // Draw range circle on hover
        // if (World.PosToTilePos(Raylib.GetScreenToWorld2D(Raylib.GetMousePosition(), World.Camera)) == new Int2D(X,Y))
        // {
        //     Raylib.DrawCircleLinesV(position, (int)_template.Range, new Color(200, 50, 50, 255));
        //     Raylib.DrawCircleV(position, (int)_template.Range, new Color(200, 50, 50, 64));
        // }
    }

    public override void Destroy()
    {
        Team e = World.GetOtherTeam(Team);

        Queue<Int2D> protectedArea = new Queue<Int2D>();
        for (int x = 0; x < World.BoardWidth; x++)
        for (int y = 0; y < World.BoardHeight; y++)
        {
            if (Vector2.Distance(World.GetTileCenter(x,y), position) > _template.Range) continue;
            if (e.GetFearOf(x, y) > 0) protectedArea.Enqueue(new Int2D(x,y));
        }   

        double reduction = -(e.GetHateFor(X, Y) / protectedArea.Count);
        // Console.WriteLine($"Tower at {X},{Y} destroyed, reducing {protectedArea.Count} surrounding tiles fear by {reduction}");
        while (protectedArea.Count > 0)
        {
            e.AddFearOf(reduction, protectedArea.Dequeue());
        }
        
        base.Destroy();
    }

    protected Minion? FindTargetNearest()
    {
        List<Minion> nearbyMinions = World.GetMinionsInRegion(new Int2D(X, Y), (int)(1 + _template.Range / 24));
        Minion? nearest = null;
        double minDist = double.MaxValue;
        for (int i = 0; i < nearbyMinions.Count; i++)
        {
            Minion m = nearbyMinions[i];
            if (m.Team == Team) continue;
            if ((m.IsFlying && !_template.CanHitFlying) || (!m.IsFlying && !_template.CanHitGround)) continue;
            double d = Vector2.Distance(nearbyMinions[i].Position.XY(), position);
            if (d < _template.Range && d < minDist)
            {
                minDist = d;
                nearest = m;
            }
        }

        return nearest;
    }

    protected Minion? FindTargetRandom()
    {
        List<Minion> nearbyMinions = World.GetMinionsInRegion(new Int2D(X, Y), (int)(1 + _template.Range / 24));
        Minion? random = null;
        List<Minion> ValidTargets = new List<Minion>();
        foreach (Minion m in nearbyMinions)
        {
            if ((m.IsFlying && !_template.CanHitFlying) || (!m.IsFlying && !_template.CanHitGround)) continue;
            if (m.Team != Team && Vector2.Distance(m.Position.XY(), position) < _template.Range)
            {
                ValidTargets.Add(m);
            }
        }

        if (ValidTargets.Count > 0)
        {
            random = ValidTargets[World.RandomInt(ValidTargets.Count)];
        }
        
        return random;
    }

    protected Minion? FindTargetRandomFocus()
    {
        Minion? random = _target;
        if (random != null && random.Health > 0 && Vector2.Distance(random.Position.XY(), position) < _template.Range) return random;
        return FindTargetRandom();
    }
}