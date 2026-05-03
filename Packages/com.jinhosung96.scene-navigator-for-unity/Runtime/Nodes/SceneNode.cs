using System.Threading;
using UnityEngine;
#if UNITASK_SUPPORT
using Cysharp.Threading.Tasks;
#else
using System.Threading.Tasks;
#endif

namespace SceneNavigator
{
    /// <summary>
    /// Base class for every category of SceneNode. All concrete subclasses must derive from one of
    /// <see cref="BaseSceneNode"/>, <see cref="MainSceneNode"/>, <see cref="SubSceneNode"/>,
    /// or <see cref="InstanceSceneNode"/>. Direct subclassing of SceneNode is not supported.
    /// </summary>
    public abstract class SceneNode : MonoBehaviour
    {
        public SceneNodeData Data { get; internal set; }

#if UNITASK_SUPPORT
        protected virtual UniTask OnNodeLoadedAsync   (CancellationToken ct) => UniTask.CompletedTask;
        protected virtual UniTask OnNodeUnloadingAsync(CancellationToken ct) => UniTask.CompletedTask;

        internal UniTask InvokeLoadedAsync   (CancellationToken ct) => OnNodeLoadedAsync(ct);
        internal UniTask InvokeUnloadingAsync(CancellationToken ct) => OnNodeUnloadingAsync(ct);
#else
        protected virtual Task OnNodeLoadedAsync   (CancellationToken ct) => Task.CompletedTask;
        protected virtual Task OnNodeUnloadingAsync(CancellationToken ct) => Task.CompletedTask;

        internal Task InvokeLoadedAsync   (CancellationToken ct) => OnNodeLoadedAsync(ct);
        internal Task InvokeUnloadingAsync(CancellationToken ct) => OnNodeUnloadingAsync(ct);
#endif

        protected virtual void OnDestroy()
        {
            if (Data != null && Data.Node == this)
            {
                Data.Node = null;
                Data.UnityScene = null;
            }
        }
    }
}
