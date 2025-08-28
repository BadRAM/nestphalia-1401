using System.Reflection;
using Raylib_cs;
using WrenNET;
using static WrenNET.Wren;

namespace nestphalia;

public static class WrenCommand
{
    private static WrenConfiguration _config;
    private static WrenVM _vm;

    private static WrenHandle _threadResumeCallHandle;

    static WrenCommand()
    {
        GameConsole.WriteLine("Initializing Wren VM...");
        
        _config = new();
        wrenInitConfiguration(ref _config);
        _config.writeFn = WriteFn;
        _config.errorFn = ErrorFn;
        _config.loadModuleFn = LoadModuleFn;
        _config.bindForeignMethodFn = BindForeignMethodFn;
        
        _vm = wrenNewVM(_config);
        
        string script = """
var MainFiber = Fiber.current

class Command {
    foreign static kill(team, id)
    foreign static build(structure, team, x, y)
    foreign static demolish(x, y)
    foreign static dialogForeign(mode, portrait, text)
    static dialog(text) {
        MainFiber = Fiber.current
        dialogForeign(0, "", text)
        Fiber.yield()
    }
    static dialogL(portrait, text) {
        MainFiber = Fiber.current
        dialogForeign(1, portrait, text)
        Fiber.yield()
    }
    static dialogR(portrait, text) {
        MainFiber = Fiber.current
        dialogForeign(2, portrait, text)
        Fiber.yield()
    }
}
""";
        
        wrenInterpret(_vm, "main", script);
        
        GameConsole.WriteLine("Wren VM init complete!");

        // Confusing name overlap:
        //   - Call       - the action of invoking a method
        //   - CallHandle - wren's way of storing a method signature to invoke later
        //   - call()     - is the signature of the method that resumes fibers
        _threadResumeCallHandle = wrenMakeCallHandle(_vm, "call()");
    }
    
    public static void WriteFn(WrenVM vm, string text) => GameConsole.WriteLine(text);
	
    public static void ErrorFn(WrenVM vm, WrenErrorType errorType, string module, int line, string msg)
    {
        GameConsole.WriteLine($"Wren {errorType} error from {module}, line {line}: {msg}");
    }
	
    public static WrenLoadModuleResult LoadModuleFn(WrenVM vm, string module)
    {
        return new WrenLoadModuleResult { source = "" };
    }

    public static WrenForeignMethodFn BindForeignMethodFn(WrenVM vm, string module, string classname, bool isStatic, string signature)
    {
        if (signature == "kill(_,_)") return Kill;
        if (signature == "dialogForeign(_,_,_)") return Dialog;
        if (signature == "build(_,_,_,_)") return Build;
        if (signature == "demolish(_,_)") return Demolish;
        return null;
    }
    
    public static void Execute(string input)
    {
        wrenInterpret(_vm, "main", input);
    }
    
    public static void Dialog(WrenVM vm)
    {
        DialogBox.Mode mode = (DialogBox.Mode)wrenGetSlotDouble(vm, 1);
        string portrait = wrenGetSlotString(vm, 2);
        string text = wrenGetSlotString(vm, 3);
        
        Texture2D portraitTex = Resources.GetTextureByName(portrait);
        
        Action resume = () =>
        {
            wrenEnsureSlots(vm, 1);
            wrenGetVariable(vm, "main", "MainFiber", 0);
            wrenCall(vm, _threadResumeCallHandle);
        };
        
        PopupManager.Start(new DialogBox(text, resume, mode));
    }
    
    // Wren signature: kill(team,id)
    public static void Kill(WrenVM vm)
    {
        string team = wrenGetSlotString(vm, 1);
        string id = wrenGetSlotString(vm, 2);
        
        if (Program.CurrentScene is not BattleScene)
        {
            GameConsole.WriteLine("Can't do that in this scene!");
            return;
        }

        Team? t = World.GetTeam(team);
        
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

    // Wren signature: build(structure, team, x, y)
    public static void Build(WrenVM vm)
    {
        string structure = wrenGetSlotString(vm, 1);
        string team = wrenGetSlotString(vm, 2);
        int x = (int)wrenGetSlotDouble(vm, 3);
        int y = (int)wrenGetSlotDouble(vm, 4);
        
        if (Program.CurrentScene is not BattleScene)
        {
            GameConsole.WriteLine("Build() error: Can't do that in this scene!");
            return;
        }

        StructureTemplate? st = Assets.GetStructureByID(structure);
        if (st == null)
        {
            GameConsole.WriteLine($"Build() error: Can't find structure {structure}");
            return;
        }

        Team? t = World.GetTeam(team);
        if (t == null)
        {
            GameConsole.WriteLine($"Build() error: Invalid team {team}");
            return;
        }
        
        World.SetTile(st, t, x, y);
    }

    // Wren signature: destroy(x, y)
    public static void Demolish(WrenVM vm)
    {
        int x = (int)wrenGetSlotDouble(vm, 1);
        int y = (int)wrenGetSlotDouble(vm, 2);
        
        if (Program.CurrentScene is not BattleScene)
        {
            GameConsole.WriteLine("Destroy() error: Can't do that in this scene!");
            return;
        }
        
        World.DestroyTile(x, y);
    }
}