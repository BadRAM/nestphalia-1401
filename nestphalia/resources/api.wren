class Cmd {
    foreign static spawn(id, team, count, x, y, targetX, targetY)
    foreign static kill(id, team)
    foreign static build(structure, team, x, y)
    foreign static demolish(x, y)
    foreign static dialogForeign(mode, portrait, text, fiber)
    static dialog(text) {
        dialogForeign(0, "", text, Fiber.current)
        Fiber.yield()
    }
    static dialogL(portrait, text) {
        dialogForeign(1, portrait, text, Fiber.current)
        Fiber.yield()
    }
    static dialogR(portrait, text) {
        dialogForeign(2, portrait, text, Fiber.current)
        Fiber.yield()
    }
}

class Event {
    foreign static teamHealthBelow(team, threshold, action)
    foreign static timer(duration, recurring, action)
    foreign static structureDestroyed(x, y, action)
    foreign static battleOver(winner, action)
}