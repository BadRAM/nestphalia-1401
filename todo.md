 == This file is for BadRAM's personal organization. ==


Project management todo:
========================

- Title change?
- Get art design package ready for Numb Dame
  - Art Design Document
    - Color Palette
    - Example of audio crunch level, general music references.
    - Key visuals in palette
    - Features of visual style
      - Level of detail in rendering? (detail + lighting + texturing)
      - Textured or smooth?
      - Lighting? (specular, shading)
      - Detail? (Are large flat areas allowed or should everything be greebled?)
      - Number of colors per sprite
      - Number of shades per color
      - Contrasting cartoonish and photo-derived assets or no?
      - Prerendered 3d elements?
      - Outlines? Only for bugs or background?
  - Campaign plot summary
  - Intro Cutscene storyboard

Programming todo:
=================

----- V2 -----

- Game intro cutscene
- Loading screen
- Dialog boxes and scripted events at start of levels
  - Allow dialog boxes to provide a list of choices
  - SFX?
- Nice title screen
- Nice credits screen
- 2-3 more bugs & towers
  - Ranged attack bug
  - ~~Another flyer. Ladybug? Flies until attacked.~~
  - Roaches - Gets within a radius of target then flies close to it.
  - Snail rework
  - Beetle rework. Attacks can damage the ball?
  - Centipede that gets longer and shorter
  - Tower with a big sawblade on an arm that slowly orbits
  - Springboard trap that launches a bug towards the other fort based on it's weight
  - Glue paper rework - Maybe it just works forever, but only affects bugs standing on it?
  - Multitarget tower, attacks 3 units simultaneously, but can't focus all attacks on one target
  - Penetrating tower, like spike roller from bloons
  - Direct damage minions in radius stratagem
  - Freeze minions in radius stratagem
  - Build new wall/tower stratagem (Can it build in the neutral zone, or even the enemy fort?)
  - Create/remove fear stratagem
- Crush damage
- Minion Status Effect System
- Make Frenzy Beacon work again
- Make nests solid again
- Command console
  - Interface
  - Command format, usable by json levels to script battle events.
- Better level format, 
  - allow structures in center
  - customize floor
  - scripted events
  - soil texture triangle (meaningless)
- Nice campaign screen with a map and paths appearing every time you unlock a new level. Nonlinear campaign?
- Design campaign levels
- Corpses/bloodstains to indicate fear
- Balance decision: Should flight grant immunity to explosions? Currently it does not.
- Orphaned sapper beetles will seek another burrow
- Help text / Tutorial
  - Dialog box
  - Campaign intro tutorial
  - Bugspedia
- ~~Minion animation~~
  - ~~Add standing frame~~
  - Attack anim?
    - Special minion attack projectile type, slash effect? pow?
    - Animate the minion's position a little in and out
  - ~~Jump anim + hoppers~~
  - ~~Sappers~~
  - ~~Spidermom + spiderling~~
  - ~~Bees~~
  - ~~Frog~~
- Team colored structures, Stratagem banners in particular
- Animated structures
  - Stratagems raise their flag as they charge, or is it when they activate?
- ~~Settings menu scene~~
  - ~~Music and SFX volume sliders~~
  - Add button to relaunch game when High DPI mode toggle selected
- More Sounds
  - Bespoke music from music peoples
  - Button hover and new button click
  - Bug Death
  - Bug take damage
  - Tower Shoot
- File Select popup
- Migrate old JSON functions to newtonsoft
- Stateful GUI


----- Premature Optimizations -----

- Codebase Refactoring
  - Get rid of as many public fields as possible
    - ~~Battlescene should accept callback function to report victory~~
  - ~~Integrate Screen.HCenter/VCenter into GUI functions~~
  - Minion
    - ~~State machine~~
    - ~~Extract physics~~
    - Move flying behavior into base minion
  - Projectile
    - Convert into general purpose gameEntity
  - World
    - make it not static, with a singleton-like static reference
    - keep it static, but move all it's state into a 'WorldState' object that isn't static.
    - move all world function into a 'worldInstance' class, and leave World as a wrapper to it
  - BattleScene
    - Camera shake stores just the last offset.
  - Team
    - ~~Move pathqueue and some pathfinding into team~~
    - Move target selection into team
  - Pathfinder
    - ~~Make it not static so it can be multithreaded~~

- Game Logic optimizations
  - ~~profile tile entities separately by ID~~

- Physics optimizations
  - ~~Sector based minion culling/lookup~~
    - ~~No significant difference~~
    - ~~11000 minions slows the game to 50% speed on my laptop.~~
  - ~~Multi Threading~~
    - ~~multithreading collision made the game slower~~
    - ~~Giving pathfinding a background task while collision detection happens on the main thread seems to be helping a little bit.~~
  - ~~Terrain collision doesn't need to figure out what sector (side/corner/center) of the tile it's in because for each tile you consider you already know from the offset.~~
  - Fix left side tug of war bias

- Pathfinding optimizations:
  - ~~note: published builds seem to run about 2x faster than debug mode in IDE~~
  - ~~as soon as a node is set in the targeted half of the battlefield, purge all nodes and dont' allow any new ones to be created in the untargeted half~~
    - ~~This feels too complex, limiting, and like it could cause difficult bugs. A more generalized chunking method would be better, but only if the game grows and pathing performance becomes a problem again~~
  - ~~Binary search to find consider queue insertion point~~
    - ~~Before: Registernode 30%, avg pathtime 0.547ms~~
    - ~~After:  Registernode 50%, avg pathtime 0.650ms :(~~
  - ~~Reverse sort order of list to maximize operations on performant end~~
    - ~~Before: 0.322ms - 0.344 deterministic~~
    - ~~After: 0.300ms - 0.318 deterministic :)~~
  - ~~Build the targets list once per frame (only if needed) and save it in the team class. Retargeting nests can reference this list~~
  - ~~Make pathnodes structs to improve cache coherency~~
    - ~~Before: 0.420ms deterministic~~
    - ~~After: Worse :(~~
  - ~~Keep a cache of the weight calculation in team, updated by world events.~~
    - ~~Before: 0.300 release, 1.000 debug~~
    - ~~After:  0.450 release, 1.500 debug~~
  - ~~Path outwards from start and destination simultaneously, finishing as soon as the two sides touch~~
    - ~~Before: 0.850 release~~
    - ~~After:  0.045 release :')~~
  - ~~When a lot of minions are told to target one spot, generate a full nodegrid then get all their paths from that.~~
    - ~~Whenever a path is calculated, searth the pathqueue for paths with the same destination or source, then discard all set nodes(if source shared)/antinodes(if dest shared) and solve from there. - Probably too limiting.~~
    - ~~https://www.redblobgames.com/pathfinding/tower-defense/~~
    - ~~https://www.redblobgames.com/blog/2024-04-27-flow-field-pathfinding/~~
    - ~~https://www.roguebasin.com/index.php?title=Dijkstra_Maps_Visualized~~

- Emergency Measure: Reduce tickrate from 60 to 30. This is easy to do just by changing timescale and framerate, but results in noticeably choppier gameplay. Interpolation would probably eat into the benefits somewhat but could easily still be worth it.


----- Postponed to team bug fort 2 -----

- Tournament campaign
- Worker units
- Multitile structures
- Inventory system
- Many more bugs and towers than current
- Better fear evaluation, make bugs more scared of staying in scary places than passing through them.
- Minion vs Minion melee
- Multiple team colors via shader & rgb swizzling, or channel splitting assets at load time, or something.


Design todo:
============
- 1401 v2 content
 - Towers
  - AOE tower - Bomb mortar? Gas?
  - Slow tower - 
  - Lightning tower?
  - 
 - Minions
  - Ranged Minion that shoots over walls
  - Dragonfly
  - Centipede
  - Roach
  - Earwig
  - Fly
  - Flea
  - Worm
  - Stinkbug
  - Scorpion
  - Lightning bug
 - Utility
  - Strong door
  - Fast floor
  - Slow floor
  - Honey pot
  - Minefield
 - Active ability tower
  - Rally Minions to point
  - Frenzy Aura
  - Freeze/slow aura
  - Tower disabling EMP
  - Minion Invincibility aura
  - Repair/Rebuild building
  - Destroy Wall 
  - Spiderling egg sac mortar

- Workers?
 - Are spawned by core structure
 - Reloading turrets - Turrets run out of ammo?
 - Can walk along the top of walls?
 - Harvest worker resource, mushrooms? which sprout randomly everywhere on the map.
