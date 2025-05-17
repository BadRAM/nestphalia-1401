 == This file is for BadRAM's personal organization. ==

Programming todo:
=================

----- V2 -----

- Game intro cutscene
- Random damage variation
- Redesign the campaign forts
- Nice campaign screen with a map and paths appearing every time you unlock a new level. Nonlinear campaign?
- Nice title screen
- 2-3 more bugs & towers
  - Ranged attack bug
  - Another flyer. Ladybug? Flies until attacked.
  - Snail rework
  - Centipede that gets longer and shorter
- ~~Random damage variation~~
- ~~Make battles able to be deterministic if desired~~
- Minion state machine
- Refactor Minion.Hurt() to not need a damagesource
- ~~Campaign rebalance, use 'funding' instead of direct cash stores, or Pay to build the fort every mission~~
- ~~Allow multiple campaign designs~~
- Help text/tutorial
- Orphaned sapper beetles will seek another burrow
- Limit total spiderlings a spider can spawn
- ~~try compiling for web~~
- Make nests solid again
- Help text / Tutorial
- Fix minion collision with wall corners
- Minion melee attack animation
- Minion state machine


----- Premature Optimizations -----

- Codebase Refactoring
  - Get rid of as many public fields as possible
    - ~~Battlescene should accept callback function to report victory~~

- Game Logic optimizations
  - ~~profile tile entities separately by ID~~

- Physics optimizations
  - ~~Sector based minion culling/lookup~~
    - ~~No significant difference~~
    - ~~11000 minions slows the game to 50% speed on my laptop.~~
  - ~~Multi Threading~~
    - ~~multithreading collision made the game slower~~
    - ~~Giving pathfinding a background task while collision detection happens on the main thread seems to be helping a little bit.~~

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
  - Give each team it's own pathfinder and pathqueue (this will prevent one team from jamming the other's pathing by filling the pathqueue)

- Emergency Measure: Reduce tickrate from 60 to 30. This is easy to do just by changing timescale and framerate, but results in noticeably choppier gameplay. Interpolation would probably eat into the benefits somewhat but could easily still be worth it.


----- Postponed to team bug fort 2 -----

- 3d engine
- Tournament campaign
- Worker units
- Multitile structures
- Inventory system
- Many more bugs and towers than current
- Better fear evaluation, make bugs more scared of staying in scary places than passing through them.
- Assets from json
- Minion vs Minion melee
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


