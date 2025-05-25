using System.Numerics;
using Raylib_cs;

namespace nestphalia;

public class BroodMinionTemplate : MinionTemplate
{
    public double SpawnInterval;
    public int SpawnsOnDeath;
    public MinionTemplate SpawnedMinion;
    
    public BroodMinionTemplate(string id, string name, string description, Texture2D texture, double maxHealth, double armor, double damage, double speed, float physicsRadius, double spawnInterval, int spawnsOnDeath, MinionTemplate spawnedMinion, double attackDuration = 1, int walkAnimDelay = 2) 
        : base(id, name, description, texture, maxHealth, armor, damage, speed, physicsRadius, attackDuration, walkAnimDelay)
    {
        SpawnInterval = spawnInterval;
        SpawnsOnDeath = spawnsOnDeath;
        SpawnedMinion = spawnedMinion;
    }
    
    public override void Instantiate(Team team, Vector2 position, NavPath? navPath)
    {
        Register(new BroodMinion(this, team, position, navPath));
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
    
    public BroodMinion(BroodMinionTemplate template, Team team, Vector2 position, NavPath navPath) : base(template, team, position, navPath)
    {
        _lastSpawnTime = Time.Scaled;
        _template = template;
    }

    public override void Update()
    {
        base.Update();
        if (_template.SpawnInterval > 0 && Time.Scaled - _lastSpawnTime >= _template.SpawnInterval)
        {
            _lastSpawnTime = Time.Scaled;
            _template.SpawnedMinion.Instantiate(Team, Position, NavPath.Clone(_template.SpawnedMinion.Name));
        }
    }

    public override void Die()
    {
        base.Die();
        for (int i = 0; i < _template.SpawnsOnDeath; i++)
        {
            Vector2 offset = new Vector2((float)(World.RandomDouble() - 0.5), (float)(World.RandomDouble() - 0.5));
            _template.SpawnedMinion.Instantiate(Team, Position + offset, null);
        }
    }
}