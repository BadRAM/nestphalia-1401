using ZeroElectric.Vinculum;

namespace _2_fort_cs;

public class SpawnerTemplate : StructureTemplate
{
    public MinionTemplate Minion;
    public int WaveSize;
    public float TimeBetweenSpawns;
    
    public SpawnerTemplate(string name, Texture texture, float maxHealth, MinionTemplate minion, int waveSize, float timeBetweenSpawns) : base(name, texture, maxHealth)
    {
        Minion = minion;
        WaveSize = waveSize;
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
    
    public Spawner(SpawnerTemplate template, int x, int y) : base(template, x, y)
    {
        _template = template;
    }

    public override void Update()
    {
        base.Update();
        if (_spawnsRemaining > 0 && Raylib.GetTime() - _lastSpawnTime > _template.TimeBetweenSpawns)
        {
            _template.Minion.Instantiate(position, Team);
            _spawnsRemaining--;
            _lastSpawnTime = (float)Raylib.GetTime();
        }
    }

    public override void WaveEffect()
    {
        _spawnsRemaining += _template.WaveSize + World.Wave;
    }

    public override bool IsSolid()
    {
        return true;
    }
}