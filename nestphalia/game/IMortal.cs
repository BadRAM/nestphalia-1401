using System.Numerics;

namespace nestphalia;

// Common interface between structures and minions for use in attack targeting and attribution
public interface IMortal
{
    // public Vector3 Position { get; }
    public double Health { get; }
    public Team Team { get; }
    public Int2D Origin { get; }

    public abstract void Hurt(double damage, Attack? damageSource = null, bool ignoreArmor = false, bool minDamage = true);
    public abstract Vector3 GetPos();
}