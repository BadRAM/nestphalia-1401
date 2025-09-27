class Cmd {
    // ======== Internal Functions ========
    // these need to be called with a Fiber.yield or other consideration.
    // don't use them unless you know what you're doing.
    foreign static dialogForeign(mode, portrait, text, fiber)

    // ======== API Functions ========
    // All functions are static. Example usage:
    // Cmd.dialog("Hello World")

    // Creates an attack (projectile, lightning, mortar shell, etc.)
    foreign static attack(id, posX, posY, posZ, targetX, targetY, targetZ)
    
    // Spawns a minion
    foreign static spawn(id, team, count, x, y, targetX, targetY)

    // kills all minions matching the arguments provided. blank string matches all
    foreign static kill(id, team)

    // Places a structure on a tile. Replaces whatever is there already
    foreign static build(id, team, x, y) 

    // Destroys structure at x, y, still leaves repairable rubble
    foreign static demolish(x, y)

    // Selected team wins immediately
    foreign static win(team)

    // Show a dialog box. Rich text formatting coming soon.
    static dialog(text) {
        dialogForeign(0, "", text, Fiber.current)
        Fiber.yield()
    }
}



class Event {
    // ======== Events ========
    // Event functions register a callback for a certain condition, which usually only triggers once.
    // action can be a Function created with Fn.new {} or a static method from a class or regular method from an object.
    //
    // Example usage:
    // Event.teamHealthBelow("right", 0.5, Fn.new {
    //   Cmd.dialog("Noooo! It's all falling apart!")
    //   Cmd.dialog("You won't get away with this!") 
    // })
    
    // Triggers when the given team's health first drops below the threshold. threshold is a 0.0-1.0 percentage.
    foreign static teamHealthBelow(team, threshold, action)

    // Triggers [duration] seconds after the timer is created. 
    // If [recurring] is true, it will reset and trigger every [duration] seconds
    // Timers can be registered by other event actions and will properly count from the time of creation.
    foreign static timer(duration, recurring, action)

    // Triggers when the structure on tile x,y is destroyed.
    foreign static structureDestroyed(x, y, action)

    // Triggers as the battle ends. 
    // Currently, dialog boxes called here do not suppress the battle end screen, but this is planned to be fixed.
    foreign static battleOver(winner, action)
}