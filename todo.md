 == This file is for BadRAM's personal organization. ==

Programming todo:
=================


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
  - note: published builds seem to run about 2x faster than debug mode in IDE
  - ~~as soon as a node is set in the targeted half of the battlefield, purge all nodes and dont' allow any new ones to be created in the untargeted half~~
    - ~~This feels too complex, limiting, and like it could cause difficult bugs. A more generalized chunking method would be better, but only if the game grows and pathing performance becomes a problem again~~
  - ~~Binary search to find consider queue insertion point~~
    - ~~Before: Registernode 30%, avg pathtime 0.547ms~~
    - ~~After:  Registernode 50%, avg pathtime 0.650ms :(~~
  - ~~Reverse sort order of list to maximize operations on performant end~~
    - ~~Before: 0.322ms - 0.344 deterministic~~
    - ~~After: 0.300ms - 0.318 deterministic :)~~
  - Build the targets list once per frame (only if needed) and save it in the team class. Retargeting nests can reference this list
  - Make pathnodes structs to improve cache coherency
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


