using System.Numerics;
using Newtonsoft.Json.Linq;
using Raylib_cs;
using static Raylib_cs.Raylib;
using static nestphalia.GUI;


namespace nestphalia;

public class LevelEditorScene : Scene
{
    private Level _level = new Level();
    private string _levelBuffer;
    private string _scriptBuffer;
    
    public void Start(Level loadLevel = null)
    {
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

        if (Button300(300, -120, "Save"))
        {
            Save();
        }
        if (Button300(300, -80, "Load"))
        {
            Load();
        }

        _levelBuffer = BigTextCopyPad(300, 0, "Level Metadata", _levelBuffer);
        _scriptBuffer = BigTextCopyPad(300, 40, "Script", _scriptBuffer);
        BigTextCopyPad(300, 100, "Full Level Data", JObject.FromObject(_level).ToString());
        
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