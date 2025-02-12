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
        FloorTiles.Add(new FloorTileTemplate("Blank", Resources.blank));
        
        
        Structures.Add(new StructureTemplate("Mud Wall", Resources.wall, 100, 10, 0));
        Structures.Add(new StructureTemplate("Stone Wall", Resources.wall2, 500, 100, 6));
        
        Structures.Add(new DoorTemplate("Gate", Resources.doorClosed, Resources.doorOpen, 60, 100, 2, 32));
        
        Structures.Add
        (
            new TurretTemplate
            (
                "Watchtower", 
                Resources.turret, 
                80, 
                100, 
                1,
                100,
                new ProjectileTemplate(Resources.bullet, 10, 400), 
                40
            )
        );
        
        // Structures.Add
        // (
        //     new TurretTemplate
        //     (
        //         "Mortar",
        //         Resources.turret,
        //         200,
        //         300,
        //         0,
        //         100,
        //         new ProjectileTemplate(Resources.bullet, 50, 200), 
        //         30
        //     )
        // );
        
        Structures.Add
        (
            new TurretTemplate
            (
                "Machinegunner",
                Resources.turret2,
                160,
                400,
                4,
                100,
                new ProjectileTemplate(Resources.bullet, 5, 400), 
                400
            )
        );
        
        Structures.Add
        (
            new TurretTemplate
            (
                "Sniper",
                Resources.turret3,
                200,
                1000,
                7,
                600,
                new ProjectileTemplate(Resources.bullet, 60, 800), 
                30,
                TurretTemplate.TargetSelector.Random
            )
        );
        
        Structures.Add
        (
            new SpawnerTemplate
            (
                "Anthill", 
                Resources.spawner, 
                80, 
                100, 
                0, 
                new MinionTemplate
                (
                    "Ant", 
                    Resources.smant, 
                    15, 
                    0, 
                    new ProjectileTemplate(Resources.bullet, 5, 400), 
                    32, 
                    30, 
                    80, 
                    false,
                    3
                ), 
                5, 
                1f,
                0.25f
            )
        );
        
        Structures.Add
        (
            new SpawnerTemplate
            (
                "Snail Warren", 
                Resources.spawner2, 
                80, 
                300,
                3,
                new MinionTemplate
                (
                    "Snail", 
                    Resources.snail, 
                    50, 
                    5, 
                    new ProjectileTemplate(Resources.bullet, 15, 400), 
                    32, 
                    20, 
                    45, 
                    false,
                    6
                ), 
                2, 
                0.5f,
                1f
            )
        );
        
        Structures.Add
        (
            new SpawnerTemplate
            (
                "Beehive", 
                Resources.spawner3, 
                80, 
                500, 
                5, 
                new MinionTemplate
                (
                    "Bee", 
                    Resources.bee, 
                    20, 
                    0, 
                    new ProjectileTemplate(Resources.bullet, 5, 400), 
                    32,
                    40,
                    60,
                    true,
                    5
                ), 
                5, 
                1f,
                0.25f
            )
        );  
        
        Structures.Add
        (
            new SpawnerTemplate
            (
                "Beetle Burrow", 
                Resources.spawner4, 
                200, 
                2500, 
                8, 
                new MinionTemplate
                (
                    "Beetle", 
                    Resources.beetle, 
                    100, 
                    10, 
                    new ProjectileTemplate(Resources.bullet, 60, 400), 
                    32, 
                    30, 
                    55, 
                    false,
                    10
                ), 
                1, 
                0.25f,
                6f
            )
        );
        
        Structures.Add
        (
            new SpawnerTemplate
            (
                "TinyAnthill",
                Resources.spawner,
                80,
                50,
                99,
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
                0,
                0,
                0.25f
            )
        );
    }

    public static StructureTemplate? GetTileByName(string name)
    {
        return Structures.FirstOrDefault(x => x.Name == name);
    }
}