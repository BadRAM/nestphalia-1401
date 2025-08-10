using Raylib_cs;

namespace nestphalia;

public class PickerPopup : Popup
{
    private string _titleText;
    private List<string> _items;
    private Action<int> _pickAction;

    private Rectangle _rect;
    private Texture2D _bgTex;
    private int _page = 0;
    
    public PickerPopup(string titleText, string[] items, Action<int> pickAction, int height = 590) : base(() => { })
    {
        _titleText = titleText;
        _items = new List<string>(items);
        _pickAction = pickAction;
        
        _rect = new Rectangle(-160, -height / 2, 320, height);
        _bgTex = Resources.GetTextureByName("9slice");
    }
    
    
    public override void Draw()
    {
        // Draw background etc
        GUI.Draw9Slice(_bgTex, _rect);
        GUI.DrawTextCentered(0, 10, _titleText, anchor: Screen.Center + _rect.Top());
        
        // Draw options
        int pageSize = (int)(_rect.Height - 60) / 40;
        int itemsToDraw = Math.Min(pageSize, _items.Count - pageSize * _page);
        for (int i = 0; i < itemsToDraw; i++)
        {
            if (GUI.Button300(-150, (int)(i * 40 + _rect.Top().Y + 24), _items[i+pageSize*_page]))
            {
                Pick(i+pageSize*_page);
            }
        }
        
        // Draw page buttons if relevant
        if (_items.Count > pageSize)
        {
            if (GUI.Button100(-150, -44, "<", _page > 0, Screen.Center + _rect.Bottom())) _page--;
                GUI.Button100(-50,  -44, $"{_page+1}/{_items.Count / pageSize + 1}", false, Screen.Center + _rect.Bottom());
            if (GUI.Button100( 50,  -44, ">", _page < _items.Count / pageSize, anchor: Screen.Center + _rect.Bottom())) _page++;
        }
    }

    private void Pick(int picked)
    {
        Close();
        _pickAction.Invoke(picked);
    }
}