namespace nestphalia;

public abstract class Popup(Action closeAction)
{
    private static Popup? _activePopup;
    
    private Action _closeAction = closeAction;

    // This is called by scenes to update whatever popup is currently active
    public static void Update()
    {
        if (_activePopup != null)
        {
            Input.StartSuppressionOverride(Input.SuppressionSource.Popup);
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
    }
    
    // This is supplied by implementations of Popup to be called when they are active.
    public abstract void Draw();

    protected void Close()
    {
        _activePopup = null;
        Time.TimeScale = 1;
        Input.SetSuppressed(Input.SuppressionSource.Popup, false);
        _closeAction.Invoke();
    }
}