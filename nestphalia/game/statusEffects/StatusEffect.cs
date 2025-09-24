using Raylib_cs;

namespace nestphalia;

public class StatusEffect
{
    public string ID;
    public string Name;
    public Color PipColor;
    public bool Hidden;
    public double Duration;
    protected double StartTime;

    public StatusEffect(string id, string name, double duration, Color pipColor, bool hidden = false)
    {
        ID = id;
        Name = name;
        Duration = duration;
        Hidden = hidden;
        PipColor = pipColor;
        StartTime = Time.Scaled;
    }
    
    // ======== EVENTS ========
    
    // Is called every frame.
    public virtual void Update(Minion affected) {}

    // Is called immediately after affected minion is drawn
    public virtual void Draw(Minion affected) {}

    // Is called when a new effect of the same type wants to be applied to a minion.
    public virtual void Stack(StatusEffect newEffect)
    {
        if (newEffect.Duration > TimeRemaining())
        {
            StartTime = Time.Scaled;
            Duration = newEffect.Duration;
        }
    }
    
    // Is called when the effect wears off naturally.
    public virtual void OnExpire(Minion affected) {}
    
    // Is called to represent the effect in tooltips / minion info panels
    public virtual string ToString()
    {
        return $"{(Hidden ? "Hidden - " : "")}{Name}{(Duration <= -1 ? "" : $" - {TimeRemaining():N1}s")}";
    }
    
    // ======== PUBLIC ========
    
    public bool IsExpired()
    {
        if (Duration <= -1) return false;
        return Time.Scaled - StartTime > Duration;
    }

    public double TimeRemaining()
    {
        if (Duration <= -1) return Duration;
        return Duration - (Time.Scaled - StartTime);
    }

    public double PercentComplete()
    {
        if (Duration <= -1) return 0;
        return (Time.Scaled - StartTime) / Duration;
    }
}