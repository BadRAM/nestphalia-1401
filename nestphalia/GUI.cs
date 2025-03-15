using System.Numerics;
using Raylib_cs;
using static Raylib_cs.Raylib;

namespace nestphalia;

public static class GUI
{
    private static Texture2D _buttonWideTexture;
    private static Texture2D _buttonNarrowTexture;
    private static SoundResource _buttonClickSFX;
    public static Font Font;
    const int FontSize = 16;
    
    public static void Initialize()
    {
        _buttonWideTexture = Resources.GetTextureByName("button_wide");
        _buttonNarrowTexture = Resources.GetTextureByName("button_narrow");
        _buttonClickSFX = Resources.GetSoundByName("shovel");
    }

    public static void DrawTextCentered(int x, int y, string text, float size = FontSize, Color? color = null)
    {
        Color c = color ?? new Color(255, 255, 255, 255);
        
        Vector2 pos = new Vector2((int)(x-MeasureTextEx(Resources.Font, text, size, size/FontSize).X/2), (int)(y-size/2));
        
        DrawTextEx(Resources.Font, text, pos, size, size/FontSize, c);
    }

    public static void DrawTextLeft(int x, int y, string text, float size = FontSize, Color? color = null)
    {
        Color c = color ?? new Color(255, 255, 255, 255);
        
        DrawTextEx(Resources.Font, text, new Vector2(x,y), size, size/FontSize, c);
    }
    
    public static bool ButtonWide(int x, int y, string text, bool enabled = true)
    {
        bool hover = CheckCollisionPointRec(GetMousePosition(), new Rectangle(x, y, 300, 40));
        bool press = !enabled || (hover && (IsMouseButtonDown(MouseButton.Left) || IsMouseButtonReleased(MouseButton.Left)));
        
        Rectangle subSprite = new Rectangle(0, !press ? !hover ? 0 : 40 : 80, 300, 40);
        DrawTextureRec(_buttonWideTexture, subSprite, new Vector2(x,y), Color.White);
        DrawTextCentered(x+150, y+20, text);
        
        if (enabled && hover && IsMouseButtonReleased(MouseButton.Left))
        {
            _buttonClickSFX.Play();
            return true;
        }
        return false;
    }
    
    public static bool ButtonNarrow(int x, int y, string text, bool enabled = true)
    {
        bool hover = CheckCollisionPointRec(GetMousePosition(), new Rectangle(x, y, 100, 40));
        bool press = !enabled || (hover && (IsMouseButtonDown(MouseButton.Left) || IsMouseButtonReleased(MouseButton.Left)));
        
        Rectangle subSprite = new Rectangle(0, !press ? !hover ? 0 : 40 : 80, 100, 40);
        DrawTextureRec(_buttonNarrowTexture, subSprite, new Vector2(x,y), Color.White);
        DrawTextCentered(x+50, y+20, text);
        
        if (enabled && hover && IsMouseButtonReleased(MouseButton.Left))
        {
            _buttonClickSFX.Play();
            return true;
        }
        return false;    }
}