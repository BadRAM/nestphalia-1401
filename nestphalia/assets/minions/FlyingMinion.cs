using System.Numerics;
using Raylib_cs;

namespace nestphalia;

public class FlyingMinionTemplate : MinionTemplate
{
    public FlyingMinionTemplate(string id, string name, string description, Texture2D texture, double maxHealth, double armor, double damage, double speed, float physicsRadius, double attackCooldown = 1) : base(id, name, description, texture, maxHealth, armor, damage, speed, physicsRadius, attackCooldown)
    {
    }
    
    public override void Instantiate(Team team, Vector2 position, NavPath? navPath)
    {
        Register(new FlyingMinion(this, team, position, navPath));
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
    
    public FlyingMinion(MinionTemplate template, Team team, Vector2 position, NavPath? navPath) : base(template, team, position, navPath)
    {
        IsFlying = true;
        _currentFrame = World.RandomInt(4);
    }

    public override void Update()
    {
        base.Update();
        _frameCounter++;
        if (_frameCounter >= 2)
        {
            _currentFrame++;
            _currentFrame %= 4;
            _frameCounter = 0;
        }
    }

    public override void Draw()
    {
        // Always pingpong the wings instead of using state anim frame
        DrawBug(_currentFrame == 3 ? 1 : _currentFrame);
        DrawDecorators();
        DrawDebug();
    }

    // Flying Minions don't need pathfinding
    public override void SetTarget(Int2D target, double thinkDuration = 0.2)
    {
        NavPath.Reset(Position);
        NavPath.Destination = target;
        
        State = new Wait(this, thinkDuration, () => { State = new Move(this); }, Resources.GetTextureByName("particle_confused"));
    }

    // Flying minions can't get lost
    protected override bool CheckIfLost()
    {
        return false; 
    }
}