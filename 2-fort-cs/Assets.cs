namespace _2_fort_cs;

public static class Assets
{
    public static List<FloorTileTemplate> FloorTiles = new List<FloorTileTemplate>();
    public static List<StructureTemplate> Structures = new List<StructureTemplate>();
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
        
        FloorTiles.Add(new FloorTileTemplate("Floor1", Resources.floor1));
        FloorTiles.Add(new FloorTileTemplate("Floor2", Resources.floor2));
        Structures.Add(new StructureTemplate("Wall", Resources.wall, 100));
        Structures.Add(new TurretTemplate("Turret", Resources.turret, 200, 100, Projectiles[0], 60));
        Structures.Add(new SpawnerTemplate("Spawner", Resources.spawner, 200, Minions[0], 3, 0.5f));
    }

    public static StructureTemplate? GetTileByName(string name)
    {
        return Structures.FirstOrDefault(x => x.Name == name);
    }
}