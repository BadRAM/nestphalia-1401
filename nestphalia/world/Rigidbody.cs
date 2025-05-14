using System.Numerics;
using Raylib_cs;

namespace nestphalia;

public class Rigidbody
{
    public Minion? Owner = null;
    public Vector2 Position;
    public float Radius;
    public bool IsFlying;
    private Vector2 _collisionOffset;
    private Int2D _collideOverride;
    
    public Rigidbody(Minion owner, Vector2 position, float radius, bool isFlying)
    {
        Owner = owner;
        Position = position;
        Radius = radius;
        IsFlying = isFlying;
        _collideOverride = World.PosToTilePos(position);
    }

    public Rigidbody()
    {
        Owner = null;
    }


    // Should this be here, or in World? maybe somewhere else entirely, like a physics functions class?
    public void CollideRigidbody(Rigidbody other)
    {
        if (other.IsFlying != IsFlying) return;
        if (!Raylib.CheckCollisionCircles(Position, Radius, other.Position, other.Radius)) return;
        if (Position == other.Position) // jostle randomly if both minions are in the exact same position
        {
                  _collisionOffset += new Vector2((float)(World.Random.NextDouble() - 0.5), (float)(World.Random.NextDouble() - 0.5));
            other._collisionOffset += new Vector2((float)(World.Random.NextDouble() - 0.5), (float)(World.Random.NextDouble() - 0.5));
            return;
        }
        
        Vector2 delta = Position - other.Position;
        float weightRatio = Radius / (Radius + other.Radius);
        float penDepth = (Radius + other.Radius - delta.Length());
              _collisionOffset += delta.Normalized() * Math.Min(penDepth * (1f-weightRatio), 0.5f);
        other._collisionOffset -= delta.Normalized() * Math.Min(penDepth * (weightRatio),    0.5f);
    }
    
    public void CollideTerrain()
    {
        if (IsFlying) return;
        Int2D tilePos = World.PosToTilePos(Position);
        
        for (int x = tilePos.X-1; x < tilePos.X+3; ++x)
        {
            for (int y = tilePos.Y-1; y < tilePos.Y+3; ++y)
            {
                // guard against out of bounds, non-solid tiles, and source hive
                if (x < 0 || 
                    x >= World.BoardWidth || 
                    y < 0 || 
                    y >= World.BoardHeight || 
                    !(World.GetTile(x,y)?.PhysSolid() ?? false) ||
                    (_collideOverride.EqualsCoords(x,y))
                    ) continue;
                Vector2 c = World.GetTileCenter(x, y);
                Rectangle b = World.GetTileBounds(x, y);
                if (!Raylib.CheckCollisionCircleRec(Position, Radius, b)) continue;
                {
                    if (Position.X > b.X && Position.X < b.X + b.Width) // circle center is in tile X band
                    {
                        // Find desired Y for above or below cases
                        double desiredY = Position.Y > c.Y ? b.Y + b.Height + Radius : b.Y - Radius;
                        _collisionOffset.Y = (float)desiredY - Position.Y;
                    }
                    else if (Position.Y > b.Y && Position.Y < b.Y + b.Height) // Circle Center is in tile Y band 
                    {
                        double desiredX = Position.X > c.X ? b.X + b.Width + Radius : b.X - Radius;
                        _collisionOffset.X = (float)desiredX - Position.X;
                    }
                    // else // Circle Center is in tile corner region
                    // {
                    //     Vector2 corner = new Vector2
                    //     (
                    //         Position.X > c.X ? b.X : b.X + b.Width,
                    //         Position.Y > c.Y ? b.Y : b.Y + b.Height
                    //     );
                    //     Vector2 delta = Position - corner;
                    //     _collisionOffset += delta.Normalized() * (Template.PhysicsRadius - delta.Length());
                    // }
                }
            }
        }
    }
    
    public void LateUpdate()
    {
        Position += _collisionOffset;
        _collisionOffset = Vector2.Zero;
    }

    public void SetCollideOverride(Int2D coords)
    {
        _collideOverride = coords;
    }


}