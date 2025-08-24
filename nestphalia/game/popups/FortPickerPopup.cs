using Raylib_cs;

namespace nestphalia;

public class FortPickerPopup : Popup
{
    private string _titleText;
    private List<string> _items;
    private List<string> _directories;
    private List<Fort> _forts;
    private Action<Fort> _fortReturnAction;
    private Action<string> _newFortAction;

    private Rectangle _rect;
    private Texture2D _bgTex;
    private string _path;
    private int _page = 0;
    private FortPickerPopup? _parent;

    public FortPickerPopup(string path, Action<Fort> fortReturnAction, Action<string> newFortAction, FortPickerPopup? parent = null, int height = 590)
    {
        _titleText = "Load";
        _path = path;
        _parent = parent;
        _forts = new List<Fort>();
        var files = new List<string>(Directory.GetFiles(path));
        for (int i = files.Count - 1; i >= 0; i--)
        {
            if (files[i].Substring(files[i].Length - 5) == ".fort")
            {
                _forts.Add(Fort.LoadFromDisc(files[i]));
            }
        }

        _directories = new List<string>(Directory.GetDirectories(path));
        _items = new List<string>();
        if (_parent != null) _items.Add("^  Return to Parent Folder  ^");
        foreach (string dir in _directories) _items.Add("[" + Path.GetFileNameWithoutExtension(dir) + "]");
        foreach (Fort fort in _forts) _items.Add(fort.Name);
        _items.Add("+  New Fort  +");

        _fortReturnAction = fortReturnAction;
        _newFortAction = newFortAction;

        _rect = new Rectangle(-160, -height / 2, 320, height);
        _bgTex = Resources.GetTextureByName("9slice");
    }

    public override void Draw()
    {
        if (Input.Pressed(Input.InputAction.Exit))
        {
            Close(); // we can use this to cancel because we aren't using the normal exitaction
            return;
        }

        // Draw background etc
        GUI.Draw9Slice(_bgTex, _rect);
        GUI.DrawTextCentered(0, 10, _titleText, anchor: Screen.Center + _rect.Top());

        // Draw options
        int pageSize = (int)(_rect.Height - 60) / 40;
        int itemsToDraw = Math.Min(pageSize, _items.Count - pageSize * _page);
        for (int i = 0; i < itemsToDraw; i++)
        {
            if (GUI.Button300(-150, (int)(i * 40 + _rect.Top().Y + 24), _items[i + pageSize * _page]))
            {
                Pick(i + pageSize * _page);
            }
        }

        // Draw page buttons if relevant
        if (_items.Count > pageSize)
        {
            if (GUI.Button100(-150, -44, "<", _page > 0, Screen.Center + _rect.Bottom())) _page--;
            GUI.Button100(-50, -44, $"{_page + 1}/{_items.Count / pageSize + 1}", false,
                Screen.Center + _rect.Bottom());
            if (GUI.Button100(50, -44, ">", _page < _items.Count / pageSize,
                    anchor: Screen.Center + _rect.Bottom())) _page++;
        }
    }

    // TODO: just load the new folder instead of recursing.
    private void Pick(int picked)
    {
        Close();

        if (_parent != null) picked--;

        if (picked == -1) // Return to parent folder
        {
            PopupManager.Start(_parent);
        }
        else if (picked < _directories.Count) // Open directory
        {
            PopupManager.Start(new FortPickerPopup(_directories[picked] + "/", _fortReturnAction, _newFortAction, this,
                (int)_rect.Height));
        }
        else if (picked < _directories.Count + _forts.Count) // Select fort
        {
            Fort? fort = Fort.LoadFromDisc(_forts[picked - _directories.Count].Path);
            if (fort == null)
            {
                PopupManager.Start(new AlertPopup("Error", "Failed to load fort!", "Dismiss", () => { }));
                return;
            }

            _fortReturnAction.Invoke(fort);
        }
        else // New fort
        {
            _newFortAction.Invoke(_path);
        }
    }
}