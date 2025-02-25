using System.Numerics;
using ZeroElectric.Vinculum;

namespace _2_fort_cs;

public class Team
{
    public string Name;
    private double[,] _fearMap = new double[World.BoardWidth, World.BoardHeight];
    private double[,] _hateMap = new double[World.BoardWidth, World.BoardHeight];
    public bool IsRightSide;
    public bool IsPlayerControlled;
    public List<ActiveAbilityBeacon?> Beacons = new List<ActiveAbilityBeacon?>();
    public const int BeaconCap = 4;
    public Color UnitTint;
    private Texture _abilitySlot;
    private int _usingAbility = -1;
    //private double _minimumValue = 100;
    private double _timeLastAbilityUsed = -10;

    public Team(string name, bool isRightSide, Color unitTint)
    {
        Name = name;
        IsRightSide = isRightSide;
        UnitTint = unitTint;
        _abilitySlot = Resources.GetTextureByName("ability_slot");
    }

    public void Initialize()
    {
        for (int x = 0; x < World.BoardWidth; x++)
        {
            for (int y = 0; y < World.BoardHeight; y++)
            {
                _fearMap[x, y] = 0;

                Structure? s = World.GetTile(x, y);
                if (s == null) continue;
                if (s.Team != this)
                {
                    _hateMap[x, y] = s.Template.BaseHate;
                }
                else
                {
                    _hateMap[x, y] = 0;
                    if (s is ActiveAbilityBeacon b)
                    {
                        Beacons.Add(b);
                    }
                }
            }
        }
        while (Beacons.Count < BeaconCap) Beacons.Add(null);
    }
    
    public double GetHateFor(int x, int y)
    {
        return _hateMap[x, y];
    }
    
    public double GetHateFor(Int2D pos)
    {
        return _hateMap[pos.X, pos.Y];
    }

    public void AddHateFor(double hate, int x, int y)
    {
        _hateMap[x, y] += hate;
    }
    
    public double GetFearOf(int x, int y)
    {
        return _fearMap[x, y];
    }

    public void AddFearOf(double fear, int x, int y)
    {
        _fearMap[x, y] += fear;
    }
    
    public void AddFearOf(double fear, Int2D pos)
    {
        _fearMap[pos.X, pos.Y] += fear;
    }

    public void RemoveBeacon(RallyBeacon rallyBeacon)
    {
        if (Beacons.Contains(rallyBeacon))
        {
            Beacons[Beacons.IndexOf(rallyBeacon)] = null;
        }
    }

    public void Update()
    {
        if (IsPlayerControlled)
        {
            if (_usingAbility != -1 && (Beacons[_usingAbility]?.IsReady() ?? false) && Raylib.IsMouseButtonPressed(MouseButton.MOUSE_BUTTON_LEFT))
            {
                Beacons[_usingAbility]?.Activate(Raylib.GetScreenToWorld2D(Raylib.GetMousePosition(), World.Camera));
                _usingAbility = -1;
            }
            if ((Beacons[0]?.IsReady() ?? false) && Raylib.IsKeyPressed(KeyboardKey.KEY_ONE))   _usingAbility = 0;
            if ((Beacons[1]?.IsReady() ?? false) && Raylib.IsKeyPressed(KeyboardKey.KEY_TWO))   _usingAbility = 1;
            if ((Beacons[2]?.IsReady() ?? false) && Raylib.IsKeyPressed(KeyboardKey.KEY_THREE)) _usingAbility = 2;
        }
        else
        {
            int i = Random.Shared.Next(3);
            if ((Beacons[i]?.IsReady() ?? false) && Time.Scaled - _timeLastAbilityUsed > 10)
            {
                Vector2? targetPos = Beacons[i]?.SelectPosition();
                if (targetPos != null) Beacons[i]?.Activate((Vector2)targetPos);
                _timeLastAbilityUsed = Time.Scaled;
            }
        }
    }
    
    public void Draw()
    {
        int posX = IsRightSide ? Screen.HCenter + 450  : Screen.HCenter - 450;
        for (int i = 0; i < BeaconCap; i++)
        {
            Color c = Raylib.WHITE;
            if (!Beacons[i]?.IsReady() ?? false) c = Raylib.GRAY;
            if (i == _usingAbility) c = Raylib.DARKGRAY;
            Raylib.DrawTexture(_abilitySlot, posX + i*68 - 150, Screen.Bottom - 68, c);
            if (IsPlayerControlled) GUI.DrawTextLeft(posX + i*68 - 150, Screen.Bottom - 68, (i+1).ToString());
            Beacons[i]?.DrawStatus(posX + i*68 - 150, Screen.Bottom - 68);
        }
    }
}