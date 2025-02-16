namespace _2_fort_cs;

public static class Assets
{
    public static List<FloorTileTemplate> FloorTiles = new List<FloorTileTemplate>();
    public static List<StructureTemplate> Structures = new List<StructureTemplate>();
    //public static List<MinionTemplate> Minions = new List<MinionTemplate>();
    //public static List<ProjectileTemplate> Projectiles = new List<ProjectileTemplate>();

    public static void Load()
    {
        FloorTiles.Add(new FloorTileTemplate("Floor1", Resources.GetTextureByName("floor1")));
        FloorTiles.Add(new FloorTileTemplate("Floor2", Resources.GetTextureByName("floor2")));
        FloorTiles.Add(new FloorTileTemplate("Blank", Resources.GetTextureByName("clear")));
        
        Structures.Add(new StructureTemplate("Mud Wall", Resources.GetTextureByName("wall"), 100, 10, 0, -10));
        Structures.Add(new StructureTemplate("Stone Wall", Resources.GetTextureByName("wall2"), 500, 100, 6, -10));
        
        Structures.Add(new StructureTemplate("Honey Pot", Resources.GetTextureByName("honeypot"), 100, 10, 0, 100));

        Structures.Add(new DoorTemplate("Gate", Resources.GetTextureByName("doorClosed"), Resources.GetTextureByName("doorOpen"), 60, 100, 2, 5, 32));
        
        Structures.Add
        (
            new TurretTemplate
            (
                "Watchtower", 
                Resources.GetTextureByName("turret"), 
                80, 
                100, 
                1,
                5,
                100,
                new ProjectileTemplate(Resources.GetTextureByName("bullet"), 10, 400), 
                40,
                TurretTemplate.TargetSelector.Nearest,
                true,
                true
            )
        );
        
        Structures.Add
        (
            new TurretTemplate
            (
                "Mortar",
                Resources.GetTextureByName("mortar"),
                200,
                300,
                0,
                5,
                72,
                new MortarShellTemplate(Resources.GetTextureByName("bullet"), 10, 0.8, 96, 48), 
                30,
                TurretTemplate.TargetSelector.Random,
                true,
                false
            )
        );
        
        Structures.Add
        (
            new TurretTemplate
            (
                "Machinegunner",
                Resources.GetTextureByName("turret2"),
                160,
                400,
                4,
                5,
                100,
                new ProjectileTemplate(Resources.GetTextureByName("bullet"), 5, 400), 
                400,
                TurretTemplate.TargetSelector.Nearest,
                true,
                true
            )
        );
        
        Structures.Add
        (
            new TurretTemplate
            (
                "Sniper",
                Resources.GetTextureByName("turret3"),
                200,
                1000,
                7,
                5,
                600,
                new ProjectileTemplate(Resources.GetTextureByName("bullet"), 60, 800), 
                30,
                TurretTemplate.TargetSelector.Random,
                true,
                true
            )
        );
        
        Structures.Add
        (
            new SpawnerTemplate
            (
                "Anthill", 
                Resources.GetTextureByName("spawner"), 
                80, 
                100, 
                0, 
                50,
                new MinionTemplate
                (
                    "Ant", 
                    Resources.GetTextureByName("smant"), 
                    15, 
                    0, 
                    new ProjectileTemplate(Resources.GetTextureByName("bullet"), 5, 400), 
                    32, 
                    30, 
                    80, 
                    false,
                    3
                ), 
                5, 
                1,
                0.25
            )
        );
        
        Structures.Add
        (
            new SpawnerTemplate
            (
                "Snail Warren", 
                Resources.GetTextureByName("spawner2"), 
                80, 
                300,
                3,
                50,
                new MinionTemplate
                (
                    "Snail", 
                    Resources.GetTextureByName("snail"), 
                    50, 
                    5, 
                    new ProjectileTemplate(Resources.GetTextureByName("bullet"), 15, 400), 
                    32, 
                    20, 
                    45, 
                    false,
                    6
                ), 
                2, 
                0.5,
                1
            )
        );
        
        Structures.Add
        (
            new SpawnerTemplate
            (
                "Beehive", 
                Resources.GetTextureByName("spawner3"), 
                80, 
                500, 
                5, 
                50,
                new MinionTemplate
                (
                    "Bee", 
                    Resources.GetTextureByName("bee"), 
                    20, 
                    0, 
                    new ProjectileTemplate(Resources.GetTextureByName("bullet"), 5, 400), 
                    32,
                    40,
                    60,
                    true,
                    5
                ), 
                5, 
                1,
                0.25
            )
        );  
        
        Structures.Add
        (
            new SpawnerTemplate
            (
                "Beetle Burrow", 
                Resources.GetTextureByName("spawner4"), 
                200, 
                2500, 
                8, 
                50,
                new MinionTemplate
                (
                    "Beetle", 
                    Resources.GetTextureByName("beetle"), 
                    100, 
                    10, 
                    new ProjectileTemplate(Resources.GetTextureByName("bullet"), 60, 400), 
                    32, 
                    30, 
                    55, 
                    false,
                    10
                ), 
                1, 
                0.25,
                6
            )
        );
        
        Structures.Add
        (
            new SpawnerTemplate
            (
                "TinyAnthill",
                Resources.GetTextureByName("spawner"),
                80,
                50,
                99,
                50,
                new MinionTemplate
                (
                    "Ant",
                    Resources.GetTextureByName("wabbit"),
                    10,
                    0,
                    new ProjectileTemplate(Resources.GetTextureByName("bullet"), 5, 400),
                    32,
                    30,
                    50,
                    false,
                    3
                ), 
                0,
                0,
                0.25
            )
        );
    }

    public static StructureTemplate? GetTileByName(string name)
    {
        return Structures.FirstOrDefault(x => x.Name == name);
    }
}