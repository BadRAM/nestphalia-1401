using ZeroElectric.Vinculum;
using static ZeroElectric.Vinculum.Raylib;

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
            if (IsMouseButtonPressed(MouseButton.MOUSE_BUTTON_LEFT))
            {
                return true;
            }
        }
        return false;
    }

    public void Draw()
    {
        DrawRectangleRec(Bounds, DARKGREEN);
        DrawText(Label, (int)Bounds.X, (int)Bounds.Y, 20, WHITE);
    }
}