namespace nestphalia;

public abstract class Scene
{
    // public abstract void Start();
    // Scenes should all have a start function, but these aren't used generically so they don't need to follow a standard pattern.
    // Scene start functions are called *to* start the scene, and need to set Program.CurrentScene to their own object
    public abstract void Update();
}