namespace nestphalia;

public class IntroScene : Scene
{
    public void Start()
    {
        Program.CurrentScene = this;
    }

    public override void Update()
    {
        new MenuScene().Start();
    }
}