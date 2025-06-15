using System.Net.NetworkInformation;
using Newtonsoft.Json.Linq;
using Raylib_cs;

namespace nestphalia;

public class SpawnerTemplate : StructureTemplate
{
    public MinionTemplate Minion;
    public int WaveSize;
    public double WaveGrowth;
    public double TimeBetweenSpawns;
    
    public SpawnerTemplate(JObject jObject) : base(jObject)
    {
        Minion = Assets.LoadJsonAsset<MinionTemplate>(jObject.Value<JObject?>("minion"));
        WaveSize = jObject.Value<int?>("waveSize") ?? throw new ArgumentNullException();
        WaveGrowth = jObject.Value<int?>("waveGrowth") ?? throw new ArgumentNullException();
        TimeBetweenSpawns = jObject.Value<double?>("timeBetweenSpawns") ?? throw new ArgumentNullException();
        Class = jObject.Value<StructureClass?>("class") ?? StructureClass.Nest;
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
        _navPath = new NavPath(_template.Name, team);
        _navPath.Start = new Int2D(x, y);
    }
    
    public override void Update()
    {
        base.Update();
        if (_spawnsRemaining > 0 && Time.Scaled - _lastSpawnTime > _template.TimeBetweenSpawns)
        {
            if (!_navPath.Found)
            {
                Console.WriteLine($"Creating a minion without a path, PathQueueLength: {Team.GetQueueLength()}");
            }
            _template.Minion.Instantiate(Team, position.XYZ(), _navPath.Clone(_template.Minion.Name));
            _spawnsRemaining--;
            _lastSpawnTime = Time.Scaled;
        }
    }

    public override void PreWaveEffect()
    {
        Retarget();
        _navPath.Reset(position.XYZ());
        _navPath.Destination = _targetTile;
        if (_template.Minion.PathFromNest())
        {
            Team.RequestPath(_navPath);
        }
        else
        {
            _navPath.Found = true;
        }
    }
    
    public override void WaveEffect()
    {
        _spawnsRemaining += _template.WaveSize + (int)(World.Wave * _template.WaveGrowth);
        _spawnsRemaining += _nextWaveSpawnBonus;
        _nextWaveSpawnBonus = 0;
    }

    public override void Destroy()
    {
        base.Destroy();
        if (Team.IsPlayerControlled && Program.CurrentScene is BattleScene battle)
        {
            battle.StartCameraShake(0.2, 4);
        }
    }

    public override bool NavSolid(Team team)
    {
        return team != Team;
    }
    
    public override bool PhysSolid()
    {
        return false;
    }
    
    public void AddSpawnBonus(int bonus)
    {
        _nextWaveSpawnBonus += bonus;
    }
    
    // Todo: Move the target selection into Team
    private void Retarget()
    {
        List<Sortable<Int2D>> targets = new List<Sortable<Int2D>>();
        
        for (int x = 0; x < World.BoardWidth; ++x)
        for (int y = 0; y < World.BoardHeight; ++y)
        {
            Structure? s = World.GetTile(x, y);
            if (s != null && s.Team != Team && s is not Rubble)
            {
                if (Team.GetHateFor(x, y) > 0)
                {
                    targets.Add(new Sortable<Int2D>(Team.GetHateFor(x, y), new Int2D(x, y)));
                }
            }
        }

        targets = targets.OrderByDescending(o => o.Order).ToList();
        
        if (targets.Count == 0)
        {
            Console.WriteLine("NO TARGETS!");
            return;
        }
        
        int i = World.WeightedRandom(targets.Count);
        // Console.WriteLine($"Picked target {i}");
        _targetTile = targets[i].Value;
    }

    public void SetTarget(Int2D target)
    {
        _targetTile = target;
        _navPath.Reset(position.XYZ());
        _navPath.Destination = _targetTile;
        if (_template.Minion.PathFromNest())
        {
            Team.DemandPath(_navPath);
        }
    }
}