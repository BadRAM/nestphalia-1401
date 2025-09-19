using System.Numerics;
using Newtonsoft.Json.Linq;
using Raylib_cs;

namespace nestphalia;

public class SpringBoardTemplate : StructureTemplate
{
    public Vector2 Target;
    public bool IsRandom;
    
    public SpringBoardTemplate(JObject jObject) : base(jObject)
    {
        Target = jObject.Value<JObject>("Target")?.ToObject<Vector2>() ?? new Vector2(0, 72);
        IsRandom = jObject.Value<bool?>("IsRandom") ?? false;
    }
    
    public override SpringBoard Instantiate(Team team, int x, int y)
    {
        return new SpringBoard(this, team, x, y, Target);
    }
}

public class SpringBoard : Structure
{
    private SpringBoardTemplate _template;
    private Vector2 _target;
    
    public SpringBoard(SpringBoardTemplate template, Team team, int x, int y, Vector2 targetOffset) : base(template, team, x, y)
    {
        _template = template;
        if (team.IsRightSide) targetOffset.X = -targetOffset.X;
        _target = position + targetOffset;
    }
    
    public override void Update()
    {
        foreach (Minion m in World.GetMinionsInRegion(new Int2D(X,Y), 1))
        {
            if (m.IsFlying) continue;

            if (_template.IsRandom)
            {
                _target = position + World.RandomUnitCircle() * _template.Target.Length();
            }

            m.SetState(new Minion.Jump(m, _target, 0, 0.75, 1, 48));
        }
    }

    public override bool NavSolid(Team team)
    {
        return team != Team;
    }

    public override bool PhysSolid(Minion minion)
    {
        return false;
    }
}