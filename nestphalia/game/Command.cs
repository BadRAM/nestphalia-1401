using System.Reflection;
using Raylib_cs;

namespace nestphalia;

public static class Command
{
    private static Dictionary<string, Func<string, string>> _commands;

    static Command()
    {
        _commands = new Dictionary<string, Func<string, string>>();
        foreach (MethodInfo methodInfo in typeof(Command).GetMethods())
        {
            IsCommand? attr = methodInfo.GetCustomAttribute<IsCommand>();
            if (attr == null) continue;
            _commands.Add(attr.Name, (Func<string, string>)methodInfo.CreateDelegate(typeof(Func<string, string>)));
        }
    }
    
    public static string Execute(string input)
    {
        List<string> words = new List<string>(input.Split(" "));
        if (words.Count == 0) return "";
        string command = words[0].ToUpper();
        words.RemoveAt(0);

        try
        {
            if (_commands.ContainsKey(command)) return _commands[command].Invoke(string.Join(" ", words));
        }
        catch (Exception e)
        {
            GameConsole.WriteLine(e.ToString());
        }
        
        return $"Command not recognized";
    }
    
    [AttributeUsage(AttributeTargets.Method)]
    private class IsCommand : Attribute
    {
        public string Name;
        public string Description;
        public string Help;

        public IsCommand(string name, string description, string help)
        {
            Name = name;
            Description = description;
            Help = help;
        }
    }

    [IsCommand("HELP", "List commands, or display help text for a command",
        "Usage: HELP [command]\n But you already knew that, didn't you? ;)")]
    public static string Help(string args)
    {
        string command = args.Split(" ")[0].ToUpper();
        if (_commands.ContainsKey(command))
        {
            IsCommand attr = _commands[command].Method.GetCustomAttribute<IsCommand>();
            GameConsole.WriteLine($"{attr.Name} - {attr.Description}\n{attr.Help}");
            return "";
        }
        else
        {
            if (command != "") GameConsole.WriteLine($"Couldn't find command: {command}");
            GameConsole.WriteLine($"{_commands.Count} Commands Available:");
            foreach (MethodInfo methodInfo in typeof(Command).GetMethods())
            {
                IsCommand? attr = methodInfo.GetCustomAttribute<IsCommand>();
                if (attr == null) continue;
                GameConsole.WriteLine($"{attr.Name} - {attr.Description}");
            }
            return "";
        }
    }
    
    [IsCommand("DIALOG", "Display a dialog box", 
        "Usage: DIALOG [mode] [texture1] [texture2] Dialog Text...\n" +
        " Valid [mode]s: NONE, LEFT, RIGHT, BOTH")]
    public static string Dialog(string args)
    {
        List<string> words = new List<string>(args.Split(" "));
        if (Enum.TryParse(words[0], true, out DialogBox.Mode mode))
        {
            words.RemoveAt(0);
        }
        else
        {
            mode = DialogBox.Mode.None;
        }
        Texture2D? portraitOne = null;
        if (mode != DialogBox.Mode.None)
        {
            portraitOne = Resources.GetTextureByName(words[0]);
            words.RemoveAt(0);
        }
        Texture2D? portraitTwo = null;
        if (mode == DialogBox.Mode.Both)
        {
            portraitTwo = Resources.GetTextureByName(words[0]);
            words.RemoveAt(0);
        }
        DialogBox box = new DialogBox(string.Join(" ", words), mode, portraitOne, portraitTwo);
        if (Program.CurrentScene is BattleScene bs)
        {
            bs.AddDialog(box);
            return $"Dialog Queued Successfully";
        }
        return "Dialog failed, unsupported by scene";
    }
    
    [IsCommand("KILL", "Kill minions", 
        "Usage: KILL [minionID]\n" +
        " [minionID] if not included, kills all minions")]
    public static string Kill(string args)
    {
        string id = args.Split(" ")[0].ToLower();
        int count = 0;
        for (int index = 0; index < World.Minions.Count; index++)
        {
            Minion minion = World.Minions[index];
            if (id == "" || id == minion.Template.ID.ToLower())
            {
                minion.Health = 0;
                minion.Die();
                count++;
            }
        }
        return $"Killed {count} Minions";
    }

    [IsCommand("CENSUS", "List Minion populations", 
        "Usage: CENSUS [side]\n" +
        " [side] can be Left, Right or Both. Default: Both ")]
    public static string Census(string args)
    {
        string arg = args.Split(" ")[0].ToUpper();
        Team? team = null;
        if (arg == "LEFT") team = World.LeftTeam;
        if (arg == "RIGHT") team = World.RightTeam;
        Dictionary<string, int> census = new Dictionary<string, int>();

        foreach (Minion minion in World.Minions)
        {
            if (team == null || minion.Team == team)
            {
                if (!census.ContainsKey(minion.Template.ID)) census.Add(minion.Template.ID, 0);
                census[minion.Template.ID]++;
            }
        }

        foreach (var count in census)
        {
            // GameConsole.WriteLine($"{count.Key.PadRight(20)} - {count.Value}"); // Fancy but breaks if not fixed width font
            GameConsole.WriteLine($"{count.Key}: {count.Value}");
        }
        
        return "";
    }
    
    [IsCommand("DEMOLISH", "Destroy building at position", "")]
    public static string Demolish(string args)
    {
        List<string> words = new List<string>(args.Split(" "));
        int x = words[0] == "~" ? World.GetMouseTilePos().X : int.Parse(words[0]);
        int y = words[0] == "~" ? World.GetMouseTilePos().Y : int.Parse(words[1]);
        
        World.GetTile(x,y)?.Destroy();
        
        return "";
    }

    [IsCommand("BUILD", "", "")]
    public static string Build(string args)
    {
        return "";
    }
}