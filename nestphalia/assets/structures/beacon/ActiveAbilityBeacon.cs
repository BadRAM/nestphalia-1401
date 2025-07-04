using System.Numerics;
using Newtonsoft.Json.Linq;
using Raylib_cs;

namespace nestphalia;

public abstract class ActiveAbilityBeaconTemplate : StructureTemplate
{
    public double Cooldown;
    // public double CooldownReduction;
    public Texture2D AbilityIcon;
    
    public ActiveAbilityBeaconTemplate(JObject jObject) : base(jObject)
    {
        Cooldown = jObject.Value<double?>("Cooldown") ?? throw new ArgumentNullException();;
        // CooldownReduction = cooldownReduction;
        AbilityIcon = Resources.GetTextureByName(jObject.Value<string?>("AbilityIcon") ?? "");
        Class = jObject.Value<StructureClass?>("Class") ?? StructureClass.Tower;
    }
}

public abstract class ActiveAbilityBeacon : Structure
{
    private ActiveAbilityBeaconTemplate _template;
    protected double TimeLastUsed;
    public Vector2 TargetPosition;
    
    public ActiveAbilityBeacon(ActiveAbilityBeaconTemplate template, Team team, int x, int y) : base(template, team, x, y)
    {
        _template = template;
        TimeLastUsed = Time.Scaled;
    }

    public virtual void Activate(Vector2 targetPosition)
    {
        TimeLastUsed = Time.Scaled;
        TargetPosition = targetPosition;
    }
    public abstract Vector2? SelectPosition(double minimumValue = 100);
    
    public void DrawStatus(int posX, int posY)
    {
        Raylib.DrawTexture(_template.AbilityIcon, posX, posY, IsReady() ? Color.White : Color.Gray);
        if (!IsReady())
        {
            Raylib.DrawRectangle(posX+2, posY+2, 60, (int)(2 + 60 * (1-(Time.Scaled - TimeLastUsed) / _template.Cooldown)), new Color(255, 255, 255, 64));
            GUI.DrawTextCentered(posX + 32, posY + 32, (_template.Cooldown - (Time.Scaled - TimeLastUsed) ).ToString("N0"));
        }

        if (Time.Scaled - TimeLastUsed < 10)
        {
            Screen.SetCamera(World.Camera);
            Raylib.DrawTexture(Template.Texture, (int)(TargetPosition.X - (Template.Texture.Width-20)), (int)(TargetPosition.Y - (Template.Texture.Height-8)), Color.White);
            Screen.SetCamera();
        }
    }

    public bool IsReady()
    {
        return Time.Scaled - TimeLastUsed > _template.Cooldown;
    }
    
    public override void Destroy()
    {
        base.Destroy();
        Team.RemoveBeacon(this);
    }
    
    public override bool NavSolid(Team team)
    {
        return team != Team;
    }
    
    public override bool PhysSolid()
    {
        return false;
    }
}
