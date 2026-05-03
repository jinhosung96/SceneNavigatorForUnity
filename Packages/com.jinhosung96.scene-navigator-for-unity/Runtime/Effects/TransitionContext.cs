using System;
using System.Threading;
using UnityEngine;
#if R3_SUPPORT
using R3;
#endif

namespace SceneNavigator
{
    public sealed class TransitionContext
    {
        public SceneNodeData Prev { get; }
        public SceneNodeData Next { get; }
        public Transform OverlayRoot { get; }
        public CancellationToken Cancellation { get; }

#if R3_SUPPORT
        public Observable<float> Progress { get; }
        public TransitionContext(SceneNodeData prev, SceneNodeData next, Transform overlayRoot,
            Observable<float> progress, CancellationToken ct)
        {
            Prev = prev; Next = next; OverlayRoot = overlayRoot;
            Progress = progress; Cancellation = ct;
        }
#else
        public IObservable<float> Progress { get; }
        public TransitionContext(SceneNodeData prev, SceneNodeData next, Transform overlayRoot,
            IObservable<float> progress, CancellationToken ct)
        {
            Prev = prev; Next = next; OverlayRoot = overlayRoot;
            Progress = progress; Cancellation = ct;
        }
#endif
    }
}
