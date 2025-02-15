// using System.Numerics;
//
// namespace _2_fort_cs;
//
// public abstract class TurretTargetSelector
// {
//     private bool _canTargetFlying;
//     private bool _canTargetGround;
//     private Turret _turret;
//
//     protected TurretTargetSelector(bool canTargetFlying = true, bool canTargetGround = true)
//     {
//         _canTargetFlying = canTargetFlying;
//         _canTargetGround = canTargetGround;
//     }
//
//     public abstract Minion? GetTarget();
//
//     public void SetTurret(Turret turret)
//     {
//         _turret = turret;
//     }
//
//     public class Random : TurretTargetSelector
//     {
//         public override Minion? GetTarget()
//         {
//             throw new NotImplementedException();
//         }
//     }
//     
//     public class Nearest : TurretTargetSelector
//     {
//         public override Minion? GetTarget()
//         {
//             Minion? nearest = null;
//             double minDist = double.MaxValue;
//             for (int i = 0; i < World.Minions.Count; i++)
//             {
//                 if (World.Minions[i].Team == _turret.Team) continue;
//                 double d = Vector2.Distance(World.Minions[i].Position, _turret.GetCenter());
//                 if (d < minDist)
//                 {
//                     minDist = d;
//                     nearest = World.Minions[i];
//                 }
//             }
//             
//             if (minDist < _template.Range && nearest != null)
//             {
//                 _template.Projectile.Instantiate(nearest, this);
//                 lastFireTime = Time.Scaled;
//             }
//         }
//     }
//     
//     public class FocusRandom : TurretTargetSelector
//     {
//         public override Minion? GetTarget()
//         {
//             throw new NotImplementedException();
//         }
//     }
//     
//     // public class Strongest : TurretTargetSelector
//     // public class Weakest : TurretTargetSelector
//
// }
//
