namespace _2_fort_cs;

public static class Assets
{
    public static List<FloorTileTemplate> FloorTiles = new List<FloorTileTemplate>();
    public static List<StructureTemplate> Structures = new List<StructureTemplate>();
    public static List<MinionTemplate> Minions = new List<MinionTemplate>();
    //public static List<ProjectileTemplate> Projectiles = new List<ProjectileTemplate>();

    public static void Load()
    {
        //Projectiles.Add(new ProjectileTemplate(Resources.bullet, 5, 400));
        
        FloorTiles.Add(new FloorTileTemplate("Floor1", Resources.floor1));
        FloorTiles.Add(new FloorTileTemplate("Floor2", Resources.floor2));
        
        
        Structures.Add(new StructureTemplate("Mud Wall", Resources.wall, 100, 10, 0));
        Structures.Add(new StructureTemplate("Stone Wall", Resources.wall, 500, 100, 4));
        
        
        Structures.Add
        (
            new TurretTemplate
            (
                "Watchtower", 
                Resources.turret, 
                200, 
                100, 
                0,
                100,
                new ProjectileTemplate(Resources.bullet, 5, 400), 
                60
            )
        );
        
        Structures.Add
        (
            new TurretTemplate
            (
                "Bomb Mortar",
                Resources.turret,
                200,
                300,
                0,
                100,
                new ProjectileTemplate(Resources.bullet, 5, 400), 
                30
            )
        );
        
        Structures.Add
        (
            new TurretTemplate
            (
                "Sniper",
                Resources.turret,
                200,
                500,
                0,
                1000,
                new ProjectileTemplate(Resources.bullet, 50, 400), 
                10
            )
        );
        
        Structures.Add
        (
            new TurretTemplate
            (
                "Machinegunner",
                Resources.turret,
                200,
                300,
                0,
                100,
                new ProjectileTemplate(Resources.bullet, 5, 400), 
                300
            )
        );
        
        
        Structures.Add
        (
            new SpawnerTemplate
            (
                "Anthill", 
                Resources.spawner, 
                200, 
                100, 
                0, 
                new MinionTemplate
                (
                    "Ant", 
                    Resources.wabbit, 
                    10, 
                    0, 
                    new ProjectileTemplate(Resources.bullet, 5, 400), 
                    32, 
                    30, 
                    50, 
                    false,
                    3
                ), 
                5, 
                0.25f
            )
        );
        
        Structures.Add
        (
            new SpawnerTemplate
            (
                "Snail Warren", 
                Resources.spawner, 
                200, 
                300,
                3,
                new MinionTemplate
                (
                    "Snail", 
                    Resources.wabbit, 
                    50, 
                    5, 
                    new ProjectileTemplate(Resources.bullet, 5, 400), 
                    32, 
                    20, 
                    25, 
                    false,
                    6
                ), 
                2, 
                1f
            )
        );
        
        
        Structures.Add
        (
            new SpawnerTemplate
            (
                "Beehive", 
                Resources.spawner, 
                200, 
                100, 
                0, 
                new MinionTemplate
                (
                    "Bee", 
                    Resources.wabbit, 
                    10, 
                    0, 
                    new ProjectileTemplate(Resources.bullet, 5, 400), 
                    32, 
                    30, 
                    50, 
                    true,
                    5
                ), 
                5, 
                0.25f
            )
        );  
        
        Structures.Add
        (
            new SpawnerTemplate
            (
                "Beetle Burrow", 
                Resources.spawner, 
                200, 
                100, 
                0, 
                new MinionTemplate
                (
                    "Beetle", 
                    Resources.wabbit, 
                    10, 
                    0, 
                    new ProjectileTemplate(Resources.bullet, 5, 400), 
                    32, 
                    30, 
                    50, 
                    false,
                    6
                ), 
                5, 
                0.25f
            )
        );        
    }

    public static StructureTemplate? GetTileByName(string name)
    {
        return Structures.FirstOrDefault(x => x.Name == name);
    }
}