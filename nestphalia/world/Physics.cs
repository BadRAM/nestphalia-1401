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
        if (!Raylib.CheckCollisionCircles(a.Position, a.Template.PhysicsRadius, b.Position, b.Template.PhysicsRadius)) return false;
        if (a.Position == b.Position) // bump away if both minions are in the exact same position
        {
            a.Push(new Vector2( 0.1f,  0.1f));
            b.Push(new Vector2(-0.1f, -0.1f));
            return true;
        }
        
        Vector2 delta = a.Position - b.Position;
        float weightRatio = a.Template.PhysicsRadius / (a.Template.PhysicsRadius + b.Template.PhysicsRadius);
        float penDepth = (a.Template.PhysicsRadius + b.Template.PhysicsRadius - delta.Length());
        a.Push(delta.Normalized() * Math.Min(penDepth * (1f-weightRatio), 30 * (float)Time.DeltaTime));
        b.Push(delta.Normalized() * Math.Min(penDepth * weightRatio * -1, 30 * (float)Time.DeltaTime));
        return true;
    }
    
    public static void CollideTerrain(Minion minion)
    {
        if (minion.IsFlying) return;
        Int2D tilePos = World.PosToTilePos(minion.Position);
        Vector2 displace = Vector2.Zero;
        Vector2 displaceCorner = Vector2.Zero;
        
        for (int x = tilePos.X-1; x < tilePos.X+3; ++x)
        {
            for (int y = tilePos.Y-1; y < tilePos.Y+3; ++y)
            {
                // guard against out of bounds, non-solid tiles, and origin Hive
                if (x < 0 || 
                    x >= World.BoardWidth || 
                    y < 0 || 
                    y >= World.BoardHeight || 
                    !(World.GetTile(x,y)?.PhysSolid() ?? false) ||
                    new Int2D(x,y) == minion.OriginTile
                    ) continue;
                Vector2 c = World.GetTileCenter(x, y);
                Rectangle b = World.GetTileBounds(x, y);
                if (!Raylib.CheckCollisionCircleRec(minion.Position, minion.Template.PhysicsRadius, b)) continue;
                {
                    if (minion.Position.X > b.X && minion.Position.X < b.X + b.Width) // circle center is in tile X band
                    {
                        // Find desired Y for above or below cases
                        displace.Y = 
                            (minion.Position.Y > c.Y ?
                                b.Y + b.Height + minion.Template.PhysicsRadius : 
                                b.Y - minion.Template.PhysicsRadius
                            ) - minion.Position.Y;
                        // _collisionOffset.Y = (float)desiredY - Position.Y;
                    }
                    else if (minion.Position.Y > b.Y && minion.Position.Y < b.Y + b.Height) // Circle Center is in tile Y band 
                    {
                        displace.X = 
                            (minion.Position.X > c.X ?
                                b.X + b.Width + minion.Template.PhysicsRadius : 
                                b.X - minion.Template.PhysicsRadius
                            ) - minion.Position.X;
                    }
                    else // Circle Center is in tile corner region
                    {
                        Vector2 corner = new Vector2
                        (
                            minion.Position.X < c.X ? b.X : b.X + b.Width,
                            minion.Position.Y < c.Y ? b.Y : b.Y + b.Height
                        );
                        Vector2 delta = minion.Position - corner;
                        displaceCorner += delta.Normalized() * (minion.Template.PhysicsRadius - delta.Length());
                    }
                }
            }
        }
        minion.Push(displace == Vector2.Zero ? displaceCorner : displace);
    }
}