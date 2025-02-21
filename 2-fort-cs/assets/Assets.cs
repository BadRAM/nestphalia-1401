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
        
        Structures.Add(new StructureTemplate("wall_mud", "Mud Wall", "", Resources.GetTextureByName("wall"), 100, 10, 0, -10));
        Structures.Add(new StructureTemplate("wall_stone", "Stone Wall", "", Resources.GetTextureByName("wall2"), 500, 100, 6, -10));
        
        Structures.Add(new StructureTemplate("honey_pot", "Honey Pot", "", Resources.GetTextureByName("honeypot"), 100, 10, 0, 100));
        Structures.Add(new GluePaperTemplate("glue_paper", "Glue Paper", "", Resources.GetTextureByName("stickypaper"), 100, 10, 0, 100));
        Structures.Add(new HazardSignTemplate("hazard_sign","Hazard Sign", "", Resources.GetTextureByName("hazard_sign"), 30, 10, 0, Double.MinValue));

        Structures.Add(new DoorTemplate("door_weak", "Gate", "", Resources.GetTextureByName("doorClosed"), Resources.GetTextureByName("doorOpen"), 60, 100, 2, 5, 32));
        Structures.Add(new DoorTemplate("door_strong","Vault Door", "", Resources.GetTextureByName("vault_door_closed"), Resources.GetTextureByName("vault_door_open"), 250, 1000, 2, 5, 32));
        
        Structures.Add
        (
            new TowerTemplate
            (
                "tower_basic",
                "Watchtower",
                "",
                Resources.GetTextureByName("turret"), 
                80, 
                100, 
                1,
                5,
                100,
                new ProjectileTemplate(Resources.GetTextureByName("bullet"), 10, 400), 
                40,
                TowerTemplate.TargetSelector.Nearest,
                true,
                true
            )
        );
        
        Structures.Add
        (
            new TowerTemplate
            (
                "tower_mortar",
                "Mortar",
                "",
                Resources.GetTextureByName("mortar"),
                200,
                300,
                0,
                5,
                72,
                new MortarShellTemplate(Resources.GetTextureByName("bullet"), 10, 0.8, 96, 48), 
                30,
                TowerTemplate.TargetSelector.Random,
                true,
                false
            )
        );
        
        Structures.Add
        (
            new TowerTemplate
            (
                "tower_machinegun",
                "Machinegunner",
                "",
                Resources.GetTextureByName("turret2"),
                160,
                400,
                4,
                5,
                100,
                new ProjectileTemplate(Resources.GetTextureByName("bullet"), 5, 400), 
                400,
                TowerTemplate.TargetSelector.Nearest,
                true,
                true
            )
        );
        
        Structures.Add
        (
            new TowerTemplate
            (
                "tower_sniper",
                "Sniper",
                "",
                Resources.GetTextureByName("turret3"),
                200,
                1000,
                7,
                5,
                600,
                new ProjectileTemplate(Resources.GetTextureByName("bullet"), 60, 800), 
                30,
                TowerTemplate.TargetSelector.Random,
                true,
                true
            )
        );
        
        Structures.Add
        (
            new MinefieldTemplate
            (
                "minefield",
                "Minefield",
                "",
                Resources.GetTextureByName("minefield"),
                100,
                100,
                0,
                Double.MinValue, 
                4,
                new MortarShellTemplate(Resources.GetTextureByName("mine"), 50, 0.4, 0, 32),
                12,
                5
            )
        );
        
        Structures.Add
        (
            new SpawnerTemplate
            (
                "nest_ant",
                "Anthill", 
                "",
                Resources.GetTextureByName("spawner"), 
                80, 
                100, 
                0, 
                50,
                new MinionTemplate
                (
                    "minion_ant",
                    "Ant", 
                    "",
                    Resources.GetTextureByName("smant"), 
                    15, 
                    0, 
                    5, 
                    80, 
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
                "nest_snail",
                "Snail Warren", 
                "",
                Resources.GetTextureByName("spawner2"), 
                80, 
                300,
                3,
                50,
                new MinionTemplate
                (
                    "minion_snail",
                    "Snail", 
                    "",
                    Resources.GetTextureByName("snail"), 
                    50, 
                    5, 
                    15, 
                    20, 
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
                "nest_bee",
                "Beehive", 
                "",
                Resources.GetTextureByName("spawner3"), 
                80, 
                500, 
                5, 
                50,
                new FlyingMinionTemplate
                (
                    "minion_bee",
                    "Bee",
                    "",
                    Resources.GetTextureByName("bee"), 
                    20, 
                    0, 
                    5, 
                    40,
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
                "nest_beetle",
                "Beetle Burrow", 
                "",
                Resources.GetTextureByName("spawner4"), 
                200, 
                2500, 
                8, 
                50,
                new MinionTemplate
                (
                    "minion_beetle",
                    "Beetle", 
                    "",
                    Resources.GetTextureByName("beetle"), 
                    100, 
                    10, 
                    60, 
                    30, 
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
                "nest_spider",
                "Spider Nest", 
                "",
                Resources.GetTextureByName("spawner4"), 
                200, 
                2500, 
                8, 
                50,
                new BroodMinionTemplate
                (
                    "minion_spider",
                    "Spider Broodmother",
                    "",
                    Resources.GetTextureByName("spidermom"), 
                    100, 
                    0, 
                    20, 
                    30, 
                    10,
                    1,
                    24,
                    new MinionTemplate
                    (
                        "minion_spiderling",
                        "Spiderling", 
                        "",
                        Resources.GetTextureByName("spiderling"), 
                        15, 
                        0, 
                         5, 
                        30, 
                        2
                    )
                ), 
                1, 
                0.5,
                6
            )
        );
        
        Structures.Add
        (
            new SpawnerTemplate
            (
                "nest_fake",
                "TinyAnthill",
                "",
                Resources.GetTextureByName("spawner"),
                80,
                50,
                99,
                50,
                new MinionTemplate
                (
                    "minion_fake",
                    "Ant",
                    "",
                    Resources.GetTextureByName("wabbit"),
                    10,
                    0,
                    5, 
                    30,
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