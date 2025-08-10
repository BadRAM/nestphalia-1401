using Raylib_cs;

namespace nestphalia;

public class FortPickerPopup : Popup
{
    private string _titleText;
    private List<string> _items;
    private List<string> _directories;
    private List<string> _files;
    private Action<Fort> _pickAction;

    private Rectangle _rect;
    private Texture2D _bgTex;
    private string _path;
    private int _page = 0;
    private FortPickerPopup? _parent;
    
    public FortPickerPopup(string titleText, string path, Action<Fort> pickAction, FortPickerPopup? parent = null, int height = 590) : base(() => { })
    {
        _titleText = titleText;
        _path = path;
        _parent = parent;
        _files = new List<string>(Directory.GetFiles(path));
        for (int i = _files.Count-1; i >= 0; i--)
        {
            if (_files[i].Substring(_files[i].Length-5) != ".fort")
            {
                GameConsole.WriteLine($"Ignored non .fort file {_files[i]}");
                _files.RemoveAt(i);
            }
        }
        _directories = new List<string>(Directory.GetDirectories(path));
        _items = new List<string>();
        if (_parent != null) _items.Add("^  Return to Parent Folder  ^");
        foreach (string dir in _directories) _items.Add("[" + Path.GetFileNameWithoutExtension(dir) + "]");
        foreach (string file in _files) _items.Add(Path.GetFileNameWithoutExtension(file));
        // _items.Add("+  New Fort  +");
        
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

        if (_parent != null) picked--;

        if (picked == -1) // Return to parent folder
        {
            PopupManager.Start(_parent);
        }
        else if (picked < _directories.Count) // Open directory
        {
            PopupManager.Start(new FortPickerPopup(_titleText, _directories[picked], _pickAction, this, (int)_rect.Height));
        }
        else if (picked < _directories.Count + _files.Count) // Select fort
        {
            Fort fort = Resources.LoadFort(_files[picked - _directories.Count]);
            if (fort == null)
            {
                PopupManager.Start(new AlertPopup("Error", "Failed to load fort!", "Dismiss", () => {}));
                return;
            }
            _pickAction.Invoke(fort);
        }
        // else // New fort
        // {
        //     string path = Directory.GetCurrentDirectory() + "/forts/" + _path;
        //     Fort f = new Fort(Resources.GetUnusedFortName(path), path);
        //     new EditorScene().Start(Start, f, _data);
        // }
    }
    
    private void ListForts()
    {
        if (GUI.Button100(-600, 260, "<", _page > 1)) _page--;
        GUI.Button100(-500, 260, _page.ToString(), false); // this one's just a number display box
        if (GUI.Button100(-400, 260, ">", _page <= ((_path == "" ? 0 : 1) + _directories.Count + _files.Count)/12)) _page++;
        
        for (int i = 0; i < 12; i++)
        {
            int index = i + (_page - 1) * 12 + (_path != "" ? -1 : 0);
            if (index >= _directories.Count + _files.Count + 1) break;
            if (index == -1) // This is the 'return to parent folder' button
            {
                if (GUI.Button300(-600, i * 40 - 240, "^  Return to Parent Folder  ^"))
                {
                    _path = "";
                    // _directories = Directory.GetDirectories(Directory.GetCurrentDirectory() + "/forts/Campaign/" + _path);
                    // _files = Directory.GetFiles(Directory.GetCurrentDirectory() + "/forts/Campaign/" + _path);
                }
            }
            else if (index < _directories.Count) // This is a directory
            {
                if (GUI.Button300(-600, i * 40 - 240,
                        $"/{Path.GetFileName(_directories[index])}/"))
                {
                    _path = Path.GetFileName(_directories[index]);
                    // _directories = Directory.GetDirectories(Directory.GetCurrentDirectory() + "/forts/Campaign/" + _path);
                    // _files = Directory.GetFiles(Directory.GetCurrentDirectory() + "/forts/Campaign/" + _path);
                }
            }
            else if (index < _directories.Count + _files.Count) // This is a fort
            {
                string fortPath = _files[index - _directories.Count].Substring(Directory.GetCurrentDirectory().Length);
                if (GUI.Button300(-600, i * 40 - 240, Path.GetFileNameWithoutExtension(fortPath)))
                {
                    GameConsole.WriteLine("Loading " + Path.GetFileName(fortPath));
                    // _fort = Resources.LoadFort(fortPath);
                    // _fort.Name = Path.GetFileNameWithoutExtension(fortPath);
                    // _fort.Comment = _fort.FortSummary();
                    // // _fort.Path = fortPath;
                    // _fortValidityMessage = _fort.IsValid(_data);
                    // _selectedFort = index;
                }
            }
            else
            {
                if (GUI.Button300(-600, i * 40 - 240, "+  New Fort  +"))
                {
                    string path = "/forts/Campaign/" + _path;
                    Fort f = new Fort(Resources.GetUnusedFortName(path), path);
                    // new EditorScene().Start(Start, f, _data);
                }
            }
        }
    }

}