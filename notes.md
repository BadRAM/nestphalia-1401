Lore
====
The big questions:
Why are the bugs fighting?
How much tech/magic do the bugs have?
What aesthetics/art/architecture is popular in bug society?

You are an ant queen, spawn of the Empress of all ants, but with little right to the throne. While managing a small colony in a distant corner of the region, a friend from your youth in the capitol bursts in to inform you that the Empress has been killed and her throne taken by a pretender king! You are bid to return and free the empire from his dominion, but between you and the imperial core are three other realms that won't allow you to march your army through unhindered, and you'll need more supplies and technologies if you're to stand a chance against the imperial core...

Cicadas keep the history of the world, and every 17 years they emerge and recount the history of the preceding cycles before dying and all of it being lost.


Campaign Plan
=============
Your origins:
 - Ant queen, 300th in line to rule the ant empire, currently managing a small colony in a distant corner of the region.
The world:
Four bug kingdoms, Blue, Yellow, Green, and Red, arranged like
 R 
Y G
 B 
Red: Ant empire. Hard forts that use everything in the game. Final Boss: Frog, the pretender king
Yellow: Bee realm, expect many flyers. rewards are strong against ground. Final Boss: Queen bee, the self important
Green: Beetle kingdom, rewards are strong against air. Final Boss: Hercules, the hero of legend
Blue: Bug republic (Hansneatic league?), assorted shitter bugs, rewards are basic kit. Final Boss: Snederick of Snaxony, Leader-Elect of the autonomous bug republic (big snail)
You start in the bottom corner of blue. Forts have paths to neighboring forts that you unlock for defeating them, but paths out of a country don't work until you defeat the capitol, so you must defeat either Y or G before you can attack R, and cannot attack either before clearing B.


Tutorial Plan:
==============
 - Tell the player to go to battle with a premade fort that is guaranteed to lose, so that the player understands how a battle works before designing their first fort.
 - Provide a Bugspedia (Tome of bug lore? Nestpedia?) with explanations of more complex subjects like hate, fear, and wave scaling, as well as stat cards for bugs


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
|-Custom battle
| |-Sandbox Editor
|-Help
V-Credits
Quit


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


Playtest Notes
==============

- tell the player that nests are important
- Tell the player what rmb does in editor
- Corpses/bloodstains to indicate fear

New Level Format Requirements
=============================

- ID
- Name
- Description
- Location on world map
- Connected level IDs
- Reward amount
- World Grid size
- Background tiles
- Structures for entire world
- Start and finish dialogue
- Enemy color


- Custom editor

Event Triggers
  ----------
- after an amount of time
- when a hitpoint threshold is crossed
- when a specific set of structures are destroyed
- at the start of the battle
- just before the battle end screen is shown
- when a special minion passes a HP threshold