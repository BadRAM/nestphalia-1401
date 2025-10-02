namespace nestphalia;

public static class PopupManager
{
    private static List<Popup> _popupStack = new List<Popup>();
    private static bool _firstFrame;
    
    // This is called by scenes to update whatever popup is currently active
    public static void Update()
    {
        if (_popupStack.Count > 0)
        {
            for (int i = 0; i < _popupStack.Count-1; i++)
            {
                _popupStack[i].Draw();
            }
            
            if (_firstFrame)
            {
                _firstFrame = false;
            }
            else
            {
                Input.StartSuppressionOverride(Input.SuppressionSource.Popup);
            }
            _popupStack[^1].Draw();
            Input.EndSuppressionOverride();
        }
    }
    
    public static bool PopupActive()
    {
        return _popupStack.Count > 0;
    }
    
    public static void Start(Popup popup)
    {
        _popupStack.Add(popup);
        Time.Paused = true;
        Input.SetSuppressed(Input.SuppressionSource.Popup, true);
        _firstFrame = true;
    }

    // Warning: Calling this from outside a popup will close the current popup without running it's close behavior
    public static void Remove(Popup popup)
    {
        if (!_popupStack.Contains(popup))
        {
            throw new Exception("Unregistered popup tried to exit the stack, something is VERY wrong!");
        }
        _popupStack.Remove(popup);
        _firstFrame = true;
        if (_popupStack.Count == 0)
        {
            Input.SetSuppressed(Input.SuppressionSource.Popup, false);
        }
    }
}