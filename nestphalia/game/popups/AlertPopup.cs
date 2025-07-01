using System.Numerics;
using Raylib_cs;

namespace nestphalia;

public class AlertPopup : Popup
{
    public string TitleText;
    public string BodyText;
    public string DismissText;
    public Rectangle Rect = new Rectangle(-180, -120, 360, 240);
    public Texture2D BgTex;

    public AlertPopup(string titleText, string bodyText, string dismissText, Action closeAction) : base(closeAction)
    {
        TitleText = titleText;
        BodyText = bodyText;
        DismissText = dismissText;
        BgTex = Resources.GetTextureByName("9slice");
    }

    public override void Draw()
    {
        Rect = GUI.Draw9Slice(BgTex, Rect, draggable:true, resizable:true);
        GUI.DrawTextCentered(0, 20, TitleText, anchor: Screen.Center + Rect.Top());
        GUI.DrawTextCentered(0, 40, BodyText, anchor: Screen.Center + Rect.Top());
        if (GUI.Button100(-50, -50, DismissText, anchor: Screen.Center + Rect.Bottom()))
        {
            Close();
        }
    }
}