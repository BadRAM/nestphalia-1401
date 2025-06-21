using System.Numerics;
using Newtonsoft.Json.Linq;
using Raylib_cs;
using static Raylib_cs.Raylib;
using static nestphalia.GUI;
using WrenSharp;

namespace nestphalia;

public class LevelEditorScene : Scene
{
    private Level _level = new Level();
    private string _levelBuffer;
    private string _scriptBuffer;
    private WrenVM _vm;
    
    public void Start(Level loadLevel = null)
    {
        // Create a configuration for the VM that will forward Wren output to System.Console
        var output = new WrenConsoleOutput();
        var config = new WrenVMConfiguration()
        {
            LogErrors = true,
            ErrorOutput = output,
            WriteOutput = output,
        };

        // Fire up a new Wren VM using the configuration from above
        _vm = new WrenSharpVM(config);

        // Run some Wren source code!
        _vm.Interpret(
            module: "main",
            source: "System.print(\"Hello WrenSharp!\")",
            throwOnFailure: true);
        
        if (loadLevel != null)
        {
            _level = loadLevel;
        }
        else
        {
            _level = new Level();
        }
        
        World.InitializeEditor(new Fort());
        
        Program.CurrentScene = this;
        
        Load();
    }
    
    public override void Update()
    {
        
        // ===== DRAW =====
        
        BeginDrawing();
        ClearBackground(Color.Black);
        Screen.DrawBackground(Color.Gray);
        
        World.DrawFloor();

        if (Button300(300, -120, "Save")) Save();
        if (Button300(300, -80, "Load")) Load();
        if (Button300(300, 120, "Execute")) 
        {
            _vm.Interpret(
            module: "main",
            source: _scriptBuffer,
            throwOnFailure: true);
        }

        _levelBuffer = BigTextCopyPad(300, 40, "Level Metadata", _levelBuffer);
        _scriptBuffer = BigTextCopyPad(300, 80, "Script", _scriptBuffer);
        BigTextCopyPad(300, -40, "Full Level Data", JObject.FromObject(_level).ToString());
        
        EndDrawing();
    }

    private void Load()
    {
        // placeholder, replace this with actually loading from file
        JObject j = JObject.FromObject(_level);

        _scriptBuffer = j.Value<string>("Script") ?? "";

        j.Remove("Script");
        j.Remove("FloorTiles");
        j.Remove("Structures");

        _levelBuffer = j.ToString();
    }

    private void Save()
    {
        JObject j = JObject.Parse(_levelBuffer);
        
        _level.LoadValues(j);
        _level.Script = _scriptBuffer;
    }
}