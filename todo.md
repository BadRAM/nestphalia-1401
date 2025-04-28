 == This file is for BadRAM's personal organization. ==

Programming todo:
=================

----- Champion's Edition -----

- ~~Bugs pushed too far from their next path tile will repath~~
- ~~Re-register repaired stratagems~~
- ~~Hopper target pos randomization~~
- ~~Sell all cancel button~~
- ~~Beetles throw a lot of bombs sometimes~~
- ~~Bugs walk towards target while unpathed, then go back to their path origin~~
- ~~Heavier bugs push smaller bugs out of the way~~
- ~~Sandbox editor is inside of custom battle menu~~
- ~~Custom Battle Folders~~
- ~~fort10 doesn't save correctly - Could not reproduce~~
- ~~saving to new file multiple times doesnt' work~~
- ~~Hoppers are aware of enemy mines~~
- ~~Editor path preview~~
- ~~new graffiti~~
- ~~Paulby shoutout~~
- ~~Some linux machines can't run the game.~~
- ~~ESC quits all menus~~
- ~~Separate music and sfx mutes~~
- ~~Editor background texture~~
- ~~Bug: Spiderlings from death of bigspider just sit there for a while.~~
- ~~Selling buildings doesn't return money properly~~
- ~~Exit without saving doesn't refund bug dollars~~
- ~~Fort Editor text overflow~~
- ~~Text entry element~~
- ~~'Save fort as' function~~
- ~~Campaign fort design selector~~
- ~~Rebalance campaign rewards~~


Decision point: Do I push towards 1401 v2, or start on 1402?
---- V2: ----
- Random damage variation
- Campaign rebalance to use funding instead of direct cash.
- Allow multiple campaign designs
- Redesign the campaign forts
- Nice campaign screen with a map and paths appearing every time you unlock a new level. Nonlinear campaign?
- Nice title screen
- 2-3 more bugs & towers
 - Ranged attack bug
 - Another flyer. Ladybug? Flies until attacked.
 - Snail rework
 - Centipede that gets longer and shorter

---- 1402 ----
- 3d engine
- Tournament campaign
- Worker units
- Multitile structures
- Inventory system
- Many more bugs and towers than current


----- V2 -----

- ~~Random damage variation~~
- ~~Make battles able to be deterministic if desired~~
- Minion state machine
- Refactor Minion.Hurt() to not need a damagesource
- Pathfinding optimizations:
  - ~~as soon as a node is set in the targeted half of the battlefield, purge all nodes and dont' allow any new ones to be created in the untargeted half~~
    - ~~This feels too complex, limiting, and like it could cause difficult bugs. A more generalized chunking method would be better, but only if the game grows and pathing performance becomes a problem again~~
  - ~~Binary search to find consider queue insertion point~~
    - ~~Before: Registernode 30%, avg pathtime 0.547ms~~
    - ~~After:  Registernode 50%, avg pathtime 0.650ms :(~~
  - ~~Reverse sort order of list to maximize operations on performant end~~
    - ~~Before: 0.322ms - 0.344 deterministic~~
    - ~~After: 0.300ms - 0.318 deterministic :)~~
- Campaign rebalance, use 'funding' instead of direct cash stores, or Pay to build the fort every mission
- ~~Allow multiple campaign designs~~
- Help text/tutorial
- Game load cutscene
- Fancy title text
- Main menu art
- Campaign screen Map
- Orphaned sapper beetles will seek another burrow
- Limit total spiderlings a spider can spawn
- try compiling for web
- Make nests solid again
- Help text / Tutorial


----- Postponed to team bug fort 2 -----

- Fix minion collision with wall corners
- Better fear evaluation, make bugs more scared of staying in scary places than passing through them.
- Minion melee attack animation
- Minion state machine
- Assets from json
- Minion vs Minion melee
- Multitile structures
- Post battle screen


Design todo:
============
- Final bug fort 1 content update
 - Towers
  - AOE tower - Bomb mortar? Gas?
  - Slow tower - 
  - Lightning tower?
  - 
 - Minions
  - Ranged Minion that shoots over walls
  - Grasshopper
  - Frog
  - Dragonfly
  - Centipede
  - Roach
  - Earwig
  - Spider - Produces spiderlings while alive, turns into many spiderlings when killed
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


Project management todo:
========================


