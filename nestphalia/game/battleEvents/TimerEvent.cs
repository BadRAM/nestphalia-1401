using WrenNET;

namespace nestphalia;

public class TimerEvent : BattleEvent
{
    public double Duration;
    public bool Recurring;
    private double _startTime;
    
    public TimerEvent(double duration, bool recurring, WrenHandle handle, WrenCommand wrenCommand) : base(handle, wrenCommand)
    {
        Duration = duration;
        Recurring = recurring;
        _startTime = Time.Scaled;
    }

    public override bool Update()
    {
        if (Time.Scaled - _startTime >= Duration)
        {
            _startTime = Time.Scaled;
            Invoke();
            return !Recurring;
        }
        return false;
    }
}