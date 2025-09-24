using Newtonsoft.Json.Linq;
using Raylib_cs;

namespace nestphalia;

public class SpawnerTemplate : StructureTemplate
{
    public string Minion;
    public int WaveSize;
    public double WaveGrowth;
    public double TimeBetweenSpawns;
    public bool StandardBearer;
    
    public SpawnerTemplate(JObject jObject) : base(jObject)
    {
        Minion = jObject.Value<string?>("Minion") ?? throw new ArgumentNullException();
        WaveSize = jObject.Value<int?>("WaveSize") ?? throw new ArgumentNullException();
        WaveGrowth = jObject.Value<int?>("WaveGrowth") ?? throw new ArgumentNullException();
        TimeBetweenSpawns = jObject.Value<double?>("TimeBetweenSpawns") ?? throw new ArgumentNullException();
        StandardBearer = jObject.Value<bool?>("StandardBearer") ?? false;
        Class = Enum.Parse<StructureClass>(jObject.Value<string?>("Class") ?? "Nest");
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
               $"{Assets.Get<MinionTemplate>(Minion).GetStats()}";
    }
}

public class Spawner : Structure
{
    private SpawnerTemplate _template;
    private MinionTemplate _minion;
    private double _lastSpawnTime;
    private int _spawnsRemaining;
    private int _nextWaveSpawnBonus;
    private Int2D _targetTile;
    private NavPath _navPath;
    private bool _spawnStandardBearer;
    
    public Spawner(SpawnerTemplate template, Team team, int x, int y) : base(template, team, x, y)
    {
        _template = template;
        _navPath = new NavPath(template.Name, team);
        _navPath.Start = new Int2D(x, y);
        _minion = Assets.Get<MinionTemplate>(template.Minion);
    }
    
    public override void Update()
    {
        base.Update();
        if (_spawnsRemaining > 0 && Time.Scaled - _lastSpawnTime > _template.TimeBetweenSpawns)
        {
            if (!_navPath.Found)
            {
                GameConsole.WriteLine($"Creating a minion without a path, PathQueueLength: {Team.GetQueueLength()}");
            }
            Minion m = _minion.Instantiate(Team, position.XYZ(), _navPath.Clone(_minion.Name));
            if (_spawnStandardBearer)
            {
                m.Status.Add(new StatusEffect("StandardBearer", "Standard Bearer", -1, new Color(0,0,0,0), true));
                _spawnStandardBearer = false;
            }
            _spawnsRemaining--;
            _lastSpawnTime = Time.Scaled;
        }
    }

    public override void PreWaveEffect()
    {
        Retarget();
        _minion.RequestPath(new Int2D(X, Y), _targetTile, _navPath, Team);
    }
    
    public override void WaveEffect()
    {
        _spawnsRemaining += _template.WaveSize + (int)(World.Wave * _template.WaveGrowth);
        _spawnsRemaining += _nextWaveSpawnBonus;
        _nextWaveSpawnBonus = 0;
        if (_template.StandardBearer) _spawnStandardBearer = true;
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
        return true;
    }
    
    public override bool PhysSolid(Minion minion)
    {
        return !minion.IsOrigin(X, Y);
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
            GameConsole.WriteLine("NO TARGETS!");
            return;
        }
        
        int i = World.WeightedRandom(targets.Count);
        _targetTile = targets[i].Value;
    }

    // This is called by path overriding actions, like the brood beacon
    public void SetTarget(Int2D target)
    {
        _targetTile = target;
        _minion.RequestPath(new Int2D(X, Y), _targetTile, _navPath, Team);
        if (!_navPath.Found)
        {
            Team.DemandPath(_navPath);
        }
    }
}