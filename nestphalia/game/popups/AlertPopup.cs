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
    public Rectangle Rect = new Rectangle(-180, -120, 360, 240);
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
        Rect = GUI.Draw9Slice(_bgTex, Rect, draggable:true, resizable:true);
        GUI.DrawTextCentered(0, 20, _titleText, anchor: Screen.Center + Rect.Top());
        GUI.DrawTextCentered(0, 40, _bodyText, anchor: Screen.Center + Rect.Top());
        if (GUI.Button100(-50, -50, _dismissText, anchor: Screen.Center + Rect.Bottom()))
        {
            Close();
            _closeAction.Invoke();
        }
    }
}