// using System.Diagnostics;
// using System.Numerics;
// using ZeroElectric.Vinculum;
//
// namespace _2_fort_cs;
//
// public class LightningTowerTemplate : TowerTemplate
// {
//     public LightningTowerTemplate(string id, string name, string description, Texture texture, double maxHealth, double price, int levelRequirement, double baseHate, double range, ProjectileTemplate projectile, double rateOfFire, TargetSelector targetMode, bool canHitGround, bool canHitFlying) : base(id, name, description, texture, maxHealth, price, levelRequirement, baseHate, range, projectile, rateOfFire, targetMode, canHitGround, canHitFlying)
//     {
//     }
//
//     public override LightningTower Instantiate(Team team, int x, int y)
//     {
//         return new LightningTower(this, team, x, y);
//     }
//     
//     public override string GetStats()
//     {
//         return $"{Name}\n" +
//                $"${Price}\n" +
//                $"HP: {MaxHealth}\n" +
//                $"Damage: {Projectile.Damage} ({Projectile.Damage / (RateOfFire / 60)}/s)\n" +
//                $"Range: {Range}";
//     }
// }
//
// public class LightningTower : Tower
// {
//     private double _timeLastFired;
//     private TowerTemplate _template;
//     private Minion? _target;
//     
//     public LightningTower(TowerTemplate template, Team team, int x, int y) : base(template, team, x, y)
//     {
//         _template = template;
//     }
//     
//     public override void Update()
//     {
//         base.Update();
//         
//         if (Time.Scaled - _timeLastFired > 60/_template.RateOfFire)
//         {
//             switch (_template.TargetMode)
//             {
//                 case TowerTemplate.TargetSelector.Nearest:
//                     _target = FindTargetNearest();
//                     break;
//                 case TowerTemplate.TargetSelector.Random:
//                     _target = FindTargetRandom();
//                     break;
//                 default:
//                     throw new NotImplementedException();
//             }
//             
//             if (_target != null && _target.Health <= 0)
//             {
//                 _target = null;
//             }
//
//             if (_target != null)
//             {
//                 _template.Projectile.Instantiate(_target, this);
//                 _timeLastFired = Time.Scaled;
//             }
//         }
//     }
// }