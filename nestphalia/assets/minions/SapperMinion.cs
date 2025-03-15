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
        Projectile = new MortarShellTemplate(Resources.GetTextureByName("sapper_bomb"), damage, 0.4, 4, 0);
        AttackCooldown = 0;
    }

    public override void Instantiate(Vector2 position, Team team, NavPath? navPath)
    {
        SapperMinion m = new SapperMinion(this, team, position, null);
        World.Minions.Add(m);
        World.Sprites.Add(m);
    }
}

public class SapperMinion : Minion
{
    private SapperMinionTemplate _template;
    private Int2D _startTile;
    private bool _attacking;
    
    public SapperMinion(SapperMinionTemplate template, Team team, Vector2 position, NavPath? navPath) : base(template, team, position, navPath)
    {
        _template = template;
        _attacking = true;
        _startTile = World.PosToTilePos(position);
        Retarget();
        PathFinder.RequestPath(NavPath);
    }

    public override void Update()
    {
        NextPos = World.GetTileCenter(NavPath.NextTile(Position));
        if (!_attacking && NavPath.Found && NavPath.TargetReached(Position))
        {
            if (World.GetTile(_startTile) is Spawner s)
            {
                s.AddSpawnBonus(1);
            }
            else
            {
                Console.WriteLine("SapperMinion tried to award bonus spawn to a null/non-nest structure!");
            }
            Die();
        }
        
        // if the next tile in our path is adjacent and solid, then attack it
        if (_attacking && TryAttack())
        {
            _attacking = false;
            NavPath.Reset(Position);
            NavPath.Destination = _startTile;
            PathFinder.RequestPath(NavPath);
        }
        else
        {
            // if we're at our final destination, ask for a new path. (Don't ask for a new path if we already have)
            if (NavPath.Found && NavPath.TargetReached(Position))
            {
                Retarget();
                PathFinder.RequestPath(NavPath);
            }
            // else, move towards next tile on path.
            Position = Position.MoveTowards(NextPos, AdjustedSpeed() * Time.DeltaTime);
        }

        Frenzy = false;
    }

    public override void Draw()
    {
        //base.Draw();
        //Color c = _attacking ? new Color(200, 100, 100, 64) : new Color(100, 200, 100, 64);
        //Raylib.DrawCircle((int)Position.X, (int)Position.Y, _template.PhysicsRadius, new Color(200, 100, 100, 64));
        Z = Position.Y + (IsFlying ? 240 : 0);

        Texture2D texture = _attacking ? _template.Texture : _template.RetreatingTexture;
        Vector2 pos = new Vector2((int)Position.X - texture.Width / 2, (int)Position.Y - texture.Height / 2);
        bool flip = NextPos.X > pos.X;
        Rectangle source = new Rectangle(flip ? texture.Width : 0, 0, flip ? texture.Width : -texture.Width, texture.Height);
        Raylib.DrawTextureRec(texture, source, pos, Team.UnitTint);
        
        // Debug, shows path
        if (Raylib.CheckCollisionPointCircle(Raylib.GetScreenToWorld2D(Raylib.GetMousePosition(), World.Camera), Position, Template.PhysicsRadius))
        {
            Vector2 path = Position;
            foreach (Int2D i in NavPath.Waypoints)
            {
                Vector2 v = World.GetTileCenter(i);
                Raylib.DrawLine((int)path.X, (int)path.Y, (int)v.X, (int)v.Y, Color.Lime);
                path = v;
            }
        
            if (NavPath.Waypoints.Count == 0)
            {
                Vector2 v = World.GetTileCenter(NavPath.Destination);
                Raylib.DrawLine((int)path.X, (int)path.Y, (int)v.X, (int)v.Y, Color.Lime);
            }
        }
    }
}