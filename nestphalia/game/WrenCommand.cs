using System.Reflection;
using Raylib_cs;
using WrenNET;
using static WrenNET.Wren;

namespace nestphalia;

public static class WrenCommand
{
    private static WrenConfiguration _config;
    private static WrenVM _vm;

    private static readonly WrenHandle WrenInvokeCallCallHandle;

    static WrenCommand()
    {
        _config = new();
        wrenInitConfiguration(ref _config);
        _config.writeFn = WriteFn;
        _config.errorFn = ErrorFn;
        _config.loadModuleFn = LoadModuleFn;
        _config.bindForeignMethodFn = BindForeignMethodFn;
        
        _vm = wrenNewVM(_config);
        
        string script = """
class Cmd {
    foreign static kill(team, id)
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
}
""";
        
        wrenInterpret(_vm, "main", script);
        
        // Confusing name overlap:
        //   - Call       - the action of invoking a method
        //   - CallHandle - wren's way of storing a method signature to invoke later
        //   - call()     - the signature of the method that resumes fibers and invokes functions
        WrenInvokeCallCallHandle = wrenMakeCallHandle(_vm, "call()");
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
        if (signature == "dialogForeign(_,_,_,_)") return Dialog;
        if (signature == "build(_,_,_,_)") return Build;
        if (signature == "demolish(_,_)") return Demolish;
        if (signature == "teamHealthBelow(_,_,_)") return TeamHealthBelowEvent;
        if (signature == "timer(_,_,_)") return TimerEvent;
        if (signature == "structureDestroyed(_,_,_)") return StructureDestroyedEvent;
        throw new Exception($"Tried to bind foreign with unrecognized signature: {signature}");
    }

    public static void InvokeCallOnWrenHandle(WrenHandle handle)
    {
        wrenEnsureSlots(_vm, 1);
        wrenSetSlotHandle(_vm, 0, handle);
        wrenCall(_vm, WrenInvokeCallCallHandle);
    }

    public static void ReleaseWrenHandle(WrenHandle handle)
    {
        wrenReleaseHandle(_vm, handle);
    }

    private static void TeamHealthBelowEvent(WrenVM vm)
    {
        string team = wrenGetSlotString(vm, 1);
        double threshold = wrenGetSlotDouble(vm, 2);
        WrenHandle action = wrenGetSlotHandle(vm, 3);
        
        if (Program.CurrentScene is not BattleScene battleScene)
        {
            GameConsole.WriteLine("Can't do that outside of battle!");
            return;
        }

        Team? t = World.GetTeam(team);
        if (t == null)
        {
            GameConsole.WriteLine($"teamHealthBelowEvent() error: Invalid team {team}");
            return;
        }
        
        battleScene.AddEvent(new TeamHealthBelowEvent(t, threshold, action));
    }

    // Wren: timer(duration, recurring, action)
    private static void TimerEvent(WrenVM vm)
    {
        double duration = wrenGetSlotDouble(vm, 1);
        bool recurring = wrenGetSlotBool(vm, 2);
        WrenHandle action = wrenGetSlotHandle(vm, 3);
        
        if (Program.CurrentScene is not BattleScene battleScene)
        {
            GameConsole.WriteLine("Can't do that outside of battle!");
            return;
        }
        
        battleScene.AddEvent(new TimerEvent(duration, recurring, action));
    }
    
    // Wren: structureDestroyed(x, y, action)
    public static void StructureDestroyedEvent(WrenVM vm)
    {
        int x = (int)wrenGetSlotDouble(vm, 1);
        int y = (int)wrenGetSlotDouble(vm, 2);
        WrenHandle action = wrenGetSlotHandle(vm, 3);
        
        if (Program.CurrentScene is not BattleScene battleScene)
        {
            GameConsole.WriteLine("Can't do that outside of battle!");
            return;
        }
        
        battleScene.AddEvent(new StructureDestroyedEvent(new Int2D(x,y), action));
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
        WrenHandle fiber = wrenGetSlotHandle(vm, 4);
        
        Texture2D portraitTex = Resources.GetTextureByName(portrait);
        
        Action resume = () =>
        {
            wrenEnsureSlots(vm, 1);
            wrenSetSlotHandle(vm, 0, fiber);
            wrenCall(vm, WrenInvokeCallCallHandle);
            wrenReleaseHandle(vm, fiber);
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
        
        if (Assets.Exists<StructureTemplate>(structure))
        {
            GameConsole.WriteLine($"Build() error: Can't find structure {structure}");
            return;
        }
        StructureTemplate st = Assets.Get<StructureTemplate>(structure);

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