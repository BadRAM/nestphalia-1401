namespace nestphalia;

public static class PopupManager
{
    private static Popup? _activePopup;
    private static bool _firstFrame;
    
    // This is called by scenes to update whatever popup is currently active
    public static void Update()
    {
        if (_activePopup != null)
        {
            if (_firstFrame)
            {
                _firstFrame = false;
            }
            else
            {
                Input.StartSuppressionOverride(Input.SuppressionSource.Popup);
            }
            _activePopup.Draw();
            Input.EndSuppressionOverride();
        }
    }
    
    public static bool PopupActive()
    {
        return _activePopup != null;
    }
    
    public static void Start(Popup popup)
    {
        if (_activePopup != null) throw new Exception("Tried to create a popup when one already exists");
        _activePopup = popup;
        Time.TimeScale = 0;
        Input.SetSuppressed(Input.SuppressionSource.Popup, true);
        _firstFrame = true;
    }

    // Warning: Calling this from outside a popup will close the current popup without running it's close behavior
    public static void Clear()
    {
        _activePopup = null;
    }
}