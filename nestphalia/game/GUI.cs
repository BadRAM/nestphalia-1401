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

    public static void DrawTextCentered(int x, int y, string text, float size = FontSize, Color? color = null, Vector2? anchor = null)
    {
        anchor ??= Screen.Center;
        x += (int)anchor.Value.X;
        y += (int)anchor.Value.Y;
        
        Color c = color ?? new Color(255, 255, 255, 255);
        Vector2 pos = new Vector2((int)(x-MeasureTextEx(Resources.Font, text, size, size/FontSize).X/2), (int)(y-size/2));
        DrawTextEx(Resources.Font, text, pos, size, size/FontSize, c);
    }

    public static void DrawTextLeft(int x, int y, string text, float size = FontSize, Color? color = null, Vector2? anchor = null)
    {
        anchor ??= Screen.Center;
        x += (int)anchor.Value.X;
        y += (int)anchor.Value.Y;
        
        Color c = color ?? new Color(255, 255, 255, 255);
        
        DrawTextEx(Resources.Font, text, new Vector2(x,y), size, size/FontSize, c);
    }

    public static Rectangle Draw9Slice(Texture2D texture, Rectangle rect, Vector2? anchor = null, bool draggable = false, bool resizable = false)
    {
        anchor ??= Screen.Center;
        rect.X += anchor.Value.X;
        rect.Y += anchor.Value.Y;
        
        int tileSize = texture.Width / 3;
        
        // resize
        if (resizable && IsMouseButtonDown(MouseButton.Left) && CheckCollisionPointRec(GetScaledMousePosition(), new Rectangle(GetMouseDelta() + rect.Position + rect.Size - Vector2.One * tileSize, tileSize, tileSize)))
        {
            rect.Size = Vector2.Clamp(rect.Size + GetMouseDelta(), Vector2.Zero, Vector2.PositiveInfinity);
        }
        // move
        else if (draggable && IsMouseButtonDown(MouseButton.Left) && CheckCollisionPointRec(GetScaledMousePosition(), new Rectangle(rect.Position + GetMouseDelta(), rect.Width, tileSize)))
        {
            rect.Position = Vector2.Clamp(rect.Position + GetMouseDelta(), Vector2.Zero, Screen.BottomRight - rect.Size);
        }
        
        // Draw background
        int centerWidth = ((int)rect.Width - tileSize*2) / (tileSize);
        int centerHeight = ((int)rect.Height - tileSize*2) / (tileSize);
        Rectangle texRect = new Rectangle(tileSize, tileSize, tileSize, tileSize);
        for (int x = 0; x <= centerWidth; x++)
        for (int y = 0; y <= centerHeight; y++)
        {
            texRect = new Rectangle(tileSize, tileSize, tileSize, tileSize);
            if (x == centerWidth) texRect.Width = (int)rect.Width % tileSize;
            if (y == centerHeight) texRect.Height = (int)rect.Height % tileSize;
            DrawTextureRec(texture, texRect, new Vector2(rect.X + (x+1) * tileSize, rect.Y + (y+1) * tileSize), Color.White);
        }

        // Draw Top bar
        for (int x = 0; x <= centerWidth; x++)
        {
            texRect = new Rectangle(tileSize, 0, tileSize, tileSize);
            if (x == centerWidth) texRect.Width = (int)rect.Width % tileSize;
            DrawTextureRec(texture, texRect, new Vector2(rect.X + (x+1) * tileSize, rect.Y), Color.White);
        }
        
        // Draw Bottom bar
        for (int x = 0; x <= centerWidth; x++)
        {
            texRect = new Rectangle(tileSize, tileSize*2, tileSize, tileSize);
            if (x == centerWidth) texRect.Width = (int)rect.Width % tileSize;
            DrawTextureRec(texture, texRect, new Vector2(rect.X + (x+1) * tileSize, rect.Y + rect.Height - tileSize), Color.White);
        }
        
        // Draw Left bar
        for (int y = 0; y <= centerHeight; y++)
        {
            texRect = new Rectangle(0, tileSize, tileSize, tileSize);
            if (y == centerHeight) texRect.Height = (int)rect.Height % tileSize;
            DrawTextureRec(texture, texRect, new Vector2(rect.X, rect.Y + (y+1) * tileSize), Color.White);
        }
        
        // Draw Right bar
        for (int y = 0; y <= centerHeight; y++)
        {
            texRect = new Rectangle(tileSize*2, tileSize, tileSize, tileSize);
            if (y == centerHeight) texRect.Height = (int)rect.Height % tileSize;
            DrawTextureRec(texture, texRect, new Vector2(rect.X + rect.Width - tileSize, rect.Y + (y+1) * tileSize), Color.White);
        }

        // Draw top left corner
        texRect = new Rectangle(0, 0, tileSize, tileSize);
        DrawTextureRec(texture, texRect, rect.Position, Color.White);
        
        // Draw top right corner
        texRect.X = tileSize*2; 
        DrawTextureRec(texture, texRect, rect.Position + Vector2.UnitX * (rect.Width-tileSize), Color.White);
        
        // Draw bottom right corner
        texRect.Y = tileSize*2;
        DrawTextureRec(texture, texRect, rect.Position + Vector2.UnitX * (rect.Width-tileSize) + Vector2.UnitY * (rect.Height-tileSize), Color.White);
        
        // Draw bottom left corner
        texRect.X = 0; 
        DrawTextureRec(texture, texRect, rect.Position + Vector2.UnitY * (rect.Height-tileSize), Color.White);
        
        rect.X -= anchor.Value.X;
        rect.Y -= anchor.Value.Y;
        
        return rect;
    }
    
    // 300 pixel wide button
    public static bool Button300(int x, int y, string text, bool enabled = true, Vector2? anchor = null)
    {
        anchor ??= Screen.Center;
        x += (int)anchor.Value.X;
        y += (int)anchor.Value.Y;
        
        bool hover = CheckCollisionPointRec(GetScaledMousePosition(), new Rectangle(x, y, 300, 40));
        bool press = !enabled || (hover && (IsMouseButtonDown(MouseButton.Left) || IsMouseButtonReleased(MouseButton.Left)));
        
        Rectangle subSprite = new Rectangle(0, !press ? !hover ? 0 : 40 : 80, 300, 40);
        DrawTextureRec(_buttonWideTexture, subSprite, new Vector2(x,y), Color.White);
        DrawTextCentered(x+150, y+20, text, anchor: Vector2.Zero);

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
    public static bool Button100(int x, int y, string text, bool enabled = true, Vector2? anchor = null)
    {
        anchor ??= Screen.Center;
        x += (int)anchor.Value.X;
        y += (int)anchor.Value.Y;
        
        bool hover = CheckCollisionPointRec(GetScaledMousePosition(), new Rectangle(x, y, 100, 40));
        bool press = !enabled || (hover && (IsMouseButtonDown(MouseButton.Left) || IsMouseButtonReleased(MouseButton.Left)));
        
        Rectangle subSprite = new Rectangle(0, !press ? !hover ? 0 : 40 : 80, 100, 40);
        DrawTextureRec(_buttonNarrowTexture, subSprite, new Vector2(x,y), Color.White);
        DrawTextCentered(x+50, y+20, text, anchor: Vector2.Zero);
        
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
    
    public static bool ButtonImage(int x, int y, Texture2D image, bool selected = false, Vector2? anchor = null)
    {
        anchor ??= Screen.Center;
        x += (int)anchor.Value.X;
        y += (int)anchor.Value.Y;
        
        bool hover = CheckCollisionPointRec(GetScaledMousePosition(), new Rectangle(x, y, image.Width, image.Height));

        if (selected) DrawRectangle(x-2, y-2, image.Width + 4, image.Height + 4, Color.White);
        DrawTexture(image, x, y, Color.White);

        if (hover)
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
    
    public static bool TileButton(int x, int y, Texture2D image, bool selected = false, Vector2? anchor = null)
    {
        anchor ??= Screen.Center;
        x += (int)anchor.Value.X;
        y += (int)anchor.Value.Y;
        
        bool hover = CheckCollisionPointRec(GetScaledMousePosition(), new Rectangle(x, y, 24, 24));

        if (selected) DrawRectangle(x-2, y-2, 28, 28, Color.White);
        DrawTexture(image, x, y+24 - image.Height, Color.White);

        if (hover)
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

    public static string TextEntry(int x, int y, string text, Vector2? anchor = null)
    {
        anchor ??= Screen.Center;
        x += (int)anchor.Value.X;
        y += (int)anchor.Value.Y;
        
        bool hover = CheckCollisionPointRec(GetScaledMousePosition(), new Rectangle(x, y, 300, 40));
        
        Rectangle subSprite = new Rectangle(0, !hover ? 0 : 40, 300, 40);
        DrawTextureRec(_buttonWideTexture, subSprite, new Vector2(x,y), Color.White);
        DrawTextCentered(x+150, y+20, text + (hover ? "_" : ""), anchor: Vector2.Zero);
        
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
    
    public static string BigTextCopyPad(int x, int y, string label, string text, Vector2? anchor = null)
    {
        anchor ??= Screen.Center;
        x += (int)anchor.Value.X;
        y += (int)anchor.Value.Y;
        
        bool hover = CheckCollisionPointRec(GetScaledMousePosition(), new Rectangle(x, y, 300, 40));

        if (hover)
        {
            _cursorLook = MouseCursor.PointingHand;
            
            DrawRectangle(Screen.CenterX, Screen.CenterY - 300, 600, 600, ColorAlpha(Color.Black, 0.5f));
            DrawTextLeft(2, -298, text);

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
        DrawTextCentered(x+150, y+20, label, anchor: Vector2.Zero);
        
        return text;
    }
    
    public static void UpdateCursor()
    {
        SetMouseCursor(_cursorLook);
        _cursorLook = MouseCursor.Default;
    }
}