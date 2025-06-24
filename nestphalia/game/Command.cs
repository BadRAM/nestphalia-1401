using System.Reflection;
using Raylib_cs;
using WrenSharp;

namespace nestphalia;

public class WrenBufferOutput : IWrenWriteOutput, IWrenErrorOutput
{
    private string buffer;
    
    public void OutputWrite(WrenVM vm, string text)
    {
        buffer += text;
    }

    public void OutputError(WrenVM vm, WrenErrorType errorType, string moduleName, int lineNumber, string message)
    {
        switch (errorType)
        {
            case WrenErrorType.Compile:
                buffer += ($"Wren compile error in {moduleName}:{lineNumber} : {message}") + "\n";
                break;

            case WrenErrorType.StackTrace:
                buffer += ($"at {message} in {moduleName}:{lineNumber}") + "\n";
                break;

            case WrenErrorType.Runtime:
                buffer += (string.IsNullOrEmpty(moduleName)
                    ? $"Wren error: {message}"
                    : $"Wren error in {moduleName}: {message}") + "\n";
                break;
        }
    }

    public string GetBuffer()
    {
        string ret = buffer;
        buffer = "";
        return ret;
    }
}

public static class Command
{
    // private static Dictionary<string, Func<string, string>> _commands;
    private static WrenSharpVM _vm;
    private static WrenBufferOutput _output = new WrenBufferOutput();

    static Command()
    {
        // _commands = new Dictionary<string, Func<string, string>>();
        // foreach (MethodInfo methodInfo in typeof(Command).GetMethods())
        // {
        //     IsCommand? attr = methodInfo.GetCustomAttribute<IsCommand>();
        //     if (attr == null) continue;
        //     _commands.Add(attr.Name, (Func<string, string>)methodInfo.CreateDelegate(typeof(Func<string, string>)));
        // }

        _vm = new WrenSharpVM(new WrenVMConfiguration()
        {
            LogErrors = true,
            ErrorOutput = _output,
            WriteOutput = _output,
        });

        string script = @"
class Command {
    foreign static kill(team, id)
    foreign static dialog(boxMode, portrait1, portrait2, text)
}";

        var cm = _vm.Foreign("main", "Command");
        cm.Static("kill(_,_)", ctx => Kill(ctx.GetArgString(0), ctx.GetArgString(1)));
        cm.Static("dialog(_,_,_,_)", ctx => Dialog(ctx.GetArgString(0), ctx.GetArgString(1), ctx.GetArgString(2), ctx.GetArgString(3)));
        
        _vm.Interpret("main", script);
    }
    
    public static string Execute(string input)
    {
        _vm.Interpret(
            module: "main",
            source: input, 
            throwOnFailure: false);

        return _output.GetBuffer();
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
    
    public static string Dialog(string boxMode, string portrait1, string portrait2, string text)
    {
        if (Program.CurrentScene is not BattleScene)
        {
            return "Can't show dialog in this scene!";
        }
        
        if (!Enum.TryParse(boxMode, true, out DialogBox.Mode mode))
        {
            mode = DialogBox.Mode.None;
        }
        Texture2D? portraitOne = null;
        if (mode != DialogBox.Mode.None)
        {
            portraitOne = Resources.GetTextureByName(portrait1);
        }
        Texture2D? portraitTwo = null;
        if (mode == DialogBox.Mode.Both)
        {
            portraitTwo = Resources.GetTextureByName(portrait2);
        }
        DialogBox box = new DialogBox(text, mode, portraitOne, portraitTwo);
        if (Program.CurrentScene is BattleScene bs)
        {
            bs.AddDialog(box);
            return $"Dialog Queued Successfully";
        }
        return "Dialog failed, unsupported by scene";
    }

    
    public static void Kill(string team, string id)
    {
        if (Program.CurrentScene is not BattleScene)
        {
            GameConsole.WriteLine("Can't do that in this scene!");
            return;
        }

        Team? t = null;
        if (team.ToLower() == "left") t = World.LeftTeam;
        if (team.ToLower() == "right") t = World.RightTeam;
        int count = 0;
        for (int index = 0; index < World.Minions.Count; index++)
        {
            Minion minion = World.Minions[index];
            if ((id == "" || id == minion.Template.ID.ToLower()) && (t == null || t == minion.Team))
            {
                minion.Health = 0;
                minion.Die();
                count++;
            }
        }
        
        GameConsole.WriteLine($"Killed {count} Minions");
    }
    
    // [IsCommand("KILL", "Kill minions", 
    //     "Usage: KILL [minionID] [team]\n" +
    //     " [minionID] if not included, kills all minions")]
    // public static string Kill(string args)
    // {
    //     if (Program.CurrentScene is not BattleScene)
    //     {
    //         return "Can't do that in this scene!";
    //     }
    //     
    //     string id = args.Split(" ")[0].ToLower();
    //     int count = 0;
    //     for (int index = 0; index < World.Minions.Count; index++)
    //     {
    //         Minion minion = World.Minions[index];
    //         if (id == "" || id == minion.Template.ID.ToLower())
    //         {
    //             minion.Health = 0;
    //             minion.Die();
    //             count++;
    //         }
    //     }
    //     return $"Killed {count} Minions";
    // }

    [IsCommand("CENSUS", "List Minion populations", 
        "Usage: CENSUS [side]\n" +
        " [side] can be Left, Right or Both. Default: Both ")]
    public static string Census(string args)
    {
        if (Program.CurrentScene is not BattleScene)
        {
            return "Can't do that in this scene!";
        }
        
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
    
    [IsCommand("DEMOLISH", "Destroy structure(s)", 
        "Usage: DEMOLISH [x] [y] [width] [height]\n" +
        " [x], [y] can be supplied \".\" to use mouse position\n" +
        " [width], [height] default to 1")]
    public static string Demolish(string args)
    {
        if (Program.CurrentScene is not BattleScene)
        {
            return "Can't do that in this scene!";
        }
        
        List<string> words = new List<string>(args.Split(" "));
        int startX = words[0] == "." ? World.GetMouseTilePos().X : int.Parse(words[0]);
        int startY = words[1] == "." ? World.GetMouseTilePos().Y : int.Parse(words[1]);
        int w = 1;
        if (words.Count >= 3) int.TryParse(words[2], out w);
        int h = 1;
        if (words.Count >= 4) int.TryParse(words[3], out h);

        for (int x = startX; x < startX+w; x++)
        for (int y = startY; y < startY+h; y++)
        {
            World.GetTile(x,y)?.Destroy();
        }
        
        return "";
    }

    [IsCommand("BUILD", "Create structure(s)", 
        "Usage: BUILD [structure_id] [team] [x] [y] [width] [height]\n" +
        " [team] must be LEFT or RIGHT\n" +
        " [x], [y] can be supplied \".\" to use mouse position\n" +
        " [width], [height] default to 1")]
    public static string Build(string args)
    {
        if (Program.CurrentScene is not BattleScene)
        {
            return "Can't do that in this scene!";
        }
        
        List<string> words = new List<string>(args.Split(" "));
        StructureTemplate structure = Assets.GetStructureByID(words[0]) ?? throw new Exception("Invalid structure ID!");
        Team team = words[1].ToLower() == "left" ? World.LeftTeam : World.RightTeam;
        int startX = words[2] == "." ? World.GetMouseTilePos().X : int.Parse(words[2]);
        int startY = words[3] == "." ? World.GetMouseTilePos().Y : int.Parse(words[3]);
        int w = 1;
        if (words.Count >= 5) int.TryParse(words[4], out w);
        int h = 1;
        if (words.Count >= 6) int.TryParse(words[5], out h);

        for (int x = startX; x < startX+w; x++)
        for (int y = startY; y < startY+h; y++)
        {
            World.SetTile(structure, team, x, y);
        }
        
        return "";
    }
}