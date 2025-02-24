using System.Numerics;
using ZeroElectric.Vinculum;

namespace _2_fort_cs;

public class FrenzyBeaconTemplate : ActiveAbilityBeaconTemplate
{
    public FrenzyBeaconTemplate(string id, string name, string description, Texture texture, double maxHealth, double price, int levelRequirement, double baseHate, double cooldown, double cooldownReduction, Texture abilityIcon) : base(id, name, description, texture, maxHealth, price, levelRequirement, baseHate, cooldown, cooldownReduction, abilityIcon)
    {
    }

    public override Structure Instantiate(Team team, int x, int y)
    {
        return new FrenzyBeacon(this, team, x, y);
    }
}

public class FrenzyBeacon : ActiveAbilityBeacon
{
    private bool _active;
    private Vector2 _effectPos;
    private double _effectRadius = 48;
    private double _effectDuration = 10;
    
    public FrenzyBeacon(ActiveAbilityBeaconTemplate template, Team team, int x, int y) : base(template, team, x, y)
    {
    }

    public override void Activate(Vector2 targetPosition)
    {
        base.Activate(targetPosition);
        _active = true;
        _effectPos = targetPosition;
    }

    public override Vector2? SelectPosition(double minimumValue)
    {
        Vector2 bestPos = Vector2.Zero;
        double bestValue = Double.MaxValue;
        foreach (Minion minion in World.Minions)
        {
            if (minion.Team != Team) continue;
            double d = Vector2.Distance(minion.Position, World.GetTileCenter(minion.GetTargetTile()));
            if (d <= minimumValue)
            {
                bestPos = minion.Position;
                bestValue = d;
            }
        }
        
        return bestValue < _effectRadius ? bestPos : null;
    }

    public override void Update()
    {
        if (_active)
        {
            foreach (Minion minion in World.Minions)
            {
                if (Raylib.CheckCollisionCircles(_effectPos, (float)_effectRadius, minion.Position, minion.Template.PhysicsRadius))
                {
                    minion.Frenzy = true;
                }
            }

            if (Time.Scaled - TimeLastUsed > _effectDuration)
            {
                _active = false;
            }
        }
    }

    public override void Draw()
    {
        base.Draw();

        if (_active)
        {
            Raylib.DrawCircleV(_effectPos, 48, new Color(255, 64, 64, 64));
        }
    }
}