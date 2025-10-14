using System.Diagnostics;
using System.Numerics;
using Raylib_cs;

namespace nestphalia;

public static class Physics
{
    // Should this be here, or in World? maybe somewhere else entirely, like a physics functions class?
    public static bool CollideMinion(Minion a, Minion b)
    {
        Debug.Assert(b != a);
        
        if (b.IsFlying != a.IsFlying) return false;
        if (!Raylib.CheckCollisionCircles(a.Position.XY(), a.Template.PhysicsRadius, b.Position.XY(), b.Template.PhysicsRadius)) return false;
        if (a.Position == b.Position) // bump away if both minions are in the exact same position
        {
            a.Push(new Vector2( 0.1f,  0.1f));
            b.Push(new Vector2(-0.1f, -0.1f));
            return true;
        }
        
        Vector2 delta = a.Position.XY() - b.Position.XY();
        float weightRatio = a.Template.PhysicsRadius / (a.Template.PhysicsRadius + b.Template.PhysicsRadius);
        float penDepth = (a.Template.PhysicsRadius + b.Template.PhysicsRadius - delta.Length());
        a.Push(delta.Normalized() * Math.Min(penDepth * (1f-weightRatio), 30 * (float)Time.DeltaTime));
        b.Push(delta.Normalized() * Math.Min(penDepth * weightRatio * -1, 30 * (float)Time.DeltaTime));
        return true;
    }
    
    public static void CollideTerrain(Minion minion)
    {
        if (minion.Position.Z >= 8 || (minion.State is Minion.Jump jump && jump.Height > 8) ) return;
        Int2D tilePos = World.PosToTilePos(minion.Position);
        int x = tilePos.X;
        int y = tilePos.Y;
        Vector2 tileCenter = World.GetTileCenter(x, y);
        
        bool n =  (World.GetTile(x  , y-1)?.PhysSolid(minion) ?? false) || y <= 0;
        bool s =  (World.GetTile(x  , y+1)?.PhysSolid(minion) ?? false) || y >= World.BoardHeight-1;
        bool w =  (World.GetTile(x-1, y  )?.PhysSolid(minion) ?? false) || x <= 0;
        bool e =  (World.GetTile(x+1, y  )?.PhysSolid(minion) ?? false) || x >= World.BoardWidth-1;
        bool nw = (World.GetTile(x-1, y-1)?.PhysSolid(minion) ?? false);
        bool ne = (World.GetTile(x+1, y-1)?.PhysSolid(minion) ?? false);
        bool sw = (World.GetTile(x-1, y+1)?.PhysSolid(minion) ?? false);
        bool se = (World.GetTile(x+1, y+1)?.PhysSolid(minion) ?? false);
        bool c =  (World.GetTile(x  , y  )?.PhysSolid(minion) ?? false);
        
        // Handle being on top of a wall
        if (minion.IsOnTopOfStructure)
        {
            if (!c)
            {
                minion.IsOnTopOfStructure = false;
            }
            else
            {
                return;
            }
        }

        // Handle being clipped inside a wall
        if (c)
        {
            List<Vector2> ejectPos = new List<Vector2>();
            if (!n) ejectPos.Add(new Vector2(minion.Position.X, (tileCenter.Y - 12) - minion.Template.PhysicsRadius));
            if (!s) ejectPos.Add(new Vector2(minion.Position.X, (tileCenter.Y + 12) + minion.Template.PhysicsRadius));
            if (!w) ejectPos.Add(new Vector2((tileCenter.X - 12) - minion.Template.PhysicsRadius, minion.Position.Y));
            if (!e) ejectPos.Add(new Vector2((tileCenter.X + 12) + minion.Template.PhysicsRadius, minion.Position.Y));
            if (ejectPos.Count == 0)
            {
                minion.IsOnTopOfStructure = true;
                // Console.WriteLine($"{minion.Template.Name} is trapped inside a wall with no way out! wall collision has been skipped.");
                return;
            }

            Vector2 minDisplacementEject = ejectPos[0];
            for (int i = 1; i < ejectPos.Count; i++)
            {
                if ((minion.Position.XY() - ejectPos[i]).Length() < (minion.Position.XY() - minDisplacementEject).Length())
                {
                    minDisplacementEject = ejectPos[i];
                }
            }
            minion.Position = minDisplacementEject.XYZ();
            return;
        }
        
        // Handle wall collision
        if (n && minion.Position.Y - minion.Template.PhysicsRadius < tileCenter.Y - 12)
        {
            minion.Position.Y = (tileCenter.Y - 12) + minion.Template.PhysicsRadius;
        }
        else if (s && minion.Position.Y + minion.Template.PhysicsRadius > tileCenter.Y + 12)
        {
            minion.Position.Y = (tileCenter.Y + 12) - minion.Template.PhysicsRadius;
        }
        if (w && minion.Position.X - minion.Template.PhysicsRadius < tileCenter.X - 12)
        {
            minion.Position.X = (tileCenter.X - 12) + minion.Template.PhysicsRadius;
        }
        else if (e && minion.Position.X + minion.Template.PhysicsRadius > tileCenter.X + 12)
        {
            minion.Position.X = (tileCenter.X + 12) - minion.Template.PhysicsRadius;
        }

        // Handle corner collision
        if (nw && !n && !w && Raylib.CheckCollisionPointCircle(tileCenter + new Vector2(-12, -12), minion.Position.XY(), minion.Template.PhysicsRadius))
        {
            Vector3 delta = minion.Position - (tileCenter.XYZ() + new Vector3(-12, -12, 0));
            minion.Position += delta.Normalized() * (minion.Template.PhysicsRadius - delta.Length());
        }
        if (ne && !n && !e && Raylib.CheckCollisionPointCircle(tileCenter + new Vector2(12, -12), minion.Position.XY(), minion.Template.PhysicsRadius))
        {
            Vector3 delta = minion.Position - (tileCenter.XYZ() + new Vector3(12, -12, 0));
            minion.Position += delta.Normalized() * (minion.Template.PhysicsRadius - delta.Length());
        }
        if (sw && !s && !w && Raylib.CheckCollisionPointCircle(tileCenter + new Vector2(-12, 12), minion.Position.XY(), minion.Template.PhysicsRadius))
        {
            Vector3 delta = minion.Position - (tileCenter.XYZ() + new Vector3(-12, 12, 0));
            minion.Position += delta.Normalized() * (minion.Template.PhysicsRadius - delta.Length());
        }
        if (se && !s && !e && Raylib.CheckCollisionPointCircle(tileCenter + new Vector2(12, 12), minion.Position.XY(), minion.Template.PhysicsRadius))
        {
            Vector3 delta = minion.Position - (tileCenter.XYZ() + new Vector3(12, 12, 0));
            minion.Position += delta.Normalized() * (minion.Template.PhysicsRadius - delta.Length());
        }
    }
}