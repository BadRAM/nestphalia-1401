namespace _2_fort_cs;

public static class Assets
{
    public static List<TileTemplate> Tiles = new List<TileTemplate>();
    public static List<MinionTemplate> Minions = new List<MinionTemplate>();
    public static List<ProjectileTemplate> Projectiles = new List<ProjectileTemplate>();

    public static void Load()
    {
        Projectiles.Add(new ProjectileTemplate(Resources.bullet, 5, 400));
        
        Minions.Add
        (new MinionTemplate
            (
                "TestMinion", 
                Resources.wabbit, 
                20, 
                0, 
                Projectiles[0], 
                32, 
                60, 
                50, 
                false,
                6
            )
        );
        
        Tiles.Add(new TileTemplate("Floor1", Resources.floor1));
        Tiles.Add(new TileTemplate("Floor2", Resources.floor2));
        Tiles.Add(new StructureTemplate("Wall", Resources.wall, 100));
        Tiles.Add(new TurretTemplate("Turret", Resources.turret, 200, 100, Projectiles[0], 60));
        Tiles.Add(new SpawnerTemplate("Spawner", Resources.spawner, 200, Minions[0], 3, 0.5f));
    }

    public static TileTemplate? GetTileByName(string name)
    {
        return Tiles.FirstOrDefault(x => x.Name == name);
    }
}