using System.Numerics;
using ZeroElectric.Vinculum;

namespace _2_fort_cs;

public class DoorTemplate : StructureTemplate
{
    public float Range;
    public Texture OpenTexture;
    
    public DoorTemplate(string name, Texture texture, Texture openTexture, float maxHealth, float price, int levelRequirement, float range) : base(name, texture, maxHealth, price, levelRequirement)
    {
        OpenTexture = openTexture;
        Range = range;
    }
    
    public override Door Instantiate(int x, int y)
    {
        return new Door(this, x, y);
    }
}

public class Door : Structure
{
    private bool _isOpen = false;
    private DoorTemplate _template;
    
    
    public Door(DoorTemplate template, int x, int y) : base(template, x, y)
    {
        _template = template;
    }
    
    public override void Update()
    {
        if (_isOpen)
        {
            _isOpen = false;
            foreach (Minion m in World.Minions)
            {
                if (m.Team == Team)
                {
                    if (Raylib.CheckCollisionCircles(position, _template.Range, m.Position, m.Template.PhysicsRadius))
                    {
                        _isOpen = true;
                    }
                }
                else
                {
                    if (Raylib.CheckCollisionCircleRec(m.Position, m.Template.PhysicsRadius, World.GetTileBounds(X, Y)))
                    {
                        _isOpen = true;
                    }
                }
            }
        }
        else
        {
            foreach (Minion m in World.Minions)
            {
                if (m.Team == Team && Raylib.CheckCollisionCircles(position, _template.Range, m.Position, m.Template.PhysicsRadius))
                {
                    _isOpen = true;
                }
            }
        }

    }

    public override bool IsSolid()
    {
        return !_isOpen;
    }

    public override void Draw(int x, int y)
    {
        int t = 127 + (int)(128 * (Health / Template.MaxHealth));
        Raylib.DrawTexture(_isOpen ? _template.OpenTexture : _template.Texture, x, y, new Color(t,t,t,255));
    }
}