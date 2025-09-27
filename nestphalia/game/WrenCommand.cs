using System.Numerics;
using Raylib_cs;
using WrenNET;
using static WrenNET.Wren;

namespace nestphalia;

public class WrenCommand
{
    private static string _initScript;
    
    // the config needs to be kept around because _vm breaks when it gets GC'd
    private WrenConfiguration _config;
    private WrenVM _vm;
    // keeps the foreign function delegates from getting GC'd
    private List<WrenForeignMethodFn> _foreigns = new List<WrenForeignMethodFn>();

    // Unsure if this needs to be VM specific. internally it's a pointer, so probably?
    private readonly WrenHandle _wrenInvokeCallCallHandle;

    static WrenCommand()
    {
        _initScript = File.ReadAllText(Resources.Dir + "/resources/api.wren");
    }
    
    public WrenCommand()
    {
        _config = new();
        wrenInitConfiguration(ref _config);
        _config.writeFn = WriteFn;
        _config.errorFn = ErrorFn;
        _config.loadModuleFn = LoadModuleFn;
        _config.bindForeignMethodFn = BindForeignMethodFn;
        
        _vm = wrenNewVM(_config);
        
        wrenInterpret(_vm, "main", _initScript);
        
        // Confusing name overlap:
        //   - Call       - the action of invoking a method
        //   - CallHandle - wren's way of storing a method signature to invoke later
        //   - call()     - the signature of the method that resumes fibers and invokes functions
        _wrenInvokeCallCallHandle = wrenMakeCallHandle(_vm, "call()");
    }

    ~WrenCommand()
    {
        wrenFreeVM(_vm);
    }
    
    private void WriteFn(WrenVM vm, string text) => GameConsole.WriteLine(text);
	
    private void ErrorFn(WrenVM vm, WrenErrorType errorType, string module, int line, string msg)
    {
        GameConsole.WriteLine($"Wren {errorType} error from {module}, line {line}: {msg}");
    }
	
    private WrenLoadModuleResult LoadModuleFn(WrenVM vm, string module)
    {
        return new WrenLoadModuleResult { source = "" };
    }

    // If passed function is not static, then it's delegate must be kept in memory somehow.
    private WrenForeignMethodFn BindForeignMethodFn(WrenVM vm, string module, string classname, bool isStatic, string signature)
    {
        WrenForeignMethodFn foreign = null;
        
        // Commands
        if (signature == "attack(_,_,_,_,_,_,_)") foreign = Attack;
        if (signature == "spawn(_,_,_,_,_,_,_)") foreign = Spawn;
        if (signature == "kill(_,_)") foreign = Kill;
        if (signature == "dialogForeign(_,_,_,_)") foreign = Dialog;
        if (signature == "build(_,_,_,_)") foreign = Build;
        if (signature == "demolish(_,_)") foreign = Demolish;
        if (signature == "win(_)") foreign = Win;
        
        // Events
        if (signature == "teamHealthBelow(_,_,_)") foreign = TeamHealthBelowEvent;
        if (signature == "timer(_,_,_)") foreign = TimerEvent;
        if (signature == "structureDestroyed(_,_,_)") foreign = StructureDestroyedEvent;
        if (signature == "battleOver(_,_)") foreign = BattleOverEvent;
        
        if (foreign == null) throw new Exception($"Tried to bind foreign with unrecognized signature: {signature}");
        _foreigns.Add(foreign);
        return foreign;
    }
    
    public void Execute(string input)
    {
        wrenInterpret(_vm, "main", input);
    }

    public void InvokeCallOnWrenHandle(WrenHandle handle)
    {
        wrenEnsureSlots(_vm, 1);
        wrenSetSlotHandle(_vm, 0, handle);
        wrenCall(_vm, _wrenInvokeCallCallHandle);
    }
    
    public void ReleaseWrenHandle(WrenHandle handle)
    {
        wrenReleaseHandle(_vm, handle);
    }

    #region Wren Foreigns
    // Wren foreigns are grouped into classes, each one gets it's own region as well.
    
    #region Cmd
    // wren: dialogForeign(mode, portrait, text, fiber)
    private void Dialog(WrenVM vm)
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
            wrenCall(vm, _wrenInvokeCallCallHandle);
            wrenReleaseHandle(vm, fiber);
        };
        
        PopupManager.Start(new DialogBox(text, resume, mode));
    }

    // Wren: attack(id, posX, posY, posZ, targetX, targetY, targetZ)
    private void Attack(WrenVM vm)
    {
        string id = wrenGetSlotString(vm, 1);
        double posX = wrenGetSlotDouble(vm, 2);
        double posY = wrenGetSlotDouble(vm, 3);
        double posZ = wrenGetSlotDouble(vm, 4);
        double targetX = wrenGetSlotDouble(vm, 5);
        double targetY = wrenGetSlotDouble(vm, 6);
        double targetZ = wrenGetSlotDouble(vm, 7);

        AttackTemplate attack = Assets.Get<AttackTemplate>(id);
        Vector3 pos = new Vector3((float)posX, (float)posY, (float)posZ);
        Vector3 target = new Vector3((float)targetX, (float)targetY, (float)targetZ);

        attack.Instantiate(target, null, pos);
    }
    
    // wren: spawn(id, team, count, x, y, targetX, targetY)
    private void Spawn(WrenVM vm)
    {
        string id = wrenGetSlotString(vm, 1);
        string team = wrenGetSlotString(vm, 2);
        int count = (int)wrenGetSlotDouble(vm, 3);
        int x = (int)wrenGetSlotDouble(vm, 4);
        int y = (int)wrenGetSlotDouble(vm, 5);
        int targetX = (int)wrenGetSlotDouble(vm, 6);
        int targetY = (int)wrenGetSlotDouble(vm, 7);
        
        if (Program.CurrentScene is not BattleScene)
        {
            GameConsole.WriteLine("spawn() error: Can't do that in this scene!");
            return;
        }
        
        if (!Assets.Exists<MinionTemplate>(id))
        {
            GameConsole.WriteLine($"spawn() error: Can't find minion {id}");
            return;
        }
        MinionTemplate mt = Assets.Get<MinionTemplate>(id);
        
        Team? t = World.GetTeam(team);
        if (t == null)
        {
            GameConsole.WriteLine($"spawn() error: Invalid team {team}");
            return;
        }
        
        for (int i = 0; i < count; i++)
        {
            Vector2 pos = World.GetTileCenter(x, y);
            if (count > 1)
            {
                pos += World.RandomUnitCircle() * (float)Math.Sqrt(mt.PhysicsRadius * count) * 2;
            }
            Minion m = mt.Instantiate(t, pos.XYZ(), new NavPath(id, t));
            m.SetTarget(new Int2D(targetX, targetY));
        }
    }
    
    // Wren signature: kill(id, team)
    private void Kill(WrenVM vm)
    {
        string id = wrenGetSlotString(vm, 1);
        string team = wrenGetSlotString(vm, 2);
        
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
    private void Build(WrenVM vm)
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
        
        if (!Assets.Exists<StructureTemplate>(structure))
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
    private void Demolish(WrenVM vm)
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

    private void Win(WrenVM vm)
    {
        string team = wrenGetSlotString(vm, 1);

        
        if (Program.CurrentScene is not BattleScene battleScene)
        {
            GameConsole.WriteLine("win() error: Can't do that in this scene!");
            return;
        }
        
        Team? t = World.GetTeam(team);
        if (t == null)
        {
            GameConsole.WriteLine($"win() error: Invalid team {team}");
            return;
        }
        
        // do winning here
        battleScene.SetWinner(t);
    }
    
    #endregion

    #region Event
    // Events register callbacks in the battlescene event registry
    
    // Wren: teamHealthBelow(team, threshold, action)
    private void TeamHealthBelowEvent(WrenVM vm)
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
        
        battleScene.AddEvent(new TeamHealthBelowEvent(t, threshold, action, this));
    }
    
    // Wren: timer(duration, recurring, action)
    private void TimerEvent(WrenVM vm)
    {
        double duration = wrenGetSlotDouble(vm, 1);
        bool recurring = wrenGetSlotBool(vm, 2);
        WrenHandle action = wrenGetSlotHandle(vm, 3);
        
        if (Program.CurrentScene is not BattleScene battleScene)
        {
            GameConsole.WriteLine("Can't do that outside of battle!");
            return;
        }
        
        battleScene.AddEvent(new TimerEvent(duration, recurring, action, this));
    }
    
    // Wren: structureDestroyed(x, y, action)
    private void StructureDestroyedEvent(WrenVM vm)
    {
        int x = (int)wrenGetSlotDouble(vm, 1);
        int y = (int)wrenGetSlotDouble(vm, 2);
        WrenHandle action = wrenGetSlotHandle(vm, 3);
        
        if (Program.CurrentScene is not BattleScene battleScene)
        {
            GameConsole.WriteLine("Can't do that outside of battle!");
            return;
        }
        
        battleScene.AddEvent(new StructureDestroyedEvent(new Int2D(x,y), action, this));
    }
    
    // wren: battleOver(team, action)
    private void BattleOverEvent(WrenVM vm)
    {
        string team = wrenGetSlotString(vm, 1);
        WrenHandle action = wrenGetSlotHandle(vm, 2);
        
        if (Program.CurrentScene is not BattleScene battleScene)
        {
            GameConsole.WriteLine("Can't do that outside of battle!");
            return;
        }
        
        battleScene.AddEndEvent(new BattleEndEvent(team, action, this));
    }
    
    #endregion
    
    #endregion
}