using System.Numerics;
using Raylib_cs;

namespace nestphalia;

public class StatusCollect
{
    private Dictionary<String, StatusEffect> _effects = new Dictionary<String, StatusEffect>();
    private Minion _affected;
    
    public StatusCollect(Minion affected)
    {
        _affected = affected;
    }
    
    // returns false if the affected already had that effect, even if it stacks successfully.
    public bool Add(StatusEffect effect)
    {
        if (_effects.TryGetValue(effect.ID, out StatusEffect e))
        {
            e.Stack(effect);
            return false;
        }
        else
        {
            _effects.Add(effect.ID, effect);
            return true;
        }
    }
    
    public bool Has(string id)
    {
        return _effects.ContainsKey(id);
    }
    
    public bool TryGet<T>(string id, out T? effect) where T : StatusEffect
    {
        if (_effects.TryGetValue(id, out StatusEffect? get) && get is T got)
        {
            effect = got;
            return true;
        }
        else
        {
            effect = null;
            return false;
        }
    }
    
    public string ListEffects()
    {
        string s = "";
        foreach (StatusEffect effect in _effects.Values)
        {
            if (effect.Hidden && !Screen.DebugMode) continue;
            s += effect.ToString() + "\n";
        }
        return s;
    }
    
    public void Update()
    {
        foreach (string id in _effects.Keys)
        {
            _effects[id].Update(_affected);
            if (_effects[id].IsExpired())
            {
                Remove(id, false);
            }
        }
    }
    
    public void Draw()
    {
        Vector2 pipPos = new Vector2(-_affected.Template.PhysicsRadius + 1, -_affected.Template.PhysicsRadius - 4);
        pipPos += _affected.Position.XYZ2D();
        foreach (string id in _effects.Keys)
        {
            if (_effects[id].Hidden) continue;
            if (_effects[id].PipColor.A > 0)
            {
                Raylib.DrawCircleV(pipPos, 0.75f, _effects[id].PipColor);
                pipPos += Vector2.UnitX * 2;
            }
            _effects[id].Draw(_affected);
        }
    }
    
    // set silent false to invoke the end effect of the status
    private void Remove(string id, bool silent)
    {
        _effects.Remove(id, out StatusEffect? value);
        if (!silent) value?.OnExpire(_affected);
    }
}