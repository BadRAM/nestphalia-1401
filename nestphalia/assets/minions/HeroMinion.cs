using System.Numerics;
using System.Runtime.InteropServices.JavaScript;
using Newtonsoft.Json.Linq;
using Raylib_cs;

namespace nestphalia;

public class HeroMinionTemplate : MinionTemplate
{
    public HeroMinionTemplate(JObject jObject) : base(jObject)
    {
    }
    
    public override Minion Instantiate(Team team, Vector3 position, NavPath? navPath)
    {
        Minion m = new HeroMinion(this, team, position, navPath);
        World.RegisterMinion(m);
        return m;
    }
}
    
public class HeroMinion : Minion
{
    public HeroMinion(HeroMinionTemplate template, Team team, Vector3 position, NavPath navPath) : base(template, team, position, navPath)
    {
    }

    public override void Update()
    {
        // base.Update();
        if (Raylib.IsKeyDown(KeyboardKey.I)) Position.Y -= (float)(Template.Speed * Time.DeltaTime);
        if (Raylib.IsKeyDown(KeyboardKey.K)) Position.Y += (float)(Template.Speed * Time.DeltaTime);
        if (Raylib.IsKeyDown(KeyboardKey.J)) Position.X -= (float)(Template.Speed * Time.DeltaTime);
        if (Raylib.IsKeyDown(KeyboardKey.L)) Position.X += (float)(Template.Speed * Time.DeltaTime);
    }

    public override void Die()
    {

    }
}