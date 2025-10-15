using Raylib_cs;
using static nestphalia.GUI;
using static Raylib_cs.Raylib;

namespace nestphalia;

public class DialogBox : Popup
{
    private RichText _text;
    private int _charsRevealed = -1; // start at -1 so first frame it can tick to zero and trigger update event
    private bool _finished;
    // private double _lastCharRevealTime;
    private double _timeToNextChar;
    private double _timePerChar = 0.1;
    private Texture2D _portrait;
    private Texture2D _portraitPanel;
    private StretchyTexture _background;
    private Mode _mode;
    private Rectangle _rect = new Rectangle(-300, 50, 600, 200);
    private Action _closeAction;
    private SoundResource _textSound;
    
    public enum Mode // Where the portrait is on the dialog box
    {
        None,
        Right,
        Left
    }
    
    public DialogBox(string text, Action closeAction, Mode mode = Mode.None, Texture2D? portrait = null) : base()
    {
        _text = new RichText(text);
        _closeAction = closeAction;
        _mode = mode;
        _portrait = portrait ?? Resources.MissingTexture;
        _portraitPanel = Resources.GetTextureByName("ability_slot");
        _background = Assets.Get<StretchyTexture>("stretch_default");
        _textSound = Resources.GetSoundByName("marimba");
        // _lastCharRevealTime = Time.Unscaled;
    }
    
    public override void Draw()
    {
        DrawStretchyTexture(_background, _rect);

        _timeToNextChar -= Time.DeltaTime;
        if(Input.Held(InputAction.Click) || Input.Held(InputAction.Click)) _timeToNextChar -= Time.DeltaTime;
        if (!_finished && _timeToNextChar <= 0)
        {
            _timeToNextChar += _timePerChar;
            _charsRevealed++;

            if (_charsRevealed > 0 && _text.Text[_charsRevealed-1] != ' ') _textSound.PlayRandomPitch(volume:0.3f);
            
            foreach (StringTag tag in _text.Tags.FindAll(o => o.Index == _charsRevealed))
            {
                switch (tag.Tag)
                {
                    case "pause":
                        _timeToNextChar += double.TryParse(tag.Attribute, out double num) ? num : 0.5;
                        break;
                    case "sound":
                        Resources.GetSoundByName(tag.Attribute).Play(volume:1);
                        break;
                }
            }

            if (_charsRevealed == _text.Text.Length) _finished = true;
        }
        _text.DrawLeft(-256, 104, _charsRevealed);
        
        if (_mode != Mode.None)
        {
            int x = Screen.CenterX + (_mode == Mode.Left ? -256 : 192);
            DrawTexture(_portraitPanel, x, Screen.CenterY + 104, Color.White);
            DrawTexture(_portrait, x, Screen.CenterY + 104, Color.White);
        }
        
        if (Input.Pressed(InputAction.Click))
        {
            if (_charsRevealed < _text.Text.Length)
            {
                // _charsRevealed = _text.Text.Length;
            }
            else
            {
                Close();
                _closeAction.Invoke();
            }
        }
    }
}