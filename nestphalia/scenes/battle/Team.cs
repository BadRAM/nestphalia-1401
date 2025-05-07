using System.Numerics;
using Raylib_cs;

namespace nestphalia;

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
    private Texture2D _abilitySlot;
    private Texture2D _healthBar;
    private int _usingAbility = -1;
    //private double _minimumValue = 100;
    private double _timeLastAbilityUsed = -10;
    private double _maxHealth;
    private double _health;

    public Team(string name, bool isRightSide, Color unitTint)
    {
        Name = name;
        IsRightSide = isRightSide;
        UnitTint = unitTint;
        _abilitySlot = Resources.GetTextureByName("ability_slot");
        _healthBar = Resources.GetTextureByName("button_wide");
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

                    if (s is Spawner spawner)
                    {
                        _maxHealth += spawner.Health;
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
        _fearMap[x, y] = Math.Max(0, _fearMap[x, y] + fear);
    }
    
    public void AddFearOf(double fear, Int2D pos)
    {
        AddFearOf(fear, pos.X, pos.Y);
    }

    public double GetHealth()
    {
        return _health;
    }

    public void AddBeacon(ActiveAbilityBeacon beacon)
    {
        for (int i = 0; i < BeaconCap; i++)
        {
            if (Beacons[i] == null)
            {
                Beacons[i] = beacon;
                return;
            }
        }
    }

    public void RemoveBeacon(ActiveAbilityBeacon beacon)
    {
        if (Beacons.Contains(beacon))
        {
            Beacons[Beacons.IndexOf(beacon)] = null;
        }
    }

    public void Update()
    {
        // update health
        _health = 0;
        for (int x = 0; x < World.BoardWidth; x++)
        {
            for (int y = 0; y < World.BoardHeight; y++)
            {
                Structure? s = World.GetTile(x, y);
                if (s == null) continue;
                if (s.Team == this)
                {
                    if (s is Spawner spawner)
                    {
                        _health += spawner.Health;
                    }
                }
            }
        }
        
        // use abilities
        if (IsPlayerControlled)
        {
            if (_usingAbility != -1 && (Beacons[_usingAbility]?.IsReady() ?? false) && Raylib.IsMouseButtonPressed(MouseButton.Left))
            {
                Beacons[_usingAbility]?.Activate(Raylib.GetScreenToWorld2D(Raylib.GetMousePosition(), World.Camera));
                _usingAbility = -1;
            }
            if ((Beacons[0]?.IsReady() ?? false) && Raylib.IsKeyPressed(KeyboardKey.One))   _usingAbility = 0;
            if ((Beacons[1]?.IsReady() ?? false) && Raylib.IsKeyPressed(KeyboardKey.Two))   _usingAbility = 1;
            if ((Beacons[2]?.IsReady() ?? false) && Raylib.IsKeyPressed(KeyboardKey.Three)) _usingAbility = 2;
            if ((Beacons[3]?.IsReady() ?? false) && Raylib.IsKeyPressed(KeyboardKey.Four))  _usingAbility = 3;

            int posX = IsRightSide ? Screen.HCenter + 450  : Screen.HCenter - 450;
            for (int i = 0; i < BeaconCap; i++)
            {
                if (Raylib.IsMouseButtonPressed(MouseButton.Left) && Raylib.CheckCollisionPointRec(GUI.GetScaledMousePosition(), new Rectangle( posX + i*68 - 150, Screen.Bottom - 68, 64, 64)))
                {
                    _usingAbility = i;
                }
            }
        }
        else
        {
            int i = World.Random.Next(BeaconCap);
            if ((Beacons[i]?.IsReady() ?? false) && Time.Scaled - _timeLastAbilityUsed > 8)
            {
                Vector2? targetPos = Beacons[i]?.SelectPosition();
                if (targetPos != null) Beacons[i]?.Activate((Vector2)targetPos);
                _timeLastAbilityUsed = Time.Scaled;
            }
        }
    }
    
    public void Draw()
    {
        // fear/hate debug
        if (Raylib.IsKeyDown(KeyboardKey.H))
        {
            Raylib.BeginMode2D(World.Camera);
            for (int x = 0; x < World.BoardWidth; x++)
            {
                for (int y = 0; y < World.BoardHeight; y++)
                {
                    Raylib.DrawCircleV(World.GetTileCenter(x,y), (float)(GetFearOf(x,y)/10), new Color(128,  128, 255, 128));
                    Raylib.DrawCircleV(World.GetTileCenter(x,y), (float)(GetHateFor(x,y)/10), new Color(255, 0, 128, 128));
                    if (GetFearOf(x,y) < 0)
                    {
                        Raylib.DrawCircleV(World.GetTileCenter(x,y), 12, new Color(255, 255, 0, 128));
                    }
                }
            }
            Raylib.EndMode2D();
        }
        
        // ability slots
        int posX = IsRightSide ? Screen.HCenter + 462  : Screen.HCenter - 462;
        for (int i = 0; i < BeaconCap; i++)
        {
            int x = posX + i * 68 - 134;
            int y = Screen.Bottom - 68;
            Color c = Color.White;
            if (!Beacons[i]?.IsReady() ?? false) c = Color.Gray;
            if (i == _usingAbility) c = Color.DarkGray;
            Raylib.DrawTexture(_abilitySlot, x, y, c);
            Beacons[i]?.DrawStatus(x, y);
            if (IsPlayerControlled)
            {
                GUI.DrawTextLeft(x, y, (i + 1).ToString());
                if ((Beacons[i]?.IsReady() ?? false) && Raylib.CheckCollisionPointRec(GUI.GetScaledMousePosition(), new Rectangle( x, y, 64, 64)))
                {
                    Raylib.DrawRectangle(x, y, 64, 64, new Color(255, 255, 255, 32));
                }
            }   
        }
        
        // health bar
        Raylib.DrawTextureRec(_healthBar, new Rectangle(0, 80, 300, 40), new Vector2(IsRightSide ? Screen.HCenter + 20  : Screen.HCenter - 320, Screen.Bottom - 44), Color.White);
        float hpBarSize = (float)(300 * _health / _maxHealth);
        Raylib.DrawTextureRec(_healthBar, new Rectangle( IsRightSide ? 300 - hpBarSize : 0, 40, hpBarSize, 40), new Vector2(IsRightSide ? (300 - hpBarSize) + Screen.HCenter + 20  : Screen.HCenter - 320, Screen.Bottom -44), Color.White);
        GUI.DrawTextCentered(IsRightSide ? Screen.HCenter + 160 : Screen.HCenter - 160, Screen.Bottom - 24, $"{Name} - {_health:n0}/{_maxHealth:n0}");
    }
}