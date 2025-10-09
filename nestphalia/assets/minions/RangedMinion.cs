using System.Numerics;
using Newtonsoft.Json.Linq;

namespace nestphalia;

public class RangedMinionTemplate : MinionTemplate
{
    public double Range;
    
    public RangedMinionTemplate(JObject jObject) : base(jObject)
    {
        Range = jObject.Value<double?>("Range") ?? throw new ArgumentNullException();
        AttackType = Enum.Parse<Minion.StateType>(jObject.Value<string?>("AttackType") ?? "RangedAttack");
        
    }
    
    public override Minion Instantiate(Team team, Vector3 position, NavPath? navPath)
    {
        return Register(new RangedMinion(this, team, position, navPath));
    }

    public override void RequestPath(Int2D startPos, Int2D targetPos, NavPath navPath, Team team, Minion minion = null)
    {
        navPath.Reset(startPos);
        navPath.Destination = targetPos;

        int radius = (int)(Range / 24) + 1;
        Vector2 center = World.GetTileCenter(targetPos);
        for (int x = Math.Max(0, targetPos.X - radius); x < Math.Min(World.BoardWidth,  targetPos.X + radius); x++)
        for (int y = Math.Max(0, targetPos.Y - radius); y < Math.Min(World.BoardHeight, targetPos.Y + radius); y++)
        {
            if (Vector2.Distance(center, World.GetTileCenter(x,y)) < Range)
            {
                navPath.Points.Add(new Int2D(x,y));
            }
        }
        
        team.RequestPath(navPath);
    }
}
    
public class RangedMinion : Minion
{
    private RangedMinionTemplate _template;

    private Vector2 _target;
    private int _lookAhead;
    
    public RangedMinion(RangedMinionTemplate template, Team team, Vector3 position, NavPath navPath) : base(template, team, position, navPath)
    {
        _template = template;
        _target = World.GetTileCenter(navPath.Destination);
        _lookAhead = (int)_template.Range / 24 + 1;
    }

    public override void SetTarget(Int2D target, double thinkDuration = 0.5)
    {
        _target = World.GetTileCenter(target);
        base.SetTarget(target, thinkDuration);
    }

    protected override bool CanAttack()
    {
        // Don't attack while we're on top of structures
        if (IsOnTopOfStructure) return false;
        // Guard against out of range attacks
        if (Vector2.Distance(Position.XY(), NextPos) > _template.Range) return false;
        Structure? t = World.GetTileAtPos(NextPos);
        // Guard against attacking tiles that could just be walked over
        if (t == null || t is Rubble || (!t.NavSolid(Team) && NavPath.NextTile(Position.XY()) != NavPath.Destination)) return false;
        // Guard against friendly tiles that can be traversed
        if (t.Team == Team && !t.PhysSolid(this)) return false; 

        return true;
    }

    public override void Update()
    {
        // Check that we're in a valid state to start an attack
        if (NavPath.Found && !IsOnTopOfStructure && State is Move)
        {
            // Check if we're in range of our final target
            if (Vector2.Distance(Position.XY(), _target) < _template.Range)
            {
                Structure? t = World.GetTileAtPos(_target);
                if (t is Rubble || t == null) // check if it's been destroyed
                {
                    OnTargetReached();
                } 
                else // else attack it
                {
                    NextPos = _target;
                    ResetState(Template.AttackType);
                }
            }
            else
            {
                Vector2 n = NextPos;
                for (int i = 0; i < _lookAhead; i++)
                {
                    Int2D? pos = NavPath.LookAhead(i);
                    if (pos == null) break;
                    NextPos = World.GetTileCenter(pos.Value);
                    if (CanAttack())
                    {
                        ResetState(Template.AttackType);
                        break;
                    }
                    NextPos = n;
                }
            }
        }
        
        base.Update();
    }
}