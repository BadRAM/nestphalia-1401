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
- Heavier bugs push smaller bugs out of the way
- ~~Sandbox editor is inside of custom battle menu~~
- ~~Custom Battle Folders~~
- fort10 doesn't save correctly
- new graffiti
- Editor path preview
- Some linux machines can't run the game.
    DIINotFoundException: Unable to load shared library
    ' raylib' or one of its dependencies. In order to help diagnose loading problems,
    consider setting the LD DEBUG environment variable: libraylib: cannot open shar
    ed object file: No such file or directory
    at ZeroElectric. Vinculum. Raylib. SetWindoHinSize(Int32 width, Int32 height)
    at nestphalia.Program.Main() in C:\Users\Luke\Workspace\RiderProjects\2-fort-
    cs\nestphalia\Program.cs:line 27
    Aborted (core dumped)
- ESC quits all menus
- Separate music and sfx mutes

----- V2 -----

- Refactor Minion.Hurt() to not need a damagesource
- Pathfinding optimizations: 
  - as soon as a node is set in the targeted half of the battlefield, purge all nodes and dont' allow any new ones to be created in the untargeted half
  - Binary search to find consider queue insertion point
- Campaign rebalance, use 'funding' instead of direct cash stores.
- Help text/tutorial
- Game load cutscene
- Fancy title text
- Main menu art
- Campaign screen Map
- Orphaned sapper beetles will seek another burrow
- Limit total spiderlings a spider can spawn
- Switch back to raylib-cs and try compiling for web


----- Postponed to team bug fort 2 -----
- Fix minion collision with wall corners
- Better fear evaluation, make bugs more scared of staying in scary places than passing through them.
- Minion melee attack animation
- Minion state machine
- Assets from json
- Minion vs Minion melee
- Multitile structures
- Active ability towers?
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

