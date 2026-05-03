using System.Collections.Generic;
using System.Threading;
#if UNITASK_SUPPORT
using Cysharp.Threading.Tasks;
#else
using System.Threading.Tasks;
#endif

namespace SceneNavigator
{
    public interface ISceneNavigator : ISceneEvents
    {
        SceneNodeData Current { get; }
        IReadOnlyList<SceneNodeData> History { get; }
        bool CanGoBack { get; }
        bool IsTransitioning { get; }

#if UNITASK_SUPPORT
        UniTask Transition<TMain>(
            ITransitionEffect effect = null,
            bool recordHistory = true,
            CancellationToken ct = default) where TMain : MainSceneNode;

        UniTask Back(
            ITransitionEffect effect = null,
            CancellationToken ct = default);

        UniTask ReloadMain<TMain>(
            ITransitionEffect effect = null,
            CancellationToken ct = default) where TMain : MainSceneNode;

        UniTask ReloadSub<TSub>(CancellationToken ct = default)
            where TSub : SubSceneNode;

        UniTask ReloadInstance<TInst>(CancellationToken ct = default)
            where TInst : InstanceSceneNode;

        UniTask<TInst> Load<TInst>(CancellationToken ct = default)
            where TInst : InstanceSceneNode;

        UniTask Unload<TInst>(CancellationToken ct = default)
            where TInst : InstanceSceneNode;
#else
        Task Transition<TMain>(
            ITransitionEffect effect = null,
            bool recordHistory = true,
            CancellationToken ct = default) where TMain : MainSceneNode;

        Task Back(ITransitionEffect effect = null, CancellationToken ct = default);

        Task ReloadMain<TMain>(ITransitionEffect effect = null, CancellationToken ct = default)
            where TMain : MainSceneNode;

        Task ReloadSub<TSub>(CancellationToken ct = default) where TSub : SubSceneNode;

        Task ReloadInstance<TInst>(CancellationToken ct = default) where TInst : InstanceSceneNode;

        Task<TInst> Load<TInst>(CancellationToken ct = default) where TInst : InstanceSceneNode;

        Task Unload<TInst>(CancellationToken ct = default) where TInst : InstanceSceneNode;
#endif
    }

    /// <summary>Static service-locator-style entry point. Set internally by <see cref="BaseSceneNode"/>.</summary>
    public static class SceneNavigator
    {
        public static ISceneNavigator Instance { get; internal set; }
    }
}
