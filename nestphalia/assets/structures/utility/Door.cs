using System.Numerics;
using Raylib_cs;

namespace nestphalia;

public class DoorTemplate : StructureTemplate
{
    public double Range;
    public Texture2D OpenTexture;
    
    public DoorTemplate(string id, string name, string description, Texture2D texture, Texture2D openTexture, double maxHealth, double price, int levelRequirement, double baseHate, double range) : base(id, name, description, texture, maxHealth, price, levelRequirement, baseHate)
    {
        OpenTexture = openTexture;
        Range = range;
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
                    if (Raylib.CheckCollisionCircles(position, (float)_template.Range, m.Position.XY(), m.Template.PhysicsRadius))
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
                    Raylib.CheckCollisionCircles(position, (float)_template.Range, m.Position.XY(), m.Template.PhysicsRadius))
                {
                    _isOpen = true;
                }
            }
        }

        // _zOffset = position.Y - (_isOpen ? 24 : 0);
    }

    public override bool NavSolid(Team team)
    {
        return team != Team;
    }

    public override bool PhysSolid()
    {
        return !_isOpen;
    }

    public override void Draw()
    {
        int t = 127 + (int)(128 * (Health / Template.MaxHealth));
        int x = (int)(position.X - 12);
        int y = (int)(position.Y - (Template.Texture.Height - 12));
        Raylib.DrawTexture(_isOpen ? _template.OpenTexture : _template.Texture, x, y, new Color(t,t,t,255));
    }
}