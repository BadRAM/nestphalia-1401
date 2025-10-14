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
    private StretchyTexture _bgTex;
    private Action _closeAction;

    public AlertPopup(string titleText, string bodyText, string dismissText, Action closeAction) : base()
    {
        _titleText = titleText;
        _bodyText = bodyText;
        _dismissText = dismissText;
        _bgTex = Assets.Get<StretchyTexture>("stretch_xp");
        _closeAction = closeAction;
    }

    public override void Draw()
    {
        Rect = GUI.DrawStretchyTexture(_bgTex, Rect, draggable:true, resizable:true);
        GUI.DrawTextCentered(0, 20, _titleText, color: Color.Black, anchor: Screen.Center + Rect.TopCenter());
        GUI.DrawTextCentered(0, 40, _bodyText, color: Color.Black, anchor: Screen.Center + Rect.TopCenter());
        if (GUI.Button90(-45, -50, _dismissText, anchor: Screen.Center + Rect.BottomCenter()) || Input.Pressed(Input.InputAction.Exit))
        {
            Close();
            _closeAction.Invoke();
        }
    }
}