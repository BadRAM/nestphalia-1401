using System.Numerics;
using Raylib_cs;

namespace nestphalia;

public class HeroMinionTemplate : MinionTemplate
{
    public HeroMinionTemplate(string id, string name, string description, Texture2D texture, double maxHealth, double armor, double damage, double speed, float physicsRadius, double attackDuration = 1, int walkAnimDelay = 2) 
        : base(id, name, description, texture, maxHealth, armor, damage, speed, physicsRadius, attackDuration, walkAnimDelay)
    {

    }
    
    public override void Instantiate(Team team, Vector2 position, NavPath? navPath)
    {
        World.RegisterMinion(new HeroMinion(this, team, position, navPath));
    }
}
    
public class HeroMinion : Minion
{
    public HeroMinion(HeroMinionTemplate template, Team team, Vector2 position, NavPath navPath) : base(template, team, position, navPath)
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