using Raylib_cs;
using static nestphalia.GUI;
using static Raylib_cs.Raylib;

namespace nestphalia;

public class DialogBox : Popup
{
    private string _text;
    private int _charsRevealed;
    private double _timeStarted;
    private double _timePerChar = 0.1;
    private Texture2D _portrait;
    private Texture2D _portraitPanel;
    private Texture2D _background;
    private Mode _mode;
    private Rectangle _rect = new Rectangle(-300, 50, 600, 200);
    
    public enum Mode // Where the portrait is on the dialog box
    {
        None,
        Right,
        Left
    }
    
    public DialogBox(string text, Action closeAction, Mode mode = Mode.None, Texture2D? portrait = null) : base(closeAction)
    {
        _mode = mode;
        _portrait = portrait ?? Resources.MissingTexture;
        _portraitPanel = Resources.GetTextureByName("ability_slot");
        _background = Resources.GetTextureByName("9slice");
        _timeStarted = Time.Unscaled;
        _text = text;
    }
    
    public void Start()
    {
        _timeStarted = Time.Unscaled;
    }
    
    public override void Draw()
    {
        Draw9Slice(_background, _rect);
        // DrawRectangle(Screen.CenterX - 262, Screen.CenterY + 98,  524, 76, Color.Black);
        // DrawRectangle(Screen.CenterX - 260, Screen.CenterY + 100, 520, 72, Color.Brown);
        
        _charsRevealed = (int)((Time.Unscaled - _timeStarted) / _timePerChar);
        DrawTextLeft(_mode == Mode.Left ? -188 : -256, 104, _text.Substring(0, Math.Min(_charsRevealed, _text.Length)));
        
        if (_mode != Mode.None)
        {
            int x = Screen.CenterX + (_mode == Mode.Left ? -256 : 192);
            DrawTexture(_portraitPanel, x, Screen.CenterY + 104, Color.White);
            DrawTexture(_portrait, x, Screen.CenterY + 104, Color.White);
        }
    }
}