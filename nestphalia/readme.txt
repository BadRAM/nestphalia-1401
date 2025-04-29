Nestphalia 1401

To start the game:
 - windows: Run Nestphalia.exe
 - linux: open this folder in terminal, run 'chmod 777 ./nestphalia', then run './nestphalia'
 
All game progress is saved in campaign.sav, if you want to back up, transfer, or reset your progress, just copy, paste, or delete campaign.sav

Your custom battle forts are saved in /forts/. Renaming files and creating folders is fully supported and expected.

Version History:

==== 1.2.0 - Champion's Edition - 2025-04-28 ====
Highlighted Changes:
 - All forts used in the first grand tournament of bugs have been included
 - The tournament's participants have left graffiti on menu screens
 - The custom battle menu now supports folders
 - The sandbox editor is now accessed from inside the custom battle menu
 - The campaign now lets you save multiple fort designs
 - You must now pay the full cost of your fort design every time you start a battle. Battle rewards have increased accordingly
 - The editor now has a path preview tool, to help understand bug pathfinding
 - SFX and Music can be muted separately
 - Optimized pathfinding and collision detection, performance in large battles improved significantly
Other changes:
 - Bugs no longer get stuck in corners if they are pushed backwards by other bugs
 - Repaired stratagems work as normal
 - Hoppers no longer hop to the exact center of their target tile
 - The Sell All button now allows you to cancel
 - Fixed a bug where beetles would sometimes throw a lot of bombs instead of just one
 - Bugs no longer walk directly towards their target while waiting for a path
 - Larger bugs aren't as easy for small bugs to push around
 - Hoppers are aware of minefields
 - Fixed a bug that prevented the game from starting on most linux distributions
 - ESC exits all menus
 - The editor has a new background image
 - Fixed a bug where spiderlings spawned from the death of a spider would be inactive for a while
 - Migrated to a different raylib c# binding

---- 1.1.2 - 2025-03-02 ----
 - Fixed a bug that caused bugs to pathfind in circles

---- 1.1.1 - 2025-03-02 ----
 - Fixed a bug where victory would be awarded to the wrong team.

==== 1.1 - Tournament Edition - 2025-03-02 ====
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