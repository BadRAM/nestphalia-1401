using Raylib_cs;

namespace nestphalia;

public class YesNoPopup : Popup
{
    private string _titleText;
    private string _bodyText;
    private string _yesText;
    private string _noText;
    private Rectangle _rect = new Rectangle(-180, -120, 360, 240);
    private Texture2D _bgTex;
    private Action<bool> _closeAction;

    public YesNoPopup(string titleText, string bodyText, string yesText, string noText, Action<bool> closeAction) : base()
    {
        _titleText = titleText;
        _bodyText = bodyText;
        _yesText = yesText;
        _noText = noText;
        _bgTex = Resources.GetTextureByName("9slice");
        _closeAction = closeAction;
    }

    public override void Draw()
    {
        _rect = GUI.Draw9Slice(_bgTex, _rect, draggable:true);
        GUI.DrawTextCentered(0, 20, _titleText, anchor: Screen.Center + _rect.Top());
        GUI.DrawTextCentered(0, 40, _bodyText, anchor: Screen.Center + _rect.Top());
        if (GUI.Button180(-182, -50, _yesText, anchor: Screen.Center + _rect.Bottom()))
        {
            Close();
            _closeAction.Invoke(true);
        }
        if (GUI.Button180(2, -50, _noText, anchor: Screen.Center + _rect.Bottom()))
        {
            Close();
            _closeAction.Invoke(false);
        }
    }
}