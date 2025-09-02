using System.Numerics;
using Newtonsoft.Json.Linq;
using Raylib_cs;

namespace nestphalia;

public class FlyingMinionTemplate : MinionTemplate
{
    public double CruisingHeight = 32;
    
    public FlyingMinionTemplate(JObject jObject) : base(jObject)
    {
    }
    
    public override Minion Instantiate(Team team, Vector3 position, NavPath? navPath)
    {
        Minion m = new FlyingMinion(this, team, position, navPath);
        World.RegisterMinion(m);
        return m;
    }

    public override void RequestPath(Int2D startPos, Int2D targetPos, NavPath navPath, Team team, Minion minion = null)
    {
        if (minion == null || minion.IsFlying)
        {
            navPath.Reset(startPos);
            navPath.Destination = targetPos;
            navPath.Found = true;
        }
        else
        {
            base.RequestPath(startPos, targetPos, navPath, team, minion);
        }
    }
}

public class FlyingMinion : Minion
{
    private int _currentFrame;
    private int _frameCounter;
    private FlyingMinionTemplate _template;
    protected bool WantToFly;
    protected int FlyAnimIndex = 0;
    
    public FlyingMinion(FlyingMinionTemplate template, Team team, Vector3 position, NavPath? navPath) : base(template, team, position, navPath)
    {
        WantToFly = true;
        IsFlying = true;
        _currentFrame = World.RandomInt(4);
        _template = template;
    }
    
    public override void Update()
    {
        base.Update();

        if (WantToFly && !IsFlying)
        {
            IsFlying = true;
        }

        if (!WantToFly && IsFlying && Position.Z == 0)
        {
            IsFlying = false;
            if (World.GetTile(World.PosToTilePos(Position))?.PhysSolid(this) ?? false)
            {
                IsOnTopOfStructure = true;
            }
        }

        if (IsFlying)
        {
            if (State is Move)
            {
                if (WantToFly)
                {
                    Position.Z = Position.Z.MoveTowards((float)_template.CruisingHeight, (float)(12 * Time.DeltaTime));
                }
                else
                {
                    Position.Z = Position.Z.MoveTowards(0f, (float)(48 * Time.DeltaTime));
                }
            }
            
            _frameCounter++;
            if (_frameCounter >= 2)
            {
                _currentFrame++;
                _currentFrame %= 4;
                _frameCounter = 0;
            }
        }
    }
    
    public override void Draw()
    {
        if (!IsFlying)
        {
            base.Draw();
            return;
        }
        // Always pingpong the wings instead of using state anim frame
        DrawBug((_currentFrame == 3 ? 1 : _currentFrame) + FlyAnimIndex);
        DrawDecorators();
        DrawDebug();
    }

    // Flying minions can't get lost
    protected override bool CheckIfLost()
    {
        if (!IsFlying)
        {
            return base.CheckIfLost();
        }
        
        return false;
    }
}