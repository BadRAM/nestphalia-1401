using System.Runtime.CompilerServices;
using Raylib_cs;

namespace nestphalia;

public static class GameConsole
{
    public static List<string> LogHistory = new List<string>();
    private static List<string> _commandHistory = new List<string>();
    private static int _commandHistoryCursor = -1;
    private static bool _open;
    private static string _input;
    public static WrenCommand WrenCommand = new WrenCommand();

    public static void Draw()
    {
        if (_open && Raylib.IsKeyPressed(KeyboardKey.Escape))
        {
            _open = false;
            Input.SetSuppressed(Input.SuppressionSource.GameConsole, false);
        } 
        else if (Raylib.IsKeyPressed(KeyboardKey.Grave)) 
        {
            _open = !_open;
            Input.SetSuppressed(Input.SuppressionSource.GameConsole, _open);
        } 
        else if (_open)
        {
            if (GUI.GetScaledCursorPosition().Y < Screen.BottomY/2)
            {
                Input.SetSuppressed(Input.SuppressionSource.GameConsole, true);
                Input.StartSuppressionOverride(Input.SuppressionSource.GameConsole);
                
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
                
                if (Input.Pressed(KeyboardKey.Up) && _commandHistory.Count > 0)
                {
                    _commandHistoryCursor = Math.Min(_commandHistoryCursor + 1, _commandHistory.Count-1);
                    _input = _commandHistory[_commandHistoryCursor];
                }
                if (Input.Pressed(KeyboardKey.Down) && _commandHistory.Count > 0)
                {
                    _commandHistoryCursor = Math.Max(_commandHistoryCursor - 1, 0);
                    _input = _commandHistory[_commandHistoryCursor];
                }

                if ((Input.Held(KeyboardKey.LeftControl) || Input.Held(KeyboardKey.RightControl)) && Input.Pressed(KeyboardKey.V))
                {
                    _input += Raylib.GetClipboardText_();
                }
                
                if ((Input.Held(KeyboardKey.LeftControl) || Input.Held(KeyboardKey.RightControl)) && Input.Pressed(KeyboardKey.C))
                {
                    if (_input == "")
                    {
                        Raylib.SetClipboardText(string.Join("\n", LogHistory));
                    }
                    else
                    {
                        Raylib.SetClipboardText(_input);
                    }
                }
    
                if ((Input.Pressed(KeyboardKey.Backspace) || Input.Repeat(KeyboardKey.Backspace)) && _input.Length > 0)
                {
                    _input = _input.Substring(0, _input.Length - 1);
                    if (Input.Held(KeyboardKey.LeftControl) || Input.Held(KeyboardKey.RightControl))
                    {
                        _input = "";
                    }
                }
                Input.EndSuppressionOverride();
            }
            else
            {
                Input.SetSuppressed(Input.SuppressionSource.GameConsole, false);
            }
            
            if (Raylib.IsKeyPressed(KeyboardKey.Enter) && _input.Length > 0)
            {
                _commandHistory.Insert(0, _input);
                WriteLine(_input);
                WrenCommand.Execute(_input);
                _input = "";
            }
            
            Raylib.DrawRectangle(0, 0, Screen.RightX, Screen.BottomY/2, Raylib.ColorAlpha(Color.Black, 0.5f));
            Raylib.DrawRectangle(0, Screen.BottomY/2, Screen.RightX, 2, Color.DarkGray);

            int y = Screen.BottomY / 2 - 14;
            
            GUI.DrawMonoTextLeft(4, y, "> " + _input, anchor: Screen.TopLeft);
            int i = LogHistory.Count-1;
            while (y > -14 && i >= 0)
            {
                string line = GUI.WrapText(LogHistory[i], Screen.RightX);
                y -= (int)GUI.MeasureText(line).Y;
                GUI.DrawMonoTextLeft(4, y, line, anchor: Screen.TopLeft);
                i--;
            }
        }
    }
    
    public static void WriteLine(string? message)
    {
        if (message == null)
        {
            Console.WriteLine(message);
            return;
        }
        List<string> lines = new List<string>(message.Split("\n"));
        foreach (string line in lines) { LogHistory.Add(line); }
        Console.WriteLine(message);
    }
}