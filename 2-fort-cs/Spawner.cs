using ZeroElectric.Vinculum;

namespace _2_fort_cs;

public class SpawnerTemplate : StructureTemplate
{
    public MinionTemplate Minion;
    public int WaveSize;
    public float WaveGrowth;
    public float TimeBetweenSpawns;
    
    public SpawnerTemplate(string name, Texture texture, float maxHealth, float price, int levelRequirement, MinionTemplate minion, int waveSize, float waveGrowth, float timeBetweenSpawns) : base(name, texture, maxHealth, price, levelRequirement)
    {
        Minion = minion;
        WaveSize = waveSize;
        WaveGrowth = waveGrowth;
        TimeBetweenSpawns = timeBetweenSpawns;
    }

    public override Spawner Instantiate(int x, int y)
    {
        return new Spawner(this, x, y);
    }
}

public class Spawner : Structure
{
    private SpawnerTemplate _template;
    private float _lastSpawnTime;
    private int _spawnsRemaining;
    private Int2D _targetTile;
    
    public Spawner(SpawnerTemplate template, int x, int y) : base(template, x, y)
    {
        _template = template;
    }
    
    public override void Update()
    {
        base.Update();
        if (_spawnsRemaining > 0 && Raylib.GetTime() - _lastSpawnTime > _template.TimeBetweenSpawns)
        {
            _template.Minion.Instantiate(position, Team, _targetTile);
            _spawnsRemaining--;
            _lastSpawnTime = (float)Raylib.GetTime();
        }
    }

    public override void WaveEffect()
    {
        Retarget();
        _spawnsRemaining += _template.WaveSize + (int)(World.Wave * _template.WaveGrowth);
    }

    public override bool IsSolid()
    {
        return true;
    }

    private void Retarget()
    {
        List<Int2D> targets = new List<Int2D>();
        
        for (int x = 0; x < World.BoardWidth; ++x)
        {
            for (int y = 0; y < World.BoardHeight; ++y)
            {
                if (World.GetTile(x,y) != null && World.GetTile(x,y).Team != Team)
                {
                    targets.Add(new Int2D(x,y));
                }
            }
        }

        if (targets.Count == 0)
        {
            return;
        }

        _targetTile = targets[Random.Shared.Next(targets.Count)];
    }
}