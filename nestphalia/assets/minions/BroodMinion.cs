using System.Numerics;
using Newtonsoft.Json.Linq;

namespace nestphalia;

public class BroodMinionTemplate : MinionTemplate
{
    public double SpawnInterval;
    public int SpawnsOnDeath;
    public MinionTemplate SpawnedMinion;
    
    public BroodMinionTemplate(JObject jObject) : base(jObject)
    {
        SpawnInterval = jObject.Value<double?>("SpawnInterval") ?? 0;
        SpawnsOnDeath = jObject.Value<int?>("SpawnsOnDeath") ?? 0;
        SpawnedMinion = Assets.LoadJsonAsset<MinionTemplate>(jObject.Value<JObject?>("SpawnedMinion")) ?? throw new ArgumentNullException();
    }
    
    public override void Instantiate(Team team, Vector3 position, NavPath? navPath)
    {
        World.RegisterMinion(new BroodMinion(this, team, position, navPath));
    }

    public override string GetStats()
    {
        return                
            $"{Name}\n" +
            $"HP: {MaxHealth}\n" +
            (Armor == 0 ? "" : $"Armor: {Armor}\n") +
            $"Speed: {Speed}\n" +
            $"Damage: {Projectile.Damage} ({Projectile.Damage / AttackDuration}/s)\n" +
            $"Size: {PhysicsRadius * 2}\n" +
            $"spawns 1 {SpawnedMinion.Name} every {SpawnInterval}s\n" +
            $"spawns {SpawnsOnDeath} on death\n\n" +
            $"{SpawnedMinion.GetStats()}\n" +
            $"{Description}";
    }
}
    
public class BroodMinion : Minion
{
    private double _lastSpawnTime;
    private BroodMinionTemplate _template;
    
    public BroodMinion(BroodMinionTemplate template, Team team, Vector3 position, NavPath navPath) : base(template, team, position, navPath)
    {
        _lastSpawnTime = Time.Scaled;
        _template = template;
    }

    public override void Update()
    {
        base.Update();
        if (_template.SpawnInterval > 0 && Time.Scaled - _lastSpawnTime >= _template.SpawnInterval && !World.IsBattleOver())
        {
            _lastSpawnTime = Time.Scaled;
            _template.SpawnedMinion.Instantiate(Team, Position, NavPath.Clone(_template.SpawnedMinion.Name));
        }
    }

    public override void Die()
    {
        base.Die();
        if (World.IsBattleOver()) return;
        for (int i = 0; i < _template.SpawnsOnDeath; i++)
        {
            Vector3 offset = new Vector3((float)(World.RandomDouble() - 0.5), (float)(World.RandomDouble() - 0.5), 0f);
            _template.SpawnedMinion.Instantiate(Team, Position + offset, null);
        }
    }
}