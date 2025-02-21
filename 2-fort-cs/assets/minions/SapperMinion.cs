// using System.Numerics;
// using ZeroElectric.Vinculum;
//
// namespace _2_fort_cs;
//
// // This is a minion that targets a wall, delivers a large explosive to it, and then retreats back to it's nest.
// public class SapperMinionTemplate : MinionTemplate
// {
//     public Texture RetreatingTexture;
//     
//     public SapperMinionTemplate(string name, Texture attackingTexture, Texture retreatingTexture, double maxHealth, double armor, ProjectileTemplate projectile, double rateOfFire, double speed, float physicsRadius) : base(name, attackingTexture, maxHealth, armor, projectile, rateOfFire, speed, physicsRadius)
//     {
//         RetreatingTexture = retreatingTexture;
//     }
//
//     public override void Instantiate(Vector2 position, Team team, NavPath? navPath)
//     {
//         SapperMinion m = new SapperMinion(this, team, position, null);
//         World.Minions.Add(m);
//         World.Sprites.Add(m);
//     }
// }
//
// public class SapperMinion : Minion
// {
//     private SapperMinionTemplate _template;
//     private Int2D _startTile;
//     private bool _attacking;
//     
//     public SapperMinion(SapperMinionTemplate template, Team team, Vector2 position, NavPath? navPath) : base(template, team, position, navPath)
//     {
//         _template = template;
//         _attacking = true;
//         _startTile = World.PosToTilePos(position);
//         Retarget();
//     }
//
//     public override void Update()
//     {
//         base.Update();
//     }
//
//     public override void Draw()
//     {
//         Z = Position.Y + (IsFlying() ? 240 : 0);
//
//         Vector2 pos = new Vector2((int)Position.X - Template.Texture.width / 2, (int)Position.Y - Template.Texture.height / 2);
//         bool flip = Target.X > pos.X;
//         Rectangle source = new Rectangle(flip ? Template.Texture.width : 0, 0, flip ? Template.Texture.width : -Template.Texture.width, Template.Texture.height);
//         //Raylib.DrawTexture(Template.Texture, (int)Position.X - Template.Texture.width/2, (int)Position.Y - Template.Texture.width/2, tint);
//         Raylib.DrawTextureRec(_attacking ? _template.Texture : _template.RetreatingTexture, source, pos, Team.UnitTint);
//         
//         // Debug, shows path
//         if (Raylib.CheckCollisionPointCircle(Raylib.GetScreenToWorld2D(Raylib.GetMousePosition(), World.Camera), Position, Template.PhysicsRadius))
//         {
//             Vector2 path = Position;
//             foreach (Int2D i in NavPath.Waypoints)
//             {
//                 Vector2 v = World.GetTileCenter(i);
//                 Raylib.DrawLine((int)path.X, (int)path.Y, (int)v.X, (int)v.Y, Raylib.LIME);
//                 path = v;
//             }
//
//             if (NavPath.Waypoints.Count == 0)
//             {
//                 Vector2 v = World.GetTileCenter(NavPath.Destination);
//                 Raylib.DrawLine((int)path.X, (int)path.Y, (int)v.X, (int)v.Y, Raylib.LIME);
//             }
//         }
//     }
// }