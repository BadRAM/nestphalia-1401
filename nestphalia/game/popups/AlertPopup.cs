using System.Numerics;
using Raylib_cs;

namespace nestphalia;

// The simplest possible popup, displays text, does something on close.
// DO NOT ADD FEATURES. If a more advanced popup is necessary, create it separately.
public class AlertPopup : Popup
{
    private string _titleText;
    private string _bodyText;
    private string _dismissText;
    private Rectangle _rect = new Rectangle(-180, -120, 360, 240);
    private Texture2D _bgTex;
    private Action _closeAction;

    public AlertPopup(string titleText, string bodyText, string dismissText, Action closeAction) : base()
    {
        _titleText = titleText;
        _bodyText = bodyText;
        _dismissText = dismissText;
        _bgTex = Resources.GetTextureByName("9slice");
        _closeAction = closeAction;
    }

    public override void Draw()
    {
        _rect = GUI.Draw9Slice(_bgTex, _rect, draggable:true, resizable:true);
        GUI.DrawTextCentered(0, 20, _titleText, anchor: Screen.Center + _rect.Top());
        GUI.DrawTextCentered(0, 40, _bodyText, anchor: Screen.Center + _rect.Top());
        if (GUI.Button100(-50, -50, _dismissText, anchor: Screen.Center + _rect.Bottom()))
        {
            Close();
            _closeAction.Invoke();
        }
    }
}