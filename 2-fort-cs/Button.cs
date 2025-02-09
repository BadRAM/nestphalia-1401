using Raylib_cs;
using static Raylib_cs.Raylib;

namespace _2_fort_cs;

public class Button
{
    public string Label;
    
    public Rectangle Bounds;
    public bool Pressed;

    public bool Update()
    {
        if (CheckCollisionPointRec(GetMousePosition(), Bounds))
        {
            if (IsMouseButtonPressed(MouseButton.Left))
            {
                return true;
            }
        }
        return false;
    }

    public void Draw()
    {
        DrawRectangleRec(Bounds, Color.DarkGreen);
        DrawText(Label, (int)Bounds.X, (int)Bounds.Y, 20, Color.White);
    }
}