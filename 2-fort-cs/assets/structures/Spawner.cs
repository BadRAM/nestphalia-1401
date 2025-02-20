using ZeroElectric.Vinculum;

namespace _2_fort_cs;

public class SpawnerTemplate : StructureTemplate
{
    public MinionTemplate Minion;
    public int WaveSize;
    public double WaveGrowth;
    public double TimeBetweenSpawns;
    
    public SpawnerTemplate(string name, Texture texture, double maxHealth, double price, int levelRequirement, double baseHate, MinionTemplate minion, int waveSize, double waveGrowth, double timeBetweenSpawns) : base(name, texture, maxHealth, price, levelRequirement, baseHate)
    {
        Minion = minion;
        WaveSize = waveSize;
        WaveGrowth = waveGrowth;
        TimeBetweenSpawns = timeBetweenSpawns;
        Class = StructureClass.Nest;
    }
    
    public override Spawner Instantiate(Team team, int x, int y)
    {
        return new Spawner(this, team, x, y);
    }
    
    public override string GetDescription()
    {
        return $"{Name}\n" +
               $"${Price}\n" +
               $"HP: {MaxHealth}\n" +
               $"Wave Size: {WaveSize} + {WaveGrowth} per wave\n" +
               $"Spawn Delay: {TimeBetweenSpawns}\n\n" +
               $"{Minion.Name}\n" +
               $"HP: {Minion.MaxHealth}\n" +
               (Minion.Armor == 0 ? "" : $"Armor: {Minion.Armor}\n") +
               (Minion.IsFlying ? "Flyer\n" : "") +
               $"Speed: {Minion.Speed}\n" +
               $"Damage: {Minion.Projectile.Damage} ({Minion.Projectile.Damage / (Minion.RateOfFire / 60)}/s)\n" +
               $"Size: {Minion.PhysicsRadius*2}";
    }
}

public class Spawner : Structure
{
    private SpawnerTemplate _template;
    private double _lastSpawnTime;
    private int _spawnsRemaining;
    private Int2D _targetTile;
    private NavPath _navPath;
    
    public Spawner(SpawnerTemplate template, Team team, int x, int y) : base(template, team, x, y)
    {
        _template = template;
        _navPath = new NavPath(new Int2D(x, y), new Int2D(x, y), team);
    }
    
    public override void Update()
    {
        base.Update();
        if (_spawnsRemaining > 0 && Time.Scaled - _lastSpawnTime > _template.TimeBetweenSpawns)
        {
            _template.Minion.Instantiate(position, Team, _navPath.Clone());
            _spawnsRemaining--;
            _lastSpawnTime = Time.Scaled;
        }
    }

    public override void PreWaveEffect()
    {
        Retarget();
        _navPath.Reset();
        _navPath.Destination = _targetTile;
        if (!_template.Minion.IsFlying)
        {
            PathFinder.RequestPath(_navPath);
        }
    }
    
    public override void WaveEffect()
    {
        _spawnsRemaining += _template.WaveSize + (int)(World.Wave * _template.WaveGrowth);
    }

    public override bool NavSolid(Team team)
    {
        return team != Team;
    }

    public override bool PhysSolid(Team team)
    {
        return team != Team;
    }

    private void Retarget()
    {
        List<Sortable<Int2D>> targets = new List<Sortable<Int2D>>();
        
        for (int x = 0; x < World.BoardWidth; ++x)
        {
            for (int y = 0; y < World.BoardHeight; ++y)
            {
                if (World.GetTile(x,y) != null && World.GetTile(x,y).Team != Team)
                {
                    if (Team.GetHateFor(x,y) > 0)
                    {
                        targets.Add(new Sortable<Int2D>(Team.GetHateFor(x,y), new Int2D(x,y)));
                    }
                }
            }
        }

        targets = targets.OrderByDescending(o => o.Order).ToList();

        if (targets.Count == 0)
        {
            return;
        }

        int i = Utils.WeightedRandom(targets.Count);
        _targetTile = targets[i].Value;
        //_targetTile = targets[0].Value;
    }
}