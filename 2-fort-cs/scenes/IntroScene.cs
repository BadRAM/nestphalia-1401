namespace _2_fort_cs;

public static class IntroScene
{
    public static void Start()
    {
        Program.CurrentScene = Scene.Intro;
    }

    public static void Update()
    {
        MenuScene.Start();
    }
}