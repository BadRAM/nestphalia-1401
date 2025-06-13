using System.Numerics;
using Raylib_cs;

namespace nestphalia;

public class FlyingMinionTemplate : MinionTemplate
{
    public double CruisingHeight = 32;
    
    public FlyingMinionTemplate(string id, string name, string description, Texture2D texture, double maxHealth, double armor, double damage, double speed, float physicsRadius, double attackDuration = 1) : base(id, name, description, texture, maxHealth, armor, damage, speed, physicsRadius, attackDuration)
    {
    }
    
    public override void Instantiate(Team team, Vector3 position, NavPath? navPath)
    {
        World.RegisterMinion(new FlyingMinion(this, team, position, navPath));
    }
    
    public override bool PathFromNest()
    {
        return false;
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
        // ugly hack to prevent fliers from following paths.
        // if (NavPath.Found && IsFlying && NavPath.Points.Count > 0 )
        // {
        //     NavPath.Points.Clear();
        // }
        
        base.Update();

        if (WantToFly && !IsFlying)
        {
            IsFlying = true;
        }

        if (!WantToFly && IsFlying && Position.Z == 0)
        {
            IsFlying = false;
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
    
    // Flying Minions don't need pathfinding
    public override void SetTarget(Int2D target, double thinkDuration = 0.2)
    {
        if (!IsFlying)
        {
            base.SetTarget(target, thinkDuration);
            return;
        }
        
        NavPath.Reset(Position);
        NavPath.Destination = target;

        SetState(new Wait(this, thinkDuration, () => { State = new Move(this); }));
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