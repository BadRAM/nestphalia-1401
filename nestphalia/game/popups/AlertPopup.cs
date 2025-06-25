using System.Numerics;
using Raylib_cs;

namespace nestphalia;

public class AlertPopup : Popup
{
    private string _titleText;
    private string _bodyText;
    private string _dismissText;
    private Vector2 _position;
    private Vector2 _size;
    private Rectangle _rect;
    private Texture2D _bgTex;

    public AlertPopup(string titleText, string bodyText, string dismissText, Action closeAction, Vector2? position = null, Vector2? size = null) : base(closeAction)
    {
        _titleText = titleText;
        _bodyText = bodyText;
        _dismissText = dismissText;
        _size = size ?? new Vector2(360, 240);
        _position = position ?? -_size / 2;
        _rect = new Rectangle(_position, _size);
        
        _bgTex = Resources.GetTextureByName("9slice");
    }

    public override void Draw()
    {
        _rect = GUI.Draw9Slice(_bgTex, _rect, draggable:true, resizable:true);
        GUI.DrawTextCentered(0, 20, _titleText, anchor: Screen.Center + _rect.Top());
        GUI.DrawTextCentered(0, 40, _bodyText, anchor: Screen.Center + _rect.Top());
        if (GUI.Button100(-50, -50, _dismissText, anchor: Screen.Center + _rect.Bottom()))
        {
            Close();
        }
    }
}