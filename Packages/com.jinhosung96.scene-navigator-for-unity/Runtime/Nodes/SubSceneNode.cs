using UnityEngine;

namespace SceneNavigator
{
    /// <summary>
    /// Companion scene loaded Additive together with a Main scene. When Main switches, two Mains
    /// that share the same SubSceneNode subclass type are reconciled per <see cref="ReusePolicy"/>.
    /// </summary>
    public abstract class SubSceneNode : SceneNode
    {
        [Tooltip("Reuse: keep the Sub scene alive when transitioning to a Main that also uses it. " +
                 "Recreate: always unload+load even if the next Main uses the same Sub.")]
        [SerializeField] private SubReusePolicy reusePolicy = SubReusePolicy.Reuse;

        public SubReusePolicy ReusePolicy => reusePolicy;
    }
}
