using Newtonsoft.Json.Linq;
using Raylib_cs;

namespace nestphalia;

public class DoorTemplate : StructureTemplate
{
    public double Range;
    public Texture2D OpenTexture;
    
    public DoorTemplate(JObject jObject) : base(jObject)
    {
        Texture = Resources.GetTextureByName(jObject.Value<string?>("Texture") ?? "");
        OpenTexture = Resources.GetTextureByName(jObject.Value<string?>("OpenTexture") ?? "");
        Range = jObject.Value<double?>("Range") ?? 32;
    }
    
    public override Door Instantiate(Team team, int x, int y)
    {
        return new Door(this, team, x, y);
    }
}

public class Door : Structure
{
    private bool _isOpen = false;
    private DoorTemplate _template;
    
    public Door(DoorTemplate template, Team team, int x, int y) : base(template, team, x, y)
    {
        _template = template;
    }
    
    public override void Update()
    {
        if (_isOpen)
        {
            _isOpen = false;
            foreach (Minion m in World.GetMinionsInRegion(new Int2D(X,Y), 2))
            {
                if (m.IsFlying) continue;
                if (m.Team == Team)
                {
                    if (Raylib.CheckCollisionCircles(Position.XY(), (float)_template.Range, m.Position.XY(), m.Template.PhysicsRadius))
                    {
                        _isOpen = true;
                    }
                }
                else
                {
                    if (Raylib.CheckCollisionCircleRec(m.Position.XY(), m.Template.PhysicsRadius, World.GetTileBounds(X, Y)))
                    {
                        _isOpen = true;
                    }
                }
            }
        }
        else
        {
            foreach (Minion m in World.GetMinionsInRegion(new Int2D(X,Y), 2))
            {
                if (!m.IsFlying && 
                    m.Team == Team && 
                    Raylib.CheckCollisionCircles(Position.XY(), (float)_template.Range, m.Position.XY(), m.Template.PhysicsRadius))
                {
                    _isOpen = true;
                }
            }
        }
    }

    public override bool NavSolid(Team team)
    {
        return team != Team;
    }

    public override bool PhysSolid(Minion minion)
    {
        return !_isOpen;
    }

    public override void Draw()
    {
        int t = 127 + (int)(128 * (Health / Template.MaxHealth));
        int x = (int)(Position.X - 12);
        int y = (int)(Position.Y - (Template.Texture.Height - 12));
        Raylib.DrawTexture(_isOpen ? _template.OpenTexture : _template.Texture, x, y, new Color(t,t,t,255));
    }
}