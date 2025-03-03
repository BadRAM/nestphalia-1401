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
        Structures.Add(new StructureTemplate("wall_wood", "Wooden Wall", "", Resources.GetTextureByName("wall_wood"), 250, 55, 6, -10));
        Structures.Add(new StructureTemplate("wall_stone", "Stone Wall", "", Resources.GetTextureByName("wall2"), 500, 215, 10, -10));
        
        Structures.Add(new StructureTemplate("honey_pot", "Honey Pot", "Decoy nest", Resources.GetTextureByName("honeypot"), 75, 35, 5, 100));
        Structures.Add(new GluePaperTemplate("glue_paper", "Glue Paper", "", Resources.GetTextureByName("stickypaper"), 150, 25, 8, 0));
        Structures.Add(new HazardSignTemplate("hazard_sign","Hazard Sign", "", Resources.GetTextureByName("hazard_sign"), 30, 10, 15, Double.MinValue));
        
        Structures.Add(new DoorTemplate("door_weak", "Gate", "", Resources.GetTextureByName("doorClosed"), Resources.GetTextureByName("doorOpen"), 80, 50, 1, 5, 32));
        Structures.Add(new DoorTemplate("door_strong","Vault Door", "", Resources.GetTextureByName("vault_door_closed"), Resources.GetTextureByName("vault_door_open"), 250, 1000, 15, 5, 32));
        
        Structures.Add
        (
            new TowerTemplate
            (
                "tower_basic",
                "Watchtower",
                "\n\"Look! Ants!\"",
                Resources.GetTextureByName("turret"), 
                80,
                75,
                0,
                5,
                120,
                new ProjectileTemplate(Resources.GetTextureByName("bullet"), 15, 400), 
                0,
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
                "Millipede Ranger",
                "" +
                "\nWhen his accuracy is questioned, the " +
                "\nMillipede ranger insists that he can " +
                "\nput as many arrows in a target as a " +
                "\ndozen ant archers.",
                Resources.GetTextureByName("turret2"),
                95,
                400,
                2,
                5,
                100,
                new ProjectileTemplate(Resources.GetTextureByName("bullet"), 5, 400), 
                0,
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
                "\nFriendly bugs can safely traverse mines, " +
                "\nbut hate doing so.",
                Resources.GetTextureByName("minefield"),
                80,
                85,
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
                "Splash damage, radius 48\nCan only hit grounded units",
                Resources.GetTextureByName("mortar"),
                75,
                500,
                4,
                5,
                90,
                new MortarShellTemplate(Resources.GetTextureByName("bullet"), 10, 0.7, 96, 48),
                0,
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
                "Can only hit midair units",
                Resources.GetTextureByName("tower_lightning"),
                160,
                600,
                7,
                5,
                120,
                new LightningBoltTemplate(10), 
                46,
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
                "Mantis Sniper",
                "\n\"Don't tell my wife where I am.\"",
                Resources.GetTextureByName("turret3"),
                60,
                3000,
                14,
                5,
                450,
                new ProjectileTemplate(Resources.GetTextureByName("bullet"), 60, 800), 
                0,
                25,
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
                "Rally Stratagem", 
                "Commands all your bugs to a single target\n\n\"Strength in numbers! They can't stop all " +
                "\nof us!\"", 
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
                "Repair Stratagem",
                "Rebuilds one structure, and adjacent walls\n\n",
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
                "Brood Stratagem",
                "Grants adjacent nests an extra wave of bugs",
                Resources.GetTextureByName("flag2"),
                100,
                2000,
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
                "Frenzy Stratagem",
                "Causes a frenzy in an area",
                Resources.GetTextureByName("flag1"),
                100,
                1250,
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
                    "\nAntz is made for Fight'n and Winn'n!",
                    Resources.GetTextureByName("smant"), 
                    20, 
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
                    "\nHoppers are known for their free spirits \nand musical talent",
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
                    "\n\"For the Queen!\"",
                    Resources.GetTextureByName("bee"), 
                    20, 
                    0, 
                    5, 
                    45,
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
                1200,
                8,
                50,
                new MinionTemplate
                (
                    "minion_snail",
                    "Snail", 
                    "\nSnails get it done.",
                    Resources.GetTextureByName("snail"), 
                    60, 
                    5, 
                    15, 
                    35, 
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
                "Sapper Beetles attack once, then return to \ntheir nest to join the next wave",
                Resources.GetTextureByName("spawner4"),
                50,
                2500,
                11,
                50,
                new SapperMinionTemplate
                (
                    "minion_sapper_beetle",
                    "Beetle",
                    "\nWide load coming through!",
                    Resources.GetTextureByName("sapper_attacking"),
                    Resources.GetTextureByName("sapper_retreating"),
                    55, 
                    2, 
                    250, 
                    65, 
                    8
                ), 
                1, 
                0,
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
                        "\n\"I was born ready!\"",
                        Resources.GetTextureByName("spiderling"),
                        15,
                        0,
                        2,
                        55,
                        2,
                        0.5
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
                10000,
                15,
                50,
                new HopperMinionTemplate
                (
                    "minion_frog",
                    "Frog",
                    "\nAt least it's on our side.\nFor now...",
                    Resources.GetTextureByName("frog"),
                    1500, 
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