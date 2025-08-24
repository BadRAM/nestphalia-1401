using System.Numerics;
using Raylib_cs;
using static Raylib_cs.Raylib;

namespace nestphalia;

public static class GUI
{
    private static Texture2D _buttonWideTexture;
    private static Texture2D _buttonNarrowTexture;
    private static Texture2D _button360Texture;
    private static Texture2D _button270Texture;
    private static Texture2D _button180Texture;
    private static Texture2D _button90Texture;
    private static Texture2D _button36Texture;
    private static Texture2D _sliderBarTexture;
    private static Texture2D _sliderPinTexture;
    private static SoundResource _buttonClickSFX;
    private static MouseCursor _cursorLook;
    public static Font Font;
    const int FontSize = 16;
    
    public enum ButtonState
    {
        Disabled,
        Enabled,
        Hovered,
        Pressed
    }
    
    public static void Initialize()
    {
        _buttonWideTexture = Resources.GetTextureByName("button_wide");
        _buttonNarrowTexture = Resources.GetTextureByName("button_narrow");
        _button360Texture = Resources.GetTextureByName("button_360");
        _button270Texture = Resources.GetTextureByName("button_270");
        _button180Texture = Resources.GetTextureByName("button_180");
        _button90Texture = Resources.GetTextureByName("button_90");
        _button36Texture = Resources.GetTextureByName("button_36");
        _sliderBarTexture = Resources.GetTextureByName("slider_bar");
        _sliderPinTexture = Resources.GetTextureByName("slider_pin");
        _buttonClickSFX = Resources.GetSoundByName("shovel");
    }

    public static Vector2 GetScaledMousePosition()
    {
        // return GetMousePosition();
        return GetMousePosition() / GetWindowScale();
    }

    public static Vector2 GetScaledMouseDelta()
    {
        return GetMouseDelta() / GetWindowScale();
    }

    public static Vector2 GetWindowScale()
    {
        return (float)Settings.Saved.WindowScale * Vector2.One;
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

    public static void DrawTextLeft(int x, int y, string text, float size = FontSize, int wrapWidth = 0, Color? color = null, Vector2? anchor = null)
    {
        anchor ??= Screen.Center;
        x += (int)anchor.Value.X;
        y += (int)anchor.Value.Y;
        
        Color c = color ?? new Color(255, 255, 255, 255);

        if (wrapWidth != 0)
        {
            text = WrapText(text, wrapWidth, size);
        }
        
        DrawTextEx(Resources.Font, text, new Vector2(x,y), size, size/FontSize, c);
    }

    public static void DrawTextLeft(Vector2 pos, string text, float size = FontSize, int wrapWidth = 0, Color? color = null, Vector2? anchor = null)
    {
        DrawTextLeft((int)pos.X, (int)pos.Y, text, size, wrapWidth, color, anchor);
    }

    public static string WrapText(string text, int wrapWidth, float size = FontSize)
    {
        List<string> lines = new List<string>(text.Split("\n"));
        for (int i = 0; i < lines.Count; i++)
        {
            if (MeasureTextEx(Resources.Font, lines[i], size, size / FontSize).X > wrapWidth)
            {
                List<string> words = new List<string>(lines[i].Split(" "));
                if (MeasureTextEx(Resources.Font, words[0], size, size / FontSize).X > wrapWidth) // if the first word is too long and needs to be cut
                {
                    for (int j = 2; j < lines[i].Length; j++)
                    {
                        if (MeasureTextEx(Resources.Font, lines[i].Substring(0, j), size, size / FontSize).X > wrapWidth)
                        {
                            lines.Insert(i + 1, lines[i].Substring(j-1).Trim());
                            lines[i] = lines[i].Substring(0, j-1);
                            break;
                        }
                    }
                    continue;
                }
                for (int j = 1; j < words.Count; j++)
                {
                    if (MeasureTextEx(Resources.Font, string.Join(" ", words.GetRange(0, j+1)), size, size / FontSize).X > wrapWidth)
                    {
                        lines.Insert(i + 1, string.Join(" ", words.GetRange(j, words.Count - j)).Trim());
                        lines[i] = string.Join(" ", words.GetRange(0, j)).Trim();
                        break;
                    }
                }
            }
        }
        return string.Join("\n", lines);
    }

    public static Vector2 MeasureText(string text, float size = FontSize)
    {
        return MeasureTextEx(Resources.Font, text, size, size / FontSize);
    }

    public static Rectangle Draw9Slice(Texture2D texture, Rectangle rect, Vector2? anchor = null, bool draggable = false, bool resizable = false)
    {
        anchor ??= Screen.Center;
        rect.X += anchor.Value.X;
        rect.Y += anchor.Value.Y;
        
        int tileSize = texture.Width / 3;
        
        // resize
        if (resizable && Input.Held(MouseButton.Left) && CheckCollisionPointRec(GetScaledMousePosition(), new Rectangle(GetScaledMouseDelta() + rect.Position + rect.Size - Vector2.One * tileSize, tileSize, tileSize)))
        {
            rect.Size = Vector2.Clamp(rect.Size + GetScaledMouseDelta(), Vector2.Zero, Vector2.PositiveInfinity);
        }
        // move
        else if (draggable && Input.Held(MouseButton.Left) && CheckCollisionPointRec(GetScaledMousePosition(), new Rectangle(rect.Position + GetScaledMouseDelta(), rect.Width, tileSize)))
        {
            rect.Position = Vector2.Clamp(rect.Position + GetScaledMouseDelta(), Vector2.Zero, Screen.BottomRight - rect.Size);
        }
        
        // Draw background
        int centerWidth = ((int)rect.Width - tileSize*2) / (tileSize);
        int centerHeight = ((int)rect.Height - tileSize*2) / (tileSize);
        Rectangle texRect = new Rectangle(tileSize, tileSize, tileSize, tileSize);
        for (int x = 0; x <= centerWidth; x++)
        for (int y = 0; y <= centerHeight; y++)
        {
            texRect = new Rectangle(tileSize, tileSize, tileSize, tileSize);
            if (x == centerWidth) texRect.Width = (int)(rect.Width % tileSize)+1;
            if (y == centerHeight) texRect.Height = (int)(rect.Height % tileSize)+1;
            DrawTextureRec(texture, texRect, new Vector2(rect.X + (x+1) * tileSize, rect.Y + (y+1) * tileSize), Color.White);
        }

        // Draw Top bar
        for (int x = 0; x <= centerWidth; x++)
        {
            texRect = new Rectangle(tileSize, 0, tileSize, tileSize);
            if (x == centerWidth) texRect.Width = (int)(rect.Width % tileSize)+1;
            DrawTextureRec(texture, texRect, new Vector2(rect.X + (x+1) * tileSize, rect.Y), Color.White);
        }
        
        // Draw Bottom bar
        for (int x = 0; x <= centerWidth; x++)
        {
            texRect = new Rectangle(tileSize, tileSize*2, tileSize, tileSize);
            if (x == centerWidth) texRect.Width = (int)(rect.Width % tileSize)+1;
            DrawTextureRec(texture, texRect, new Vector2(rect.X + (x+1) * tileSize, rect.Y + rect.Height - tileSize), Color.White);
        }
        
        // Draw Left bar
        for (int y = 0; y <= centerHeight; y++)
        {
            texRect = new Rectangle(0, tileSize, tileSize, tileSize);
            if (y == centerHeight) texRect.Height = (int)(rect.Height % tileSize)+1;
            DrawTextureRec(texture, texRect, new Vector2(rect.X, rect.Y + (y+1) * tileSize), Color.White);
        }
        
        // Draw Right bar
        for (int y = 0; y <= centerHeight; y++)
        {
            texRect = new Rectangle(tileSize*2, tileSize, tileSize, tileSize);
            if (y == centerHeight) texRect.Height = (int)(rect.Height % tileSize)+1;
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

    public static bool Button(Rectangle rect, string text, Texture2D? texture = null, bool enabled = true, Vector2? anchor = null)
    {
        anchor ??= Screen.Center;
        rect.X += (int)anchor.Value.X;
        rect.Y += (int)anchor.Value.Y;

        ButtonState state;
        if (!enabled)
        {
            state = ButtonState.Disabled;
        }
        else if (Input.IsSuppressed()) // Ignore everything if input is suppressed
        {
            state = ButtonState.Enabled;
        }
        else if (Input.Held(MouseButton.Left) || Input.Released(MouseButton.Left)) // If click is held, button must be clicked or unhoverable
        {
            if (CheckCollisionPointRec(Input.GetScaledClickPos(), rect) && CheckCollisionPointRec(GetScaledMousePosition(), rect))
                state = ButtonState.Pressed;
            else
                state = ButtonState.Enabled;
        }
        else // Check if we need to show hover
        {
            if (CheckCollisionPointRec(GetScaledMousePosition(), rect))
                state = ButtonState.Hovered;
            else
                state = ButtonState.Enabled;
        }
        
        if (texture != null)
        {
            Rectangle subSprite = new Rectangle(0, 0, rect.Size);
            switch (state)
            {
                case ButtonState.Disabled:
                    subSprite.Y = rect.Height * 2;
                    break;
                case ButtonState.Enabled:
                    subSprite.Y = 0;
                    break;
                case ButtonState.Hovered:
                    subSprite.Y = rect.Height * 1;
                    break;
                case ButtonState.Pressed:
                    subSprite.Y = rect.Height * 2;
                    break; 
            }
            DrawTextureRec(texture.Value, subSprite, rect.Position, Color.White);
        }
        else
        {
            Color color = Color.White;
            switch (state)
            {
                case ButtonState.Disabled:
                    color = Color.DarkBrown;
                    break;
                case ButtonState.Enabled:
                    color = Color.Brown;
                    break;
                case ButtonState.Hovered:
                    color = Color.Orange;
                    break;
                case ButtonState.Pressed:
                    color = Color.DarkBrown;
                    break; 
            }
            DrawRectangleRec(rect, color);
            DrawRectangleLinesEx(rect, 2, Color.Black);
        }

        DrawTextCentered((int)(rect.X + rect.Width / 2), (int)(rect.Y + rect.Height/2), text, anchor: Vector2.Zero);

        if (state == ButtonState.Hovered)
        {
            _cursorLook = MouseCursor.PointingHand;
        }
        
        if (state == ButtonState.Pressed)
        {
            _cursorLook = MouseCursor.PointingHand;
            if (Input.Released(MouseButton.Left))
            {
                _buttonClickSFX.Play();
                return true;
            }
        }
        
        return false;
    }
    
    // 360x36 pixel button
    public static bool Button360(int x, int y, string text, bool enabled = true, Vector2? anchor = null)
    {
        return Button(new Rectangle(x, y, 360, 36), text, _button360Texture, enabled, anchor);
    }
    
    // 270x36 pixel button
    public static bool Button270(int x, int y, string text, bool enabled = true, Vector2? anchor = null)
    {
        return Button(new Rectangle(x, y, 270, 36), text, _button270Texture, enabled, anchor);
    }
    
    // 180x36 pixel button
    public static bool Button180(int x, int y, string text, bool enabled = true, Vector2? anchor = null)
    {
        return Button(new Rectangle(x, y, 180, 36), text, _button180Texture, enabled, anchor);
    }
    
    // 90x36 pixel button
    public static bool Button90(int x, int y, string text, bool enabled = true, Vector2? anchor = null)
    {
        return Button(new Rectangle(x, y, 90, 36), text, _button90Texture, enabled, anchor);
    }
    
    // 36x36 pixel button
    public static bool Button36(int x, int y, string text, bool enabled = true, Vector2? anchor = null)
    {
        return Button(new Rectangle(x, y, 36, 36), text, _button36Texture, enabled, anchor);
    }
    
    // 300x40 pixel button
    public static bool Button300(int x, int y, string text, bool enabled = true, Vector2? anchor = null)
    {
        return Button(new Rectangle(x, y, 300, 40), text, _buttonWideTexture, enabled, anchor);
    }
    
    // 100x40 pixel button
    public static bool Button100(int x, int y, string text, bool enabled = true, Vector2? anchor = null)
    {
        return Button(new Rectangle(x, y, 100, 40), text, _buttonNarrowTexture, enabled, anchor);
    }
    
    public static bool TileButton(int x, int y, Texture2D image, bool selected = false, Vector2? anchor = null)
    {
        anchor ??= Screen.Center;
        x += (int)anchor.Value.X;
        y += (int)anchor.Value.Y;
        
        bool hover = !Input.IsSuppressed() && CheckCollisionPointRec(GetScaledMousePosition(), new Rectangle(x, y, 24, 24));

        if (selected) DrawRectangle(x-2, y-2, 28, 28, Color.White);
        DrawTexture(image, x, y+24 - image.Height, Color.White);

        if (hover)
        {
            _cursorLook = MouseCursor.PointingHand;
            if (Input.Released(MouseButton.Left))
            {
                _buttonClickSFX.Play();
                return true;
            }
        }
        
        return false;
    }

    public static double Slider(int x, int y, string label, double value, Vector2? anchor = null)
    {
        anchor ??= Screen.Center;
        x += (int)anchor.Value.X;
        y += (int)anchor.Value.Y;

        Rectangle rect = new Rectangle(x, y, 300, 40);
        
        bool hover = !Input.IsSuppressed() && CheckCollisionPointRec(GetScaledMousePosition(), rect);
        bool press = (hover && (Input.Held(MouseButton.Left) || Input.Released(MouseButton.Left)) && CheckCollisionPointRec(Input.GetScaledClickPos(), rect));
        
        // if clicking on bar, move pin to mouse
        if (hover && press)
        {
            // value = Math.Clamp((GetScaledMousePosition().X - x + 20) / 260, 0, 1) * (max - min) + min ;
            value = Math.Clamp((GetScaledMousePosition().X - x - 10) / 280, 0, 1);
        }
        
        // Draw bar
        DrawTexture(_sliderBarTexture, x, y, Color.White);
        // Draw title
        DrawTextLeft(x + 2, y + 2, label, anchor: Vector2.Zero);
        // Draw scale labels
        // Draw pin
        // DrawTexture(_sliderPinTexture, (int)(x + (value - min) / max * 260), y, Color.White);
        DrawTexture(_sliderPinTexture, (int)(x - 10 + value * 280), y, Color.White);

        return value;
    }

    public static string TextEntry(int x, int y, string text, Vector2? anchor = null)
    {
        anchor ??= Screen.Center;
        x += (int)anchor.Value.X;
        y += (int)anchor.Value.Y;
        
        bool active = !Input.IsSuppressed() && CheckCollisionPointRec(Input.GetClickPos(), new Rectangle(x, y, 180, 36));
        
        Rectangle subSprite = new Rectangle(0, !active ? 0 : 72, 180, 36);
        DrawTextureRec(_button180Texture, subSprite, new Vector2(x,y), Color.White);
        DrawTextLeft(x+6, y+12, text + (active ? "_" : ""), anchor: Vector2.Zero);
        
        if (active)
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
    
            if ((Input.Pressed(KeyboardKey.Backspace) || Input.Repeat(KeyboardKey.Backspace)) && text.Length > 0)
            {
                text = text.Substring(0, text.Length - 1);
            }

            if (Input.Pressed(KeyboardKey.Enter))
            {
                Input.ResetClickPos();
            }
        }
        return text;
    }
    
    public static string BigTextCopyPad(int x, int y, string label, string text, Vector2? anchor = null)
    {
        anchor ??= Screen.Center;
        x += (int)anchor.Value.X;
        y += (int)anchor.Value.Y;
        
        bool hover = !Input.IsSuppressed() && CheckCollisionPointRec(GetScaledMousePosition(), new Rectangle(x, y, 300, 40));
        bool press = (hover && (Input.Held(MouseButton.Left) || Input.Held(MouseButton.Right)));
        
        Rectangle subSprite = new Rectangle(0, !press ? !hover ? 0 : 40 : 80, 300, 40);
        DrawTextureRec(_buttonWideTexture, subSprite, new Vector2(x,y), Color.White);
        DrawTextCentered(x+150, y+20, label, anchor: Vector2.Zero);

        if (hover)
        {
            _cursorLook = MouseCursor.PointingHand;
            
            DrawRectangle(Screen.CenterX, Screen.CenterY - 300, 600, 600, ColorAlpha(Color.Black, 0.5f));
            DrawTextLeft(2, -298, text);

            if (Input.Released(MouseButton.Left))
            {
                text = GetClipboardText_();
            }

            if (Input.Released(MouseButton.Right))
            {
                SetClipboardText(text);
            }
        }
        
        return text;
    }
    
    public static void UpdateCursor()
    {
        SetMouseCursor(_cursorLook);
        _cursorLook = MouseCursor.Default;
    }
}