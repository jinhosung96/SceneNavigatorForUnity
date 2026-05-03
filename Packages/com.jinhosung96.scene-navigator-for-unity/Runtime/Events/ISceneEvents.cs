using System;
#if R3_SUPPORT
using R3;
#endif

namespace SceneNavigator
{
    /// <summary>
    /// Lifecycle events for every SceneNode. Two pairs around the user async hooks:
    ///   load   : OnLoadedBegin (before OnNodeLoadedAsync)    -> hook -> OnLoadedEnd
    ///   unload : OnUnloadBegin (before OnNodeUnloadingAsync) -> hook -> OnUnloadEnd
    /// OnLoading streams the Main transition progress (0..1) with prev/next nodes.
    /// </summary>
    public interface ISceneEvents
    {
#if R3_SUPPORT
        Observable<SceneNodeData> OnLoadedBegin { get; }
        Observable<SceneNodeData> OnLoadedEnd   { get; }
        Observable<SceneNodeData> OnUnloadBegin { get; }
        Observable<SceneNodeData> OnUnloadEnd   { get; }
        Observable<TransitionProgress> OnLoading { get; }
#else
        event Action<SceneNodeData> OnLoadedBegin;
        event Action<SceneNodeData> OnLoadedEnd;
        event Action<SceneNodeData> OnUnloadBegin;
        event Action<SceneNodeData> OnUnloadEnd;
        event Action<TransitionProgress> OnLoading;
#endif
    }

    /// <summary>Snapshot for OnLoading.</summary>
    public readonly struct TransitionProgress
    {
        public readonly SceneNodeData Prev;
        public readonly SceneNodeData Next;
        public readonly float Progress;

        public TransitionProgress(SceneNodeData prev, SceneNodeData next, float progress)
        {
            Prev = prev;
            Next = next;
            Progress = progress;
        }
    }
}
