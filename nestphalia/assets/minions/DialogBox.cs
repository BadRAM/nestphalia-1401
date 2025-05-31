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
    private Texture2D _portraitPanel;
    
    
    public enum PortraitMode
    {
        None,
        Right,
        Left,
        Both
    }
    
    public DialogBox(string text, Texture2D portrait)
    {
        _portrait = portrait;
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
        // DrawTextLeft(100, -100, Time.Unscaled);
        _charsRevealed = (int)((Time.Unscaled - _timeStarted) / _timePerChar);
        DrawRectangle(Screen.HCenter - 262, Screen.VCenter + 98, 524, 76, Color.Black);
        DrawRectangle(Screen.HCenter - 260, Screen.VCenter + 100, 520, 72, Color.Brown);
        DrawTextLeft(-188, 110, _text.Substring(0, Math.Min(_charsRevealed, _text.Length)));
        
        DrawTexture(_portraitPanel, Screen.HCenter - 256, Screen.VCenter + 104, Color.White);
        DrawTexture(_portrait, Screen.HCenter - 256, Screen.VCenter + 104, Color.White);
    }
}