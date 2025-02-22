// using System.Numerics;
// using ZeroElectric.Vinculum;
//
// namespace _2_fort_cs;
//
// public class LightningBoltTemplate : ProjectileTemplate
// {
//     public LightningBoltTemplate(double damage) : base(Resources.MissingTexture, damage, 0)
//     {
//     }
//
//     public override void Instantiate(object target, object source)
//     {
//         Vector2 pos = Vector2.Zero;
//         if (source is Minion m) pos = m.Position;
//         if (source is Structure s) pos = s.GetCenter();
//         LightningBolt p = new LightningBolt(this, pos, target, source);
//         World.Projectiles.Add(p);
//         World.Sprites.Add(p);
//     }
// }
//
// public class LightningBolt : Projectile
// {
//     public LightningBolt(ProjectileTemplate template, Vector2 position, object target, object source) : base(template, position, target, source)
//     {
//         
//     }
//
//     public override void Update()
//     {
//         base.Update();
//     }
//
//     public override void Draw()
//     {
//         base.Draw();
//     }
// }