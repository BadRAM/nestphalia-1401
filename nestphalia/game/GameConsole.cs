using System.Runtime.CompilerServices;
using Raylib_cs;

namespace nestphalia;

public static class GameConsole
{
    public static List<string> LogHistory = new List<string>();
    private static bool _open;
    private static string _input;

    public static void Draw()
    {
        if (_open && Raylib.IsKeyPressed(KeyboardKey.Escape))
        {
            _open = false;
            Input.Suppressed = false;
        } 
        else if (Raylib.IsKeyPressed(KeyboardKey.Grave)) 
        {
            _open = !_open;
            Input.Suppressed = _open;
        } 
        else if (_open)
        {
            if (GUI.GetScaledMousePosition().Y < Screen.Bottom/2)
            {
                Input.Suppressed = true;
                
                // Get char pressed (unicode character) on the queue
                int key = Raylib.GetCharPressed();
    
                // Check if more characters have been pressed on the same frame
                while (key > 0)
                {
                    // NOTE: Only allow keys in range [32..125]
                    if ((key >= 32) && (key <= 125))
                    {
                        _input += (char)key;
                    }
                    key = Raylib.GetCharPressed();  // Check next character in the queue
                }

                if (Raylib.IsMouseButtonPressed(MouseButton.Right))
                {
                    _input += Raylib.GetClipboardText_();
                }
    
                if ((Raylib.IsKeyPressed(KeyboardKey.Backspace) || Raylib.IsKeyPressedRepeat(KeyboardKey.Backspace)) && _input.Length > 0)
                {
                    _input = _input.Substring(0, _input.Length - 1);
                }
            }
            else
            {
                Input.Suppressed = false;
            }
            
            if (Raylib.IsKeyPressed(KeyboardKey.Enter) && _input.Length > 0)
            {
                WriteLine(_input);
                string commandOutput = Command.Execute(_input);
                if (commandOutput != "") WriteLine(commandOutput);
                _input = "";
            }
            
            Raylib.DrawRectangle(0, 0, Screen.Right, Screen.Bottom/2, Raylib.ColorAlpha(Color.Black, 0.5f));
            Raylib.DrawRectangle(0, Screen.Bottom/2, Screen.Right, 2, Color.DarkGray);

            int y = Screen.Bottom / 2 - 14;
            
            GUI.DrawTextLeft(4, y, "> " + _input, guiSpace:false);
            int i = LogHistory.Count-1;
            while (y > -14 && i >= 0)
            {
                y -= 14;
                GUI.DrawTextLeft(4, y, LogHistory[i], guiSpace:false);
                i--;
            }
        }
    }
    
    public static void WriteLine(string message)
    {
        List<string> lines = new List<string>(message.Split("\n"));
        foreach (string line in lines) { LogHistory.Add(line); }
        Console.WriteLine(message);
    }
}