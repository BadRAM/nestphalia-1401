using System.Numerics;
using Raylib_cs;

namespace nestphalia;

public class RichText
{
    public string Text;
    public List<StringTag> Tags = new List<StringTag>(); // Keep this sorted

    public RichText(string text)
    {
        Text = text;

        for (int i = Text.IndexOf('<'); i != -1; i = Text.IndexOf('<'))
        {
            string value = Text.Substring(i);
            int tagLen = value.IndexOf('>');
            value = value.Substring(1, tagLen-1);
            Tags.Add(new StringTag(i, value));
            Text = Text.Remove(i, tagLen+1);
        }
    }

    public void InsertText(int startIndex, string value)
    {
        Text = Text.Insert(startIndex, value);
        foreach (StringTag tag in Tags)
        {
            if (tag.Index > startIndex)
            {
                tag.Index += value.Length;
            }
        }
    }

    public void AddTag(StringTag tag)
    {
        for (int i = 0; i < Tags.Count-1; i++)
        {
            if (tag.Index <= Tags[i].Index)
            {
                Tags.Insert(i, tag);
                return;
            }
        }
        Tags.Add(tag);
    }

    public void RemoveAllTagsMatching(string matching)
    {
        for (int i = Tags.Count - 1; i >= 0; i--)
        {
            if (Tags[i].Tag == matching || matching == "")
            {
                Tags.RemoveAt(i);
            }
        }
    }

    public string GetTaggedString()
    {
        string ret = Text;
        for (int i = Tags.Count - 1; i >= 0; i--)
        {
            ret = ret.Insert(Tags[i].Index, Tags[i].ToString());
        }
        return ret;
    }
    
    public void DrawLeft(Vector2 pos, int? length = null, float size = GUI.FontSize, Color? baseColor = null, Vector2? anchor = null)
    {
        length ??= Text.Length;
        length = Math.Min(length.Value, Text.Length);
        anchor ??= Screen.Center;
        pos += anchor.Value;
        Vector2 offset = Vector2.Zero;
        
        baseColor ??= new Color(255, 255, 255, 255);
        Color? colorOverride = null;
        
        if (Tags.Count == 0)
        {
            Raylib.DrawTextEx(Resources.Font, Text.Substring(0, length.Value), pos, size, size / GUI.FontSize, colorOverride ?? baseColor.Value);
            return;
        }

        for (int i = 0; i < length; i++)
        {
            // Process tags
            foreach (StringTag tag in Tags.FindAll(o => o.Index == i))
            {
                switch (tag.Tag)
                {
                    case "c":
                        colorOverride = ColorTag(tag);
                        break;
                    case "br":
                        offset.X = 0;
                        offset.Y += size;
                        break;
                    case null:
                        break;
                }
            }

            if (Text[i] == '\n')
            {
                offset.X = 0;
                offset.Y += size;
            }
            else
            {
                // Draw letter
                Raylib.DrawTextCodepoint(Resources.Font, Text[i], pos + offset, size, colorOverride ?? baseColor.Value);
                offset.X += GUI.MeasureText($"{Text[i]}", size).X + size / Resources.Font.BaseSize;
            }
        }
    }

    public void DrawLeft(int x, int y, int? length = null, float size = GUI.FontSize, Color? baseColor = null, Vector2? anchor = null)
    {
        DrawLeft(new Vector2(x, y), length, size, baseColor, anchor);
    }

    public static void DrawLeft(int x, int y, string text, int? length = null, float size = GUI.FontSize, int wrapWidth = 0, Color? baseColor = null, Vector2? anchor = null)
    {
        RichText rich = new RichText(text);
        if (wrapWidth != 0) rich.Wrap(wrapWidth, size);
        rich.DrawLeft(new Vector2(x, y), length, size, baseColor, anchor);
    }

    private static Color? ColorTag(StringTag tag)
    {
        Color? col = null;
        if (tag.Role == StringTag.Roles.End)
        {
            col = null;
        }
        else if (tag.Attribute == "red")
        {
            col = Color.Red;
        }
        else if (tag.Attribute == "green")
        {
            col = Color.Green;
        }
        else if (tag.Attribute == "blue")
        {
            col = Color.Blue;
        }
        else
        {
            col = Utils.TryHexToColor(tag.Attribute);
        }

        return col;
    }


    public void Wrap(int wrapWidth, float size = GUI.FontSize, Font? font = null)
    {
        font ??= Resources.Font;
        float spacing = size / GUI.FontSize;
        string line = "";
        int lineStart = 0;
        int cursor = 0; // relative to linestart
        RemoveAllTagsMatching("br");

        while (true)
        {
            cursor = Text.Substring(lineStart).IndexOf('\n'); // set cursor to next newline
            if (cursor == -1)
            {
                line = Text.Substring(lineStart);
                if (Raylib.MeasureTextEx(font.Value, line.Trim(), size, spacing).X <= wrapWidth)
                {
                    break; // this line is short enough, and it's the last one. we're done!
                }
            }
            else
            {
                line = Text.Substring(lineStart, cursor);
                float width = Raylib.MeasureTextEx(font.Value, line.Trim(), size, spacing).X;
                if (width <= wrapWidth)
                {
                    lineStart += cursor + 1;
                    continue; // this line is short enough, but not the last one. go to next line.
                }
            }

            cursor = line.IndexOf(' '); // set cursor to first space on line
            if (cursor == -1) cursor = line.Length;
            if (Raylib.MeasureTextEx(font.Value, line.Substring(0, cursor), size, spacing).X > wrapWidth)
            {
                // split first word
                cursor = 1;
                while (Raylib.MeasureTextEx(font.Value, line.Substring(0, cursor), size, spacing).X <= wrapWidth)
                {
                    cursor++;
                }
                // Insert(lineStart + cursor-1, "\n"); // insert newline
                AddTag(new StringTag(lineStart + cursor, "br/"));
                cursor--;
                lineStart += cursor;
                continue;
            }

            do // Advance the cursor to the first space past 
            {
                int next = line.Substring(cursor + 1).IndexOf(' ');
                if (next == -1) // there's no trailing space, but we need to only wrap the last word.
                {
                    cursor = line.Length;
                    break;
                }
                cursor += next + 1; // set cursor to next space on line
            } while (Raylib.MeasureTextEx(font.Value, line.Substring(0, cursor), size, spacing).X <= wrapWidth);

            cursor = line.Substring(0, cursor).LastIndexOf(' ')+1; // go back one space
            AddTag(new StringTag(lineStart + cursor, "br/"));
            // InsertText(lineStart + cursor, "\n"); // insert newline
            lineStart += cursor;
        }
    }
}

public class StringTag
{
    public int Index;
    public Roles Role;
    public string Tag;
    public string Attribute = "";
    // public Dictionary<string, string> Attributes = new Dictionary<string, string>();
    
    public enum Roles
    {
        Start,
        End,
        Single
    }

    public StringTag(int index, string text)
    {
        Index = index;
        if (text[^1] == '/')
        {
            Role = Roles.Single;
            text = text.Remove(text.Length - 1);
        }
        else if (text[0] == '/')
        {
            Role = Roles.End;
            text = text.Remove(0, 1);
        }
        else Role = Roles.Start;
        
        string[] split = text.Split(':');
        Tag = split[0];
        if (split.Length > 1)
        {
            Attribute = split[1];
        }

        // string[] split = text.Split(' ');
        // Tag = split[0];
        // for (int i = 1; i < split.Length; i++)
        // {
        //     Attributes.Add();
        // }
    }

    public new string ToString()
    {
        return $"<{(Role == Roles.End ? "/" : "")}" +
               $"{Tag}" +
               $"{(Attribute == "" ? "" : ":")}" +
               $"{Attribute}" +
               $"{(Role == Roles.Single ? "/" : "")}>";
    }
}
