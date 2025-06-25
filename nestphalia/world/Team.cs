using System.Diagnostics;
using System.Net;
using System.Numerics;
using Raylib_cs;

namespace nestphalia;

// TODO: Cache target list here
public class Team
{
    public string Name;
    private double[,] _fearMap = new double[World.BoardWidth, World.BoardHeight];
    private double[,] _hateMap = new double[World.BoardWidth, World.BoardHeight];
    private double[,] _weightMap = new double[World.BoardWidth, World.BoardHeight];
    private bool[,] _navSolidMap = new bool[World.BoardWidth, World.BoardHeight];
    public bool IsRightSide;
    public bool IsPlayerControlled;
    public List<ActiveAbilityBeacon?> Beacons = new List<ActiveAbilityBeacon?>();
    public const int BeaconCap = 4;
    public Color Color;
    private Texture2D _abilitySlot;
    private Texture2D _healthBar;
    private int _usingAbility = -1;
    //private double _minimumValue = 100;
    private double _timeLastAbilityUsed = -10;
    private double _maxHealth;
    private double _health;

    private List<NavPath> _pathQueue = new List<NavPath>();
    private List<NavPath> _priorityPathQueue = new List<NavPath>();

    public PathFinder PathFinder = new PathFinder();

    public Team(string name, bool isRightSide, Color color)
    {
        Name = name;
        IsRightSide = isRightSide;
        Color = color;
        _abilitySlot = Resources.GetTextureByName("ability_slot");
        _healthBar = Resources.GetTextureByName("button_wide");
    }

    public void Initialize()
    {
        for (int x = 0; x < World.BoardWidth; x++)
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
            _weightMap[x, y] = CalculateWeight(x, y);
            _navSolidMap[x, y] = World.GetTile(x, y)?.NavSolid(this) ?? false;
        }
        
        while (Beacons.Count < BeaconCap) Beacons.Add(null);

        World.StructureChanged += OnStructureChanged;
    }

    #region Path Queue
    public void RequestPath(NavPath navPath)
    {
        // One thousand guards
        Debug.Assert(!navPath.Found);
        //Debug.Assert(!_pathQueue.Contains(navPath));
        if (_pathQueue.Contains(navPath))
        {
            GameConsole.WriteLine($"{navPath.Requester} double requested a path");
            return;
        }
        //Debug.Assert(navPath.Requester != "Beetle");
        if (navPath.Start == navPath.Destination)
        {
            GameConsole.WriteLine($"{navPath.Requester} requested a zero length path.");
            navPath.Found = true;
            return;
        }
        
        // One function call
        _pathQueue.Add(navPath);
    }
    
    // Called to force a path to be found the next time the queue is served! 
    public void DemandPath(NavPath navPath)
    {
        if (_pathQueue.Remove(navPath))
        {
            GameConsole.WriteLine($"{navPath.Requester}'s path jumped the queue");
        }
        _priorityPathQueue.Add(navPath);
    }

    public void ServeQueue(int max)
    {
        double startTime = Raylib.GetTime();
        int i = 0;
        while (_priorityPathQueue.Count > 0)
        {
            NavPath p = _priorityPathQueue[0];
            _priorityPathQueue.RemoveAt(0);
            if (p.Found) // Catch queue duplicates
            {
                GameConsole.WriteLine($"{p.Requester} requested priority pathing from Team {Name} on a navPath that's already been found.");
                continue;
            }
            PathFinder.FindPath(p);
            i++;
        }
        for (; i < max; i++)
        {
            if (_pathQueue.Count == 0)  return;
            NavPath p = _pathQueue[0];
            _pathQueue.RemoveAt(0);
            if (p.Found) // Catch queue duplicates
            {
                GameConsole.WriteLine($"{p.Requester} requested pathing from Team {Name} on a navPath that's already been found.");
                continue;
            }
            PathFinder.FindPath(p);
        }
        if (i >= max-1)
        {
            GameConsole.WriteLine($"Max path calc, {i} in {((Raylib.GetTime() - startTime) * 1000):N3}ms, Queue length: {GetQueueLength()}");
        }
    }
    
    public int GetQueueLength()
    {
        return _pathQueue.Count;
    }

    public void ClearQueue()
    {
        _pathQueue.Clear();
    }
    #endregion
    
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
        
        if (World.IsBattleOver()) return;
        // use abilities
        if (IsPlayerControlled)
        {
            if (_usingAbility != -1 && (Beacons[_usingAbility]?.IsReady() ?? false) && Input.Pressed(MouseButton.Left))
            {
                Beacons[_usingAbility]?.Activate(Raylib.GetScreenToWorld2D(Raylib.GetMousePosition(), World.Camera));
                _usingAbility = -1;
            }
            if ((Beacons[0]?.IsReady() ?? false) && Input.Pressed(Input.InputAction.Use1)) _usingAbility = 0;
            if ((Beacons[1]?.IsReady() ?? false) && Input.Pressed(Input.InputAction.Use2)) _usingAbility = 1;
            if ((Beacons[2]?.IsReady() ?? false) && Input.Pressed(Input.InputAction.Use3)) _usingAbility = 2;
            if ((Beacons[3]?.IsReady() ?? false) && Input.Pressed(Input.InputAction.Use4)) _usingAbility = 3;

            int posX = IsRightSide ? Screen.CenterX + 450  : Screen.CenterX - 450;
            for (int i = 0; i < BeaconCap; i++)
            {
                if (Input.Pressed(MouseButton.Left) && Raylib.CheckCollisionPointRec(GUI.GetScaledMousePosition(), new Rectangle( posX + i*68 - 150, Screen.BottomY - 68, 64, 64)))
                {
                    _usingAbility = i;
                }
            }
        }
        else
        {
            int i = World.RandomInt(BeaconCap);
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
        if (Input.Held(KeyboardKey.H))
        {
            Raylib.BeginMode2D(World.Camera);
            for (int x = 0; x < World.BoardWidth; x++)
            for (int y = 0; y < World.BoardHeight; y++)
            {
                Raylib.DrawCircleV(World.GetTileCenter(x,y), (float)(GetFearOf(x,y)/10), new Color(128,  128, 255, 128));
                Raylib.DrawCircleV(World.GetTileCenter(x,y), (float)(GetHateFor(x,y)/10), new Color(255, 0, 128, 128));
                if (GetFearOf(x,y) < 0)
                {
                    Raylib.DrawCircleV(World.GetTileCenter(x,y), 12, new Color(255, 255, 0, 128));
                }
                Raylib.DrawCircleV(World.GetTileCenter(x,y), MathF.Min((float)(_weightMap[x,y]/10), 10), new Color(255,  64, 255, 128));
            }
            Raylib.EndMode2D();
        }
        
        // ability slots
        int posX = IsRightSide ? Screen.CenterX + 462  : Screen.CenterX - 462;
        for (int i = 0; i < BeaconCap; i++)
        {
            int x = posX + i * 68 - 134;
            int y = Screen.BottomY - 68;
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
        Raylib.DrawTextureRec(_healthBar, new Rectangle(0, 80, 300, 40), new Vector2(IsRightSide ? Screen.CenterX + 20  : Screen.CenterX - 320, Screen.BottomY - 44), Color.White);
        float hpBarSize = (float)(300 * _health / _maxHealth);
        Raylib.DrawTextureRec(_healthBar, new Rectangle( IsRightSide ? 300 - hpBarSize : 0, 40, hpBarSize, 40), new Vector2(IsRightSide ? (300 - hpBarSize) + Screen.CenterX + 20  : Screen.CenterX - 320, Screen.BottomY -44), Color.White);
        GUI.DrawTextCentered(IsRightSide ? Screen.CenterX + 160 : Screen.CenterX - 160, Screen.BottomY - 24, $"{Name} - {_health:n0}/{_maxHealth:n0}", anchor: Screen.TopLeft);
    }
    
    public double GetTileWeight(int x, int y)
    {
        return _fearMap[x, y] + _weightMap[x, y];
    }

    public bool GetNavSolid(int x, int y)
    {
        return _navSolidMap[x, y];
    }

    private double CalculateWeight(int x, int y)
    {
        double weight = 0;
        Structure? structure = World.GetTile(x, y);
        if (!(structure == null || structure is Rubble))
        {
            if (structure is Minefield && structure.Team == this)
            {
                weight += structure.Health;
            }
            else if (structure is HazardSign && structure.Team == this)
            {
                weight += 1000000;
            }
            else
            {
                if (structure.NavSolid(this))
                {
                    weight += structure.Health;
                    if (structure.Team == this) weight += 1000000; //return null;
                }
            }
        }
        return weight;
    }
    
    private void OnStructureChanged(object? sender, Int2D pos)
    {
        _weightMap[pos.X, pos.Y] = CalculateWeight(pos.X, pos.Y);
        _navSolidMap[pos.X, pos.Y] = World.GetTile(pos.X, pos.Y)?.NavSolid(this) ?? false;
    }
}