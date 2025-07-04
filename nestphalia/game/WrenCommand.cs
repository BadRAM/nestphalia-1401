using System.Reflection;
using Raylib_cs;
using WrenSharp;

namespace nestphalia;

public static class WrenCommand
{
    // private static Dictionary<string, Func<string, string>> _commands;
    private static WrenSharpVM _vm;
    private static WrenBufferOutput _output = new WrenBufferOutput();

    static WrenCommand()
    {
        _vm = new WrenSharpVM(new WrenVMConfiguration()
        {
            LogErrors = true,
            ErrorOutput = _output,
            WriteOutput = _output,
        });
        
        string script = """
var MainFiber = Fiber.current

class Command {
    foreign static kill(team, id)
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
        
        var cm = _vm.Foreign("main", "Command");
        cm.Static("kill(_,_)", ctx => Kill(ctx.GetArgString(0), ctx.GetArgString(1)));
        cm.Static("dialogForeign(_,_,_)", ctx => Dialog((DialogBox.Mode)ctx.GetArgInt32(0), ctx.GetArgString(1), ctx.GetArgString(2)));
        
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
    
    public static void Dialog(DialogBox.Mode mode, string portrait, string text)
    {
        Texture2D portraitTex = Resources.GetTextureByName(portrait);

        Action resume = () =>
        {
            using (WrenHandle main = _vm.CreateHandle("main", "MainFiber"))
            using (WrenCallHandle callHandle = _vm.CreateCallHandle("call()"))
            {
                var call = _vm.CreateCall(main, callHandle);
                call.Call();
            }
        };
        
        Popup.Start(new DialogBox(text, resume, mode));
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
}