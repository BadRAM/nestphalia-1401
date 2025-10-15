using System.Numerics;
using Raylib_cs;

namespace nestphalia;

public class IntroScene : Scene
{
    private double _duration = 2;
    private double _startTime;

    private Texture2D _1401;
    private Texture2D _1401_aura;
    
    public void Start()
    {
        Program.CurrentScene = this;
        _startTime = Time.Unscaled;

        _1401 = Resources.GetTextureByName("title_1401");
        _1401_aura = Resources.GetTextureByName("title_1401_aura2");
    }

    public override void Update()
    {
        Screen.BeginDrawing();
        float bgLightness = (float)Math.Clamp(Math.Abs((Time.Unscaled - _startTime) / _duration - 0.5) * -2 + 0.5, 0, 1);
        Raylib.ClearBackground(Raylib.ColorLerp(Color.Black, Color.Gray, bgLightness));
        
        float textAlpha = Math.Clamp((float)((Time.Unscaled - _startTime) / _duration) * 6 - 3, 0, 1);
        Raylib.DrawTexture(_1401, Screen.CenterX - _1401.Width/2, Screen.CenterY - _1401.Height/2, Raylib.ColorAlpha(Color.White, textAlpha));
        
        for (int i = 0; i < 30; i++)
        {
            float scale = 1 + i * 0.2f;
            float offset = (float)(Math.Sin((Time.Unscaled - _startTime + 1) / 2*Math.PI) * 20 * i);
            Raylib.DrawTextureEx
            (
                _1401_aura, 
                new Vector2(Screen.CenterX - _1401.Width/2 * scale, Screen.CenterY - _1401.Height/2 * scale + offset), 
                0,
                scale,
                Raylib.ColorAlpha(Color.White, 0.2f * bgLightness)
            );
        }
        
        if (Time.Unscaled - _startTime > _duration || 
            Input.Pressed(KeyboardKey.Escape) || 
            Input.Pressed(KeyboardKey.Space) || 
            Input.Pressed(InputAction.QuickLoad) || 
            Input.Released(InputAction.Click))
        {
            new MenuScene().Start();
        }
    }
}