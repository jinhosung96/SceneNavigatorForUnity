namespace SceneNavigator
{
    /// <summary>
    /// On-demand scene loaded/unloaded explicitly by <c>Load&lt;T&gt;()</c> / <c>Unload&lt;T&gt;()</c>.
    /// Lifetime is independent from Main transitions.
    /// </summary>
    public abstract class InstanceSceneNode : SceneNode
    {
    }
}
