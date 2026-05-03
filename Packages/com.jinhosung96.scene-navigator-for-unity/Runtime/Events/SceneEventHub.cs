using System;
#if R3_SUPPORT
using R3;
#endif

namespace SceneNavigator
{
    internal sealed class SceneEventHub : ISceneEvents, IDisposable
    {
#if R3_SUPPORT
        private readonly Subject<SceneNodeData> _onLoadedBegin = new Subject<SceneNodeData>();
        private readonly Subject<SceneNodeData> _onLoadedEnd   = new Subject<SceneNodeData>();
        private readonly Subject<SceneNodeData> _onUnloadBegin = new Subject<SceneNodeData>();
        private readonly Subject<SceneNodeData> _onUnloadEnd   = new Subject<SceneNodeData>();
        private readonly Subject<TransitionProgress> _onLoading = new Subject<TransitionProgress>();

        public Observable<SceneNodeData> OnLoadedBegin => _onLoadedBegin;
        public Observable<SceneNodeData> OnLoadedEnd   => _onLoadedEnd;
        public Observable<SceneNodeData> OnUnloadBegin => _onUnloadBegin;
        public Observable<SceneNodeData> OnUnloadEnd   => _onUnloadEnd;
        public Observable<TransitionProgress> OnLoading => _onLoading;

        internal void EmitLoadedBegin(SceneNodeData d) => _onLoadedBegin.OnNext(d);
        internal void EmitLoadedEnd  (SceneNodeData d) => _onLoadedEnd.OnNext(d);
        internal void EmitUnloadBegin(SceneNodeData d) => _onUnloadBegin.OnNext(d);
        internal void EmitUnloadEnd  (SceneNodeData d) => _onUnloadEnd.OnNext(d);
        internal void EmitLoading    (TransitionProgress p) => _onLoading.OnNext(p);

        public void Dispose()
        {
            _onLoadedBegin.Dispose();
            _onLoadedEnd.Dispose();
            _onUnloadBegin.Dispose();
            _onUnloadEnd.Dispose();
            _onLoading.Dispose();
        }
#else
        public event Action<SceneNodeData> OnLoadedBegin;
        public event Action<SceneNodeData> OnLoadedEnd;
        public event Action<SceneNodeData> OnUnloadBegin;
        public event Action<SceneNodeData> OnUnloadEnd;
        public event Action<TransitionProgress> OnLoading;

        internal void EmitLoadedBegin(SceneNodeData d) => OnLoadedBegin?.Invoke(d);
        internal void EmitLoadedEnd  (SceneNodeData d) => OnLoadedEnd?.Invoke(d);
        internal void EmitUnloadBegin(SceneNodeData d) => OnUnloadBegin?.Invoke(d);
        internal void EmitUnloadEnd  (SceneNodeData d) => OnUnloadEnd?.Invoke(d);
        internal void EmitLoading    (TransitionProgress p) => OnLoading?.Invoke(p);

        public void Dispose()
        {
            OnLoadedBegin = null;
            OnLoadedEnd = null;
            OnUnloadBegin = null;
            OnUnloadEnd = null;
            OnLoading = null;
        }
#endif
    }
}
