using System.Numerics;
using Raylib_cs;

namespace nestphalia;

// This is a minion that targets a wall, delivers a large explosive to it, and then retreats back to it's nest.
public class SapperMinionTemplate : MinionTemplate
{
    public Texture2D RetreatingTexture;
    
    public SapperMinionTemplate(string id, string name, string description, Texture2D texture, Texture2D retreatingTexture, double maxHealth, double armor, double damage, double speed, float physicsRadius) : base(id, name, description, texture, maxHealth, armor, damage, speed, physicsRadius, 0)
    {
        RetreatingTexture = retreatingTexture;
        Projectile = new MortarShellTemplate($"{id}_bomb", Resources.GetTextureByName("sapper_bomb"), damage, 0.4, 4, 0);
        AttackCooldown = 0;
    }

    public override void Instantiate(Team team, Vector2 position, NavPath? navPath)
    {
        Register(new SapperMinion(this, team, position, navPath));
    }
    
    public override bool PathFromNest()
    {
        return false;
    }
}

public class SapperMinion : Minion
{
    private SapperMinionTemplate _template;
    private bool _attacking = true;
    
    public SapperMinion(SapperMinionTemplate template, Team team, Vector2 position, NavPath? navPath) : base(template, team, position, navPath)
    {
        _template = template;
        // We call from base to prevent resharper complaining about calling virtual method in constructor
        base.SetTarget(GetNewTarget());
    }

    protected override void OnAttack()
    {
        base.OnAttack();
        _attacking = false;
        SetTarget(OriginTile);
    }

    // Re-enter burrow when returning.
    protected override void OnTargetReached()
    {
        if (!_attacking && NavPath.Destination == OriginTile)
        {
            if (World.GetTile(OriginTile) is Spawner s)
            {
                s.AddSpawnBonus(1);
            }
            else
            {
                Console.WriteLine("SapperMinion tried to award bonus spawn to a null/non-nest structure!");
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
        base.SetTarget(!_attacking ? OriginTile : target, thinkDuration);
    }

    public override void Draw()
    {
        Z = Position.Y + (IsFlying ? 240 : 0);

        Texture2D texture = _attacking ? _template.Texture : _template.RetreatingTexture;
        Vector2 pos = new Vector2((int)Position.X - texture.Width / 2, (int)Position.Y - texture.Height / 2);
        bool flip = NextPos.X > (int)Position.X;
        Rectangle source = new Rectangle(flip ? texture.Width : 0, 0, flip ? texture.Width : -texture.Width, texture.Height);
        Raylib.DrawTextureRec(texture, source, pos, Team.UnitTint);
        
        DrawDecorators();
        DrawDebug();
    }
}