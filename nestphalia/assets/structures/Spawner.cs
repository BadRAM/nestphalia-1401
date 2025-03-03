using System.Net.NetworkInformation;
using ZeroElectric.Vinculum;

namespace nestphalia;

public class SpawnerTemplate : StructureTemplate
{
    public MinionTemplate Minion;
    public int WaveSize;
    public double WaveGrowth;
    public double TimeBetweenSpawns;
    
    public SpawnerTemplate(string id, string name, string description, Texture texture, double maxHealth, double price, int levelRequirement, double baseHate, MinionTemplate minion, int waveSize, double waveGrowth, double timeBetweenSpawns) : base(id, name, description, texture, maxHealth, price, levelRequirement, baseHate)
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
    
    public override string GetStats()
    {
        return $"{Name}\n" +
               $"${Price}\n" +
               $"HP: {MaxHealth}\n" +
               $"Wave Size: {WaveSize} + {WaveGrowth} per wave\n" +
               $"Spawn Delay: {TimeBetweenSpawns}\n" +
               $"{Description}\n" +
               $"{Minion.GetStats()}";
    }
}

public class Spawner : Structure
{
    private SpawnerTemplate _template;
    private double _lastSpawnTime;
    private int _spawnsRemaining;
    private int _nextWaveSpawnBonus;
    private Int2D _targetTile;
    private NavPath _navPath;
    
    public Spawner(SpawnerTemplate template, Team team, int x, int y) : base(template, team, x, y)
    {
        _template = template;
        _navPath = new NavPath(team);
        _navPath.Start = new Int2D(x, y);
    }
    
    public override void Update()
    {
        base.Update();
        if (_spawnsRemaining > 0 && Time.Scaled - _lastSpawnTime > _template.TimeBetweenSpawns)
        {
            if (!_navPath.Found)
            {
                Console.WriteLine($"Creating a minion without a path, PathQueueLength: {PathFinder.GetQueueLength()}");
            }
            _template.Minion.Instantiate(position, Team, _navPath.Clone());
            _spawnsRemaining--;
            _lastSpawnTime = Time.Scaled;
        }
    }

    public override void PreWaveEffect()
    {
        Retarget();
        _navPath.Start = new Int2D(X, Y);
        _navPath.Reset();
        _navPath.Destination = _targetTile;
        if (!(_template.Minion is FlyingMinionTemplate))
        {
            PathFinder.RequestPath(_navPath);
        }
    }
    
    public override void WaveEffect()
    {
        _spawnsRemaining += _template.WaveSize + (int)(World.Wave * _template.WaveGrowth);
        _spawnsRemaining += _nextWaveSpawnBonus;
        _nextWaveSpawnBonus = 0;
    }

    public void AddSpawnBonus(int bonus)
    {
        _nextWaveSpawnBonus += bonus;
    }
    
    public override bool NavSolid(Team team)
    {
        return false;
    }
    
    public override bool PhysSolid(Team team)
    {
        return false;
    }
    
    private void Retarget()
    {
        List<Sortable<Int2D>> targets = new List<Sortable<Int2D>>();
        
        for (int x = 0; x < World.BoardWidth; ++x)
        {
            for (int y = 0; y < World.BoardHeight; ++y)
            {
                Structure? s = World.GetTile(x, y);
                if (s != null && s.Team != Team && s is not Rubble)
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
            Console.WriteLine("NO TARGETS!");
            return;
        }
        
        int i = Utils.WeightedRandom(targets.Count);
        Console.WriteLine($"Picked target {i}");
        _targetTile = targets[i].Value;
    }

    public void SetTarget(Int2D target)
    {
        _targetTile = target;
        _navPath.Start = new Int2D(X, Y);
        _navPath.Reset();
        _navPath.Destination = _targetTile;
        if (!(_template.Minion is FlyingMinionTemplate))
        {
            PathFinder.FindPath(_navPath);
        }
    }
}