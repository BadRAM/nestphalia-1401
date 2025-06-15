using Raylib_cs;
using static nestphalia.GUI;
using static Raylib_cs.Raylib;

namespace nestphalia;

public class DialogBox
{
    private string _text;
    private int _charsRevealed;
    private double _timeStarted;
    private double _timePerChar = 0.1;
    private Texture2D _portrait;
    private Texture2D _portraitTwo;
    private Texture2D _portraitPanel;
    private Mode _mode;
    
    public enum Mode // Where the portrait is on the dialog box
    {
        None,
        Right,
        Left,
        Both
    }
    
    public DialogBox(string text, Mode mode = Mode.None, Texture2D? portrait = null, Texture2D? portraitTwo = null)
    {
        _mode = mode;
        _portrait = portrait ?? Resources.MissingTexture;
        _portraitTwo = portraitTwo ?? Resources.MissingTexture;
        _portraitPanel = Resources.GetTextureByName("ability_slot");
        _timeStarted = Time.Unscaled;
        _text = text;
    }
    
    public void Start()
    {
        _timeStarted = Time.Unscaled;
    }
    
    public void Draw()
    {
        DrawRectangle(Screen.HCenter - 262, Screen.VCenter + 98,  524, 76, Color.Black);
        DrawRectangle(Screen.HCenter - 260, Screen.VCenter + 100, 520, 72, Color.Brown);
        
        _charsRevealed = (int)((Time.Unscaled - _timeStarted) / _timePerChar);
        DrawTextLeft(_mode == Mode.Left || _mode == Mode.Both ? -188 : -256, 104, _text.Substring(0, Math.Min(_charsRevealed, _text.Length)));
        
        if (_mode != Mode.None)
        {
            int x = Screen.HCenter + (_mode == Mode.Left ? -256 : 192);
            DrawTexture(_portraitPanel, x, Screen.VCenter + 104, Color.White);
            DrawTexture(_portrait, x, Screen.VCenter + 104, Color.White);
            
            if (_mode == Mode.Both)
            {
                DrawTexture(_portraitPanel, Screen.HCenter - 256, Screen.VCenter + 104, Color.White);
                DrawTexture(_portraitTwo, Screen.HCenter - 256, Screen.VCenter + 104, Color.White);
            }
        }
    }
}