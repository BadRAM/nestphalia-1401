using System.Numerics;
using ZeroElectric.Vinculum;

namespace _2_fort_cs;

public class HopperMinionTemplate : MinionTemplate
{
    public HopperMinionTemplate(string id, string name, string description, Texture texture, double maxHealth, double armor, double damage, double speed, float physicsRadius, double attackCooldown = 1) : base(id, name, description, texture, maxHealth, armor, damage, speed, physicsRadius, attackCooldown)
    {
    }
    
    public override void Instantiate(Vector2 position, Team team, NavPath? navPath)
    {
        HopperMinion m = new HopperMinion(this, team, position, navPath);
        World.Minions.Add(m);
        World.Sprites.Add(m);
    }
}
    
public class HopperMinion : Minion
{
    private bool _jumping;
    private Vector2 _jumpStartPos;
    private Vector2 _jumpEndPos;
    private double _jumpStartTime;
    private double _jumpDuration = 1;
    private double _jumpHeight = 48;
    
    public HopperMinion(MinionTemplate template, Team team, Vector2 position, NavPath? navPath) : base(template, team, position, navPath)
    {
    }
    
    public override void Update()
    {
        // If the next tile is the target, attack it
        // else, if the second from next tile is empty, hop to it
        // else, if the next tile is empty, walk to it.
        // else, attack next tile

        if (_jumping)
        {
            if (Time.Scaled - _jumpStartTime > _jumpDuration)
            {
                IsFlying = false;
                _jumping = false;
                Position = _jumpEndPos;
            }
            else
            {
                double t = (Time.Scaled - _jumpStartTime) / _jumpDuration;
                double arcOffset = Math.Sin(t * Math.PI) * _jumpHeight;
        
                Position = Vector2.Lerp(_jumpStartPos, _jumpEndPos, (float)t);
                Position.Y -= (float)arcOffset;
                return;
            }
        }
        
        Target = World.GetTileCenter(NavPath.NextTile(Position));
        Int2D? ahead = NavPath.LookAhead(1);

        if (ahead == null)
        {
            if (!TryAttack())
            {
                Retarget();
                PathFinder.RequestPath(NavPath);
            }
        }
        else
        {
            Structure? structureAhead = World.GetTile((Int2D)ahead);
            if (structureAhead == null)
            {
                NavPath.Skip();
                _jumping = true;
                IsFlying = true;
                _jumpStartPos = Position;
                _jumpEndPos = World.GetTileCenter((Int2D)ahead);
                _jumpStartTime = Time.Scaled;
            }
            else
            {
                if (!TryAttack())
                {
                    Position = Position.MoveTowards(Target, AdjustedSpeed() * Time.DeltaTime);
                }
            }
        }
    }

    public override void Draw()
    {
        Console.WriteLine(":/");
        base.Draw();
        Raylib.DrawCircle((int)Position.X, (int)Position.Y, Template.PhysicsRadius, _jumping ? new Color(50, 200, 50, 64) : new Color(200, 50, 50, 64));
    }
}