using System.Numerics;
using Raylib_cs;

namespace nestphalia;

public class HopperMinionTemplate : MinionTemplate
{
    public HopperMinionTemplate(string id, string name, string description, Texture2D texture, double maxHealth, double armor, double damage, double speed, float physicsRadius, double attackCooldown = 1) : base(id, name, description, texture, maxHealth, armor, damage, speed, physicsRadius, attackCooldown)
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
    private double _jumpDuration = 0.5;
    private double _jumpChargeDuration = 0.5;
    private double _jumpHeight = 24;
    
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
            if (Time.Scaled - _jumpStartTime < _jumpChargeDuration)
            {
                return;
            }
            else if (Time.Scaled - _jumpStartTime > _jumpDuration + _jumpChargeDuration)
            {
                IsFlying = false;
                _jumping = false;
                Position = _jumpEndPos;
            }
            else
            {
                IsFlying = true;

                double t = (Time.Scaled - (_jumpStartTime + _jumpChargeDuration)) / _jumpDuration;
                double arcOffset = Math.Sin(t * Math.PI) * _jumpHeight;
        
                Position = Vector2.Lerp(_jumpStartPos, _jumpEndPos, (float)t);
                Position.Y -= (float)arcOffset;
                return;
            }
        }
        
        UpdateNextPos();
        
        Int2D? ahead = NavPath.LookAhead(1);
        
        if (ahead == null)
        {
            if (!TryAttack())
            {
                if (NavPath.Found && NavPath.TargetReached(Position))
                {
                    Retarget();
                    PathFinder.RequestPath(NavPath);
                }
                else
                {
                    Position = Position.MoveTowards(NextPos, AdjustedSpeed() * Time.DeltaTime);
                }
            }
        }
        else
        {
            Structure? structureAhead = World.GetTile((Int2D)ahead);
            if (structureAhead == null || (!structureAhead.PhysSolid(Team) && (structureAhead.Team != Team || structureAhead is not Minefield)))
            {
                NavPath.Skip();
                _jumping = true;
                _jumpStartPos = Position;
                _jumpEndPos = World.GetTileCenter((Int2D)ahead) + new Vector2(Random.Shared.Next(17)-8, Random.Shared.Next(23)-11);
                _jumpStartTime = Time.Scaled;
            }
            else
            {
                if (!TryAttack())
                {
                    Position = Position.MoveTowards(NextPos, AdjustedSpeed() * Time.DeltaTime);
                }
            }
        }

        Frenzy = false;
    }
}