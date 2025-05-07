Screen Hierarchy:
=================

Logo/Load
|
V
Main Menu
|-Campaign
| |-Editor
| |-Battle Select
|   |-Battle > Battle result screen
|-Sandbox Editor
|-Help
V-Credits
Quit


Minion State Machine
====================
states for 1401:
Waiting
Moving
AttackingBuilding

states for 1402:
Fighting
StandingGuard
ApproachingCombat


Level Rewards
=============
0
Mud Wall
Watchtower
Anthill
Hazard Sign
1
Gate
2
Machinegunner
Minefield
3
Grasshopper Glade
Rally Beacon
4
Mortar
5
Beehive
Honey Pot
6
Repair Beacon
Wooden Wall
7
Lightning Tower
8
Snail Warren
Glue Paper
9
Brood Beacon
10
Stone Wall
11
Beetle Burrow
12
Frenzy Beacon
13
Spider Nest
14
Sniper
15
Frog Pond
Vault Door



Structure List
==============
Mud Wall
Stone Wall
Honey Pot
Glue Paper
Hazard Sign
Gate
Vault Door

Watchtower
Mortar
Machinegunner
Lightning Tower
Sniper
Minefield

Rally Beacon
Repair Beacon
Frenzy Beacon
Brood Beacon

Anthill
Snail Warren
Beehive
Beetle Burrow
Grasshopper Glade
Spider Nest
Frog Pond


Playtest Notes
==============

- tell the player that nests are important
- Tell the player what rmb does in editor
- Rename "Erase" to "Sell"
- Corpses/bloodstains to indicate fear


Patchnotes
==========

1.1 - Tournament Edition

New Features & Tweaks
 - Braver Bugs! bugs are less afraid to tread where their allies died, and their fear is eased when turrets are destroyed!
 - Custom battle menu lets you set forts as player/cpu controlled
 - "Sell All" button in fort editor
 - Abilities can be used by clicking on their icon
 - Battle map is centered by default, pan has been rebound from LMB to RMB
 - Team health bars for the battle HUD
 - Minion health bars
 - Improved some structure descriptions
 - Explosions have damage falloff. Max damage is dealt up to half the total blast radius.
 - Battle end is less jarring

Balance changes
 - Wood Wall cost reduced, 65 -> 55
 - Stone Wall cost reduced, 250 -> 215
 - Gate cost reduced, 100 -> 50
 - Vault Door cost reduced, 1000 -> 500
 - Watchtower damage increased, 10 -> 15
 - Millipede Ranger HP reduced, 120 -> 95
 - Minefield cost increased, 55 -> 85
 - Repair Stratagem now also repairs adjacent walls
 - Brood Stratagem reworked. New effect: "Adjacent nests send an extra wave of bugs to the target location"
 - Frenzy Stratagem cost increased, 1000 -> 1250
 - Ant health increased, 15 -> 20
 - Snail nest cost increased, 1000 -> 1200
 - Snail health reduced, 65 -> 60
 - Beetle health increased, 50 -> 55
 - Spiderling speed increased, 45 -> 55
 - Spiderlings attack twice per second
 
Bug fixes
 - No more flickering buttons in the fort editor
 - Campaign mode no longer lets you attack a fort with no nests built
 - Bugs will no longer become fascinated by the top left corner of the world.
 - Hoppers are now scared of minefields
 - Stratagems and nests no longer block bug movement through their tiles
 - Fixed a bug where only the rally stratagem would die when killed
 - Being at Nest/Stratagem cap no longer prevents you from replacing your nests/stratagems
 - Beacon 4 now works as expected

stages to profile:

Team updates

Pathfinder

Board update

Minion update

Minion collision

Projectile update


Draw

Sort

Spritedraw

Full profiling adds about 0.200ms to pathfinding

X-Sorted physics performance:
0.75 - 1.5 ms per update

Sector Sorted Phyaics performance:
0.75 - 1.5 ms per update