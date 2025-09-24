using Raylib_cs;

namespace nestphalia;

public class BurningStatus : StatusEffect
{
    public double Damage; // Damage per second
    
    public BurningStatus(double damage, double duration) : base("Burning", "Burning", duration, new Color(255, 100, 0))
    {
        Damage = damage;
    }

    public override string ToString()
    {
        return $"{Name} - {Damage}/s, {TimeRemaining():N1}s";
    }

    public override void Update(Minion affected)
    {
        affected.Hurt(Damage * Time.DeltaTime, ignoreArmor:true, minDamage:false);
    }

    public override void Stack(StatusEffect newEffect)
    {
        if (newEffect is not BurningStatus newBurning) return;
        if (newBurning.Damage > Damage) 
        {
            Duration = TimeRemaining() * (Damage / newBurning.Damage) + newBurning.Duration;
            StartTime = Time.Scaled;
            Damage = newBurning.Damage;
        }
        
        Duration += newEffect.Duration;
    }
}