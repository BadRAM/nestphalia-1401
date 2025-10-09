using System.Numerics;
using Newtonsoft.Json.Linq;
using Raylib_cs;

namespace nestphalia;

// This is a minion that targets a wall, delivers a large explosive to it, and then retreats back to it's nest.
public class SapperMinionTemplate : MinionTemplate
{
    public Texture2D BombTexture;
    
    public SapperMinionTemplate(JObject jObject) : base(jObject)
    {
        BombTexture = Resources.GetTextureByName(jObject.Value<string?>("BombTexture") ?? "");
        AttackDuration = 0;

    }

    public override Minion Instantiate(Team team, Vector3 position, NavPath? navPath)
    {
        return Register(new SapperMinion(this, team, position, navPath));
    }
}

public class SapperMinion : Minion
{
    private SapperMinionTemplate _template;
    private bool _attacking = true;
    
    public SapperMinion(SapperMinionTemplate template, Team team, Vector3 position, NavPath? navPath) : base(template, team, position, navPath)
    {
        _template = template;
        // We call from base to prevent resharper complaining about calling virtual method in constructor
        // It's fine because we only override setTarget to change returning behavior
        base.SetTarget(GetNewTarget());
    }

    protected override void DoAttack()
    {
        base.DoAttack();
        _attacking = false;
        SetTarget(Origin);
    }

    // Re-enter burrow when returning.
    protected override void OnTargetReached()
    {
        if (!_attacking && NavPath.Destination == Origin)
        {
            if (World.GetTile(Origin) is Spawner s)
            {
                s.AddSpawnBonus(1);
            }
            else
            {
                GameConsole.WriteLine("SapperMinion tried to award bonus spawn to a null/non-nest structure!");
            }
            Die();
        }
        else
        {
            base.OnTargetReached();
        }
    }

    // Refuse to go anywhere but home if attack is spent
    public override void SetTarget(Int2D target, double thinkDuration = 0.5)
    {
        base.SetTarget(!_attacking ? Origin : target, thinkDuration);
    }

    public override void Draw()
    {
        // Draw the bomb
        if (_attacking)
        {
            int size = _template.BombTexture.Height;
            bool flip = NextPos.X > Position.X;
            Vector2 pos = new Vector2((flip ? 12 : -12) + Position.X - size / 2f, Position.Y - size / 2f);
            Raylib.DrawTextureV(_template.BombTexture, pos, Color.White);
        }
        
        DrawBug(_attacking ? Template.GetAnimationFrame(AnimationState.Carrying, 0) : State.GetAnimFrame());
        DrawDecorators();
        DrawDebug();
    }
}