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
        Structures.Add(new StructureTemplate("wall_wood", "Wooden Wall", "", Resources.GetTextureByName("wall_wood"), 250, 45, 6, -10));
        Structures.Add(new StructureTemplate("wall_stone", "Stone Wall", "", Resources.GetTextureByName("wall2"), 500, 115, 10, -10));
        
        Structures.Add(new StructureTemplate("honey_pot", "Honey Pot", "", Resources.GetTextureByName("honeypot"), 75, 35, 5, 100));
        Structures.Add(new GluePaperTemplate("glue_paper", "Glue Paper", "", Resources.GetTextureByName("stickypaper"), 100, 10, 8, 0));
        Structures.Add(new HazardSignTemplate("hazard_sign","Hazard Sign", "", Resources.GetTextureByName("hazard_sign"), 30, 10, 0, Double.MinValue));

        Structures.Add(new DoorTemplate("door_weak", "Gate", "", Resources.GetTextureByName("doorClosed"), Resources.GetTextureByName("doorOpen"), 95, 100, 1, 5, 32));
        Structures.Add(new DoorTemplate("door_strong","Vault Door", "", Resources.GetTextureByName("vault_door_closed"), Resources.GetTextureByName("vault_door_open"), 250, 1000, 15, 5, 32));
        
        Structures.Add
        (
            new TowerTemplate
            (
                "tower_basic",
                "Watchtower",
                "",
                Resources.GetTextureByName("turret"), 
                80,
                75,
                0,
                5,
                120,
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
                "tower_machinegun",
                "Machine Gun",
                "",
                Resources.GetTextureByName("turret2"),
                160,
                400,
                2,
                5,
                100,
                new ProjectileTemplate(Resources.GetTextureByName("bullet"), 5, 400), 
                400,
                TowerTemplate.TargetSelector.RandomFocus,
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
                55,
                2,
                Double.MinValue, 
                4,
                new MortarShellTemplate(Resources.GetTextureByName("mine"), 20, 0.4, 0, 32),
                12,
                5
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
                4,
                5,
                90,
                new MortarShellTemplate(Resources.GetTextureByName("bullet"), 10, 0.7, 96, 48), 
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
                "tower_lightning",
                "Lightning Tower",
                "",
                Resources.GetTextureByName("tower_lightning"),
                160,
                400,
                7,
                5,
                120,
                new LightningBoltTemplate(10), 
                400,
                TowerTemplate.TargetSelector.Random,
                false,
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
                14,
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
            new RallyBeaconTemplate
            (
                "beacon_rally",
                "Rally Beacon", 
                "Commands all your bugs to a single target", 
                Resources.GetTextureByName("flag1"), 
                100,
                750,
                3,
                50,
                50,
                10,
                Resources.GetTextureByName("ability_icon_rally")
            )
        );
        
        Structures.Add
        (
            new RepairBeaconTemplate
            (
                "beacon_repair",
                "Repair Beacon",
                "Rebuilds one structure",
                Resources.GetTextureByName("flag2"),
                100,
                1000,
                6,
                50,
                35,
                10,
                Resources.GetTextureByName("ability_icon_repair")
            )
        );
        
        Structures.Add
        (
            new SpawnBoostBeaconTemplate
            (
                "beacon_spawnboost",
                "Brood Beacon",
                "Grants a nest an extra wave of bugs",
                Resources.GetTextureByName("flag2"),
                100,
                1000,
                9,
                50,
                42,
                10,
                Resources.GetTextureByName("ability_icon_spawnboost")
            )
        );
        
        Structures.Add
        (
            new FrenzyBeaconTemplate
            (
                "beacon_frenzy",
                "Frenzy Beacon",
                "Causes a frenzy in an area",
                Resources.GetTextureByName("flag1"),
                100,
                1000,
                12,
                50,
                45,
                10,
                Resources.GetTextureByName("ability_icon_frenzy")
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
                50, 
                250, 
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
                    75, 
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
                "nest_grasshopper",
                "Grasshopper Glade",
                "",
                Resources.GetTextureByName("spawner4"),
                50,
                400,
                3,
                50,
                new HopperMinionTemplate
                (
                    "minion_grasshopper",
                    "Grasshopper",
                    "",
                    Resources.GetTextureByName("hopper"),
                    40, 
                    0, 
                    10, 
                    40, 
                    5
                ), 
                3, 
                1,
                0.65
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
                50, 
                650, 
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
                "nest_snail",
                "Snail Warren", 
                "",
                Resources.GetTextureByName("spawner2"), 
                50, 
                300,
                8,
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
                "nest_sapper_beetle",
                "Beetle Burrow",
                "Sapper Beetles will launch a single powerful attack, then return to their nest to join the next wave",
                Resources.GetTextureByName("spawner4"),
                50,
                2500,
                8,
                50,
                new SapperMinionTemplate
                (
                    "minion_sapper_beetle",
                    "Beetle",
                    "",
                    Resources.GetTextureByName("sapper_attacking"),
                    Resources.GetTextureByName("sapper_retreating"),
                    50, 
                    0, 
                    200, 
                    65, 
                    10
                ), 
                1, 
                0.25,
                1
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
                50, 
                2500, 
                13, 
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
                "nest_frog",
                "Frog Pond",
                "",
                Resources.GetTextureByName("spawner4"),
                50,
                2500,
                15,
                50,
                new HopperMinionTemplate
                (
                    "minion_frog",
                    "Frog",
                    "",
                    Resources.GetTextureByName("frog"),
                    1000, 
                    0, 
                    50, 
                    20, 
                    10
                ), 
                1, 
                0.25,
                4
            )
        );
        
        // Structures.Add
        // (
        //     new SpawnerTemplate
        //     (
        //         "nest_fake",
        //         "TinyAnthill",
        //         "",
        //         Resources.GetTextureByName("spawner"),
        //         80,
        //         50,
        //         99,
        //         50,
        //         new MinionTemplate
        //         (
        //             "minion_fake",
        //             "Ant",
        //             "",
        //             Resources.GetTextureByName("wabbit"),
        //             10,
        //             0,
        //             5, 
        //             30,
        //             3
        //         ), 
        //         0,
        //         0,
        //         0.25
        //     )
        // );
    }

    public static StructureTemplate? GetTileByID(string ID)
    {
        return Structures.FirstOrDefault(x => x.ID == ID);
    }
}