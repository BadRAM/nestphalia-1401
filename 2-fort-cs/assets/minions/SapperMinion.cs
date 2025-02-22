using System.Numerics;
using ZeroElectric.Vinculum;

namespace _2_fort_cs;

// This is a minion that targets a wall, delivers a large explosive to it, and then retreats back to it's nest.
public class SapperMinionTemplate : MinionTemplate
{
    public Texture RetreatingTexture;

    public SapperMinionTemplate(string id, string name, string description, Texture texture, Texture retreatingTexture, double maxHealth, double armor, double damage, double speed, float physicsRadius, double attackCooldown = 1) : base(id, name, description, texture, maxHealth, armor, damage, speed, physicsRadius, attackCooldown)
    {
        RetreatingTexture = retreatingTexture;
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
        Target = World.GetTileCenter(NavPath.NextTile(Position));
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
        if (TryAttack())
        {
            _attacking = false;
            NavPath.Found = false;
            NavPath.Waypoints.Clear();
            NavPath.Start = World.PosToTilePos(Position);
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
            Position = Position.MoveTowards(Target, AdjustedSpeed() * Time.DeltaTime);
        }
    }

    public override void Draw()
    {
        base.Draw();
        Color c = _attacking ? new Color(200, 100, 100, 64) : new Color(100, 200, 100, 64);
        Raylib.DrawCircle((int)Position.X, (int)Position.Y, _template.PhysicsRadius, new Color(200, 100, 100, 64));
        // Z = Position.Y + (IsFlying ? 240 : 0);
        //
        // Vector2 pos = new Vector2((int)Position.X - Template.Texture.width / 2, (int)Position.Y - Template.Texture.height / 2);
        // bool flip = Target.X > pos.X;
        // Rectangle source = new Rectangle(flip ? Template.Texture.width : 0, 0, flip ? Template.Texture.width : -Template.Texture.width, Template.Texture.height);
        // //Raylib.DrawTexture(Template.Texture, (int)Position.X - Template.Texture.width/2, (int)Position.Y - Template.Texture.width/2, tint);
        // Raylib.DrawTextureRec(_attacking ? _template.Texture : _template.RetreatingTexture, source, pos, Team.UnitTint);
        //
        // // Debug, shows path
        // if (Raylib.CheckCollisionPointCircle(Raylib.GetScreenToWorld2D(Raylib.GetMousePosition(), World.Camera), Position, Template.PhysicsRadius))
        // {
        //     Vector2 path = Position;
        //     foreach (Int2D i in NavPath.Waypoints)
        //     {
        //         Vector2 v = World.GetTileCenter(i);
        //         Raylib.DrawLine((int)path.X, (int)path.Y, (int)v.X, (int)v.Y, Raylib.LIME);
        //         path = v;
        //     }
        //
        //     if (NavPath.Waypoints.Count == 0)
        //     {
        //         Vector2 v = World.GetTileCenter(NavPath.Destination);
        //         Raylib.DrawLine((int)path.X, (int)path.Y, (int)v.X, (int)v.Y, Raylib.LIME);
        //     }
        // }
    }
}