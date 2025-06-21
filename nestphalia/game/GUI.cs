using System.Numerics;
using Raylib_cs;
using static Raylib_cs.Raylib;

namespace nestphalia;

public static class GUI
{
    private static Texture2D _buttonWideTexture;
    private static Texture2D _buttonNarrowTexture;
    private static SoundResource _buttonClickSFX;
    private static MouseCursor _cursorLook;
    public static Font Font;
    const int FontSize = 16;
    
    public static void Initialize()
    {
        _buttonWideTexture = Resources.GetTextureByName("button_wide");
        _buttonNarrowTexture = Resources.GetTextureByName("button_narrow");
        _buttonClickSFX = Resources.GetSoundByName("shovel");
    }

    public static Vector2 GetScaledMousePosition()
    {
        // return GetMousePosition();
        return GetMousePosition() / GetWindowScale();
    }

    public static Vector2 GetWindowScale()
    {
        if (Settings.Saved.WindowScale)
        {
            return GetWindowScaleDPI();
        }
        return Vector2.One;
    }

    public static void DrawTextCentered(int x, int y, string text, float size = FontSize, Color? color = null, bool guiSpace = true)
    {
        if (guiSpace)
        {
            x += Screen.HCenter;
            y += Screen.VCenter;
        }
        
        Color c = color ?? new Color(255, 255, 255, 255);
        Vector2 pos = new Vector2((int)(x-MeasureTextEx(Resources.Font, text, size, size/FontSize).X/2), (int)(y-size/2));
        DrawTextEx(Resources.Font, text, pos, size, size/FontSize, c);
    }

    public static void DrawTextLeft(int x, int y, string text, float size = FontSize, Color? color = null, bool guiSpace = true)
    {
        if (guiSpace)
        {
            x += Screen.HCenter;
            y += Screen.VCenter;
        }
        
        Color c = color ?? new Color(255, 255, 255, 255);
        
        DrawTextEx(Resources.Font, text, new Vector2(x,y), size, size/FontSize, c);
    }
    
    // 300 pixel wide button
    public static bool Button300(int x, int y, string text, bool enabled = true, bool guiSpace = true)
    {
        if (guiSpace)
        {
            x += Screen.HCenter;
            y += Screen.VCenter;
        }
        
        bool hover = CheckCollisionPointRec(GetScaledMousePosition(), new Rectangle(x, y, 300, 40));
        bool press = !enabled || (hover && (IsMouseButtonDown(MouseButton.Left) || IsMouseButtonReleased(MouseButton.Left)));
        
        Rectangle subSprite = new Rectangle(0, !press ? !hover ? 0 : 40 : 80, 300, 40);
        DrawTextureRec(_buttonWideTexture, subSprite, new Vector2(x,y), Color.White);
        DrawTextCentered(x+150, y+20, text, guiSpace:false);

        if (enabled && hover)
        {
            _cursorLook = MouseCursor.PointingHand;
            if (IsMouseButtonReleased(MouseButton.Left))
            {
                _buttonClickSFX.Play();
                return true;
            }
        }
        
        return false;
    }
    
    // 100 pixel wide button
    public static bool Button100(int x, int y, string text, bool enabled = true, bool guiSpace = true)
    {
        if (guiSpace)
        {
            x += Screen.HCenter;
            y += Screen.VCenter;
        }
        
        bool hover = CheckCollisionPointRec(GetScaledMousePosition(), new Rectangle(x, y, 100, 40));
        bool press = !enabled || (hover && (IsMouseButtonDown(MouseButton.Left) || IsMouseButtonReleased(MouseButton.Left)));
        
        Rectangle subSprite = new Rectangle(0, !press ? !hover ? 0 : 40 : 80, 100, 40);
        DrawTextureRec(_buttonNarrowTexture, subSprite, new Vector2(x,y), Color.White);
        DrawTextCentered(x+50, y+20, text, guiSpace:false);
        
        if (enabled && hover)
        {
            _cursorLook = MouseCursor.PointingHand;
            if (IsMouseButtonReleased(MouseButton.Left))
            {
                _buttonClickSFX.Play();
                return true;
            }
        }
        return false;
    }

    public static string TextEntry(int x, int y, string text, bool guiSpace = true)
    {
        if (guiSpace)
        {
            x += Screen.HCenter;
            y += Screen.VCenter;
        }
        
        bool hover = CheckCollisionPointRec(GetScaledMousePosition(), new Rectangle(x, y, 300, 40));
        
        Rectangle subSprite = new Rectangle(0, !hover ? 0 : 40, 300, 40);
        DrawTextureRec(_buttonWideTexture, subSprite, new Vector2(x,y), Color.White);
        DrawTextCentered(x+150, y+20, text + (hover ? "_" : ""), guiSpace:false);
        
        if (hover)
        {
            // Set the window's cursor to the I-Beam
            _cursorLook = MouseCursor.IBeam;
    
            // Get char pressed (unicode character) on the queue
            int key = GetCharPressed();
    
            // Check if more characters have been pressed on the same frame
            while (key > 0)
            {
                // NOTE: Only allow keys in range [32..125]
                if ((key >= 32) && (key <= 125))
                {
                    text += (char)key;
                }
                key = GetCharPressed();  // Check next character in the queue
            }
    
            if ((IsKeyPressed(KeyboardKey.Backspace) || IsKeyPressedRepeat(KeyboardKey.Backspace)) && text.Length > 0)
            {
                text = text.Substring(0, text.Length - 1);
            }
        }
        return text;
    }
    
    public static string BigTextCopyPad(int x, int y, string label, string text, bool guiSpace = true)
    {
        if (guiSpace)
        {
            x += Screen.HCenter;
            y += Screen.VCenter;
        }
        
        bool hover = CheckCollisionPointRec(GetScaledMousePosition(), new Rectangle(x, y, 300, 40));

        if (hover)
        {
            _cursorLook = MouseCursor.PointingHand;
            
            DrawRectangle(Screen.HCenter - 600, Screen.VCenter - 300, 600, 600, ColorAlpha(Color.Black, 0.5f));
            DrawTextLeft(-598, -298, text);

            if (IsMouseButtonPressed(MouseButton.Left))
            {
                text = GetClipboardText_();
            }

            if (IsMouseButtonPressed(MouseButton.Right))
            {
                SetClipboardText(text);
            }
        }
        
        Rectangle subSprite = new Rectangle(0, !hover ? 0 : 40, 300, 40);
        DrawTextureRec(_buttonWideTexture, subSprite, new Vector2(x,y), Color.White);
        DrawTextCentered(x+150, y+20, label, guiSpace:false);
        
        return text;
    }
    
    public static void UpdateCursor()
    {
        SetMouseCursor(_cursorLook);
        _cursorLook = MouseCursor.Default;
    }
}