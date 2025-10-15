using System.Numerics;
using Raylib_cs;

namespace nestphalia;

public class CustomBattleMenu : Scene
{
    private static Fort? _leftFort;
    private static Fort? _rightFort;
    private static bool _leftIsPlayer;
    private static bool _rightIsPlayer;
    private static bool _deterministicMode;
    private string _outcomeMessage = "";
    private string _activeDirectory = "";
    private Level _arenaLevel;

    public void Start(Fort? leftFort = null, Fort? rightFort = null)
    {
        _arenaLevel = Assets.Get<Level>("level_arena");
        World.InitializePreview(_arenaLevel);

        if (leftFort != null) _leftFort = leftFort;
        if (rightFort != null) _rightFort = rightFort;
        _leftFort?.LoadToBoard(_arenaLevel.FortSpawnZones[0]);
        _rightFort?.LoadToBoard(_arenaLevel.FortSpawnZones[1]);
        _outcomeMessage = "";
        _activeDirectory = "";
        Program.CurrentScene = this;
        Screen.RegenerateBackground();
        // Resources.PlayMusicByName("scene03");
        Resources.PlayMusicByName("nd_credits_live");
    }
    
    public override void Update()
    {
        World.Camera.Zoom = (Settings.Saved.SmallScreenMode ? 0.5f : 0.75f) * GUI.GetWindowScale().X;
        World.Camera.Offset = new Vector2(Screen.CenterX, Screen.CenterY+50) * GUI.GetWindowScale();
        
        if (Input.Pressed(InputAction.Exit))
        {
            new MenuScene().Start();
        }
        
        Screen.BeginDrawing();
        Raylib.ClearBackground(Color.Gray);
        Screen.DrawBackground(Color.DarkGray);
        
        World.DrawFloor();
        World.Draw();
        
        if (GUI.Button90(0, 76, $"Pick", anchor: Screen.FocusZone.TopLeft()))
        {
            PopupManager.Start(new FortPickerPopup(Directory.GetCurrentDirectory() + "/forts/", 
            fort =>
            {
                _leftFort = fort;
                _leftFort.LoadToBoard(_arenaLevel.FortSpawnZones[0]);
            },
            path =>
            {
                new EditorScene().Start(fort => {Start(leftFort:fort);}, new Fort(path));
            }));
        }
        if (GUI.Button90(-90, 76, $"Pick", anchor: Screen.FocusZone.TopRight()))
        {
            PopupManager.Start(new FortPickerPopup(Directory.GetCurrentDirectory() + "/forts/", 
            fort =>
            {
                _rightFort = fort;
                _rightFort.LoadToBoard(_arenaLevel.FortSpawnZones[1]);
            },
            path =>
            {
                new EditorScene().Start(fort => {Start(rightFort:fort);}, new Fort(path));
            }));
        }
        if (GUI.Button90(  0, 38, $"Edit", _leftFort  != null, Screen.FocusZone.TopLeft()))  new EditorScene().Start(f => {Start(leftFort:f);},  _leftFort);
        if (GUI.Button90(-90, 38, $"Edit", _rightFort != null, Screen.FocusZone.TopRight())) new EditorScene().Start(f => {Start(rightFort:f);}, _rightFort);
        
        if (GUI.Button90(  0, 0, _leftIsPlayer  ? "PLAYER" : "CPU", _leftFort  != null, Screen.FocusZone.TopLeft()))  _leftIsPlayer  = !_leftIsPlayer;
        if (GUI.Button90(-90, 0, _rightIsPlayer ? "PLAYER" : "CPU", _rightFort != null, Screen.FocusZone.TopRight())) _rightIsPlayer = !_rightIsPlayer;
        
        string vs = (_leftFort  != null ? _leftFort.Name  : "???") + " VS " +
                    (_rightFort != null ? _rightFort.Name : "???");
        
        GUI.DrawTextCentered(0, 54, vs, 32, anchor: Screen.FocusZone.TopCenter());
        GUI.DrawTextLeft(-130, 76, FortSummary(_leftFort),  wrapWidth: 100, anchor: Screen.FocusZone.TopCenter());
        GUI.DrawTextLeft(  30, 76, FortSummary(_rightFort), wrapWidth: 100, anchor: Screen.FocusZone.TopCenter());
        GUI.DrawTextCentered(0, 80, _outcomeMessage, 24, anchor: Screen.FocusZone.BottomCenter());
        
        if (GUI.Button90(-137, 0, "Folder", anchor: Screen.FocusZone.TopCenter())) Utils.OpenFolder(@"\forts");
        
        if (GUI.Button90(47, 0, $"Fate: {(_deterministicMode ? "On" : "Off")}", anchor: Screen.FocusZone.TopCenter()))
        {
            _deterministicMode = !_deterministicMode;
        }
        
        if (GUI.Button90(-45, 0, "Back", anchor: Screen.FocusZone.TopCenter()))
        {
            new MenuScene().Start();
        }
        
        if (_leftFort != null && _rightFort != null &&
            (GUI.Button270(-135, -40, "Begin!", anchor: Screen.FocusZone.BottomCenter()) || Input.Pressed(InputAction.QuickLoad)))
        {
            new BattleScene().Start(Assets.Get<Level>("level_arena"), _leftFort, _rightFort, BattleOver, _leftIsPlayer, _rightIsPlayer, _deterministicMode);
        }
        else if (Input.Pressed(InputAction.QuickLoad))
        {
            if (_rightFort == null || _leftFort == null)
            {
                List<Fort> forts = new List<Fort>();
                foreach (string file in Directory.GetFiles(Directory.GetCurrentDirectory() + "/forts/"))
                {
                    if (file.Substring(file.Length - 5) == ".fort")
                    {
                        forts.Add(Fort.LoadFromDisc(file));
                    }
                }
                forts = forts.OrderBy(o => o.Name.ToLower()).ToList();
                _leftFort = forts[0];
                _leftFort.LoadToBoard(_arenaLevel.FortSpawnZones[0]);
                _rightFort = forts[0];
                _rightFort.LoadToBoard(_arenaLevel.FortSpawnZones[1]);
            }
        }
    }

    private string FortSummary(Fort? fort)
    {
        if (fort == null) return "";
        return $"${fort.TotalCost}\n" +
               $"{fort.NestCount} nests\n" +
               $"{(fort.HasSecret ? "Contains Forbidden Technology!" : "")}";
    }

    private void BattleOver(Team? winner)
    {
        Start();
        if (winner == null)
        {
            _outcomeMessage = "Battle aborted.";
        }
        else
        {
            _outcomeMessage = winner.Name + " won the battle!";
        }
    }
}