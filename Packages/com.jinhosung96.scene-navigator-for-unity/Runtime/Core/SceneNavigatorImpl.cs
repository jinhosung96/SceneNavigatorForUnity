using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using UnityEngine;
using UnityEngine.SceneManagement;
#if UNITASK_SUPPORT
using Cysharp.Threading.Tasks;
#else
using System.Threading.Tasks;
#endif
#if R3_SUPPORT
using R3;
#endif

namespace SceneNavigator
{
    /// <summary>
    /// Default ISceneNavigator implementation. Created and registered by <see cref="BaseSceneNode"/>
    /// during its Awake. Single global instance accessed via <see cref="SceneNavigator.Instance"/>.
    /// </summary>
    public sealed class SceneNavigatorImpl : ISceneNavigator
    {
        private readonly SceneCatalog _catalog;
        private readonly BaseSceneNode _baseNode;
        private readonly SceneEventHub _events;
        private readonly NavigationHistory _history = new NavigationHistory();
        private readonly Dictionary<Type, SceneNodeData> _byType;

        private int _busy;
        private SceneNodeData _current;

        // ----- ISceneEvents pass-through -----
#if R3_SUPPORT
        public Observable<SceneNodeData> OnLoadedBegin => _events.OnLoadedBegin;
        public Observable<SceneNodeData> OnLoadedEnd   => _events.OnLoadedEnd;
        public Observable<SceneNodeData> OnUnloadBegin => _events.OnUnloadBegin;
        public Observable<SceneNodeData> OnUnloadEnd   => _events.OnUnloadEnd;
        public Observable<TransitionProgress> OnLoading => _events.OnLoading;
#else
        public event Action<SceneNodeData> OnLoadedBegin
        { add => _events.OnLoadedBegin += value; remove => _events.OnLoadedBegin -= value; }
        public event Action<SceneNodeData> OnLoadedEnd
        { add => _events.OnLoadedEnd += value; remove => _events.OnLoadedEnd -= value; }
        public event Action<SceneNodeData> OnUnloadBegin
        { add => _events.OnUnloadBegin += value; remove => _events.OnUnloadBegin -= value; }
        public event Action<SceneNodeData> OnUnloadEnd
        { add => _events.OnUnloadEnd += value; remove => _events.OnUnloadEnd -= value; }
        public event Action<TransitionProgress> OnLoading
        { add => _events.OnLoading += value; remove => _events.OnLoading -= value; }
#endif

        // ----- ISceneNavigator state -----
        public SceneNodeData Current => _current;
        public IReadOnlyList<SceneNodeData> History => _history.Items;
        public bool CanGoBack => !_history.IsEmpty;
        public bool IsTransitioning => _busy != 0;

        private SceneNavigatorImpl(SceneCatalog catalog, BaseSceneNode baseNode)
        {
            _catalog = catalog;
            _baseNode = baseNode;
            _events = new SceneEventHub();
            _byType = new Dictionary<Type, SceneNodeData>(catalog.Entries.Count);
            foreach (var e in catalog.Entries)
            {
                if (string.IsNullOrEmpty(e.typeAssemblyQualifiedName) || string.IsNullOrEmpty(e.scenePath)) continue;
                var t = Type.GetType(e.typeAssemblyQualifiedName, throwOnError: false);
                if (t == null) continue;
                if (_byType.ContainsKey(t)) continue;
                _byType.Add(t, new SceneNodeData(t, e.kind, e.scenePath));
            }

            // Hook scene unload to keep SceneNodeData consistent.
            SceneManager.sceneUnloaded += OnSceneUnloaded;
        }

        // ===================================================================================
        // Bootstrap
        // ===================================================================================
        internal static void BootstrapFrom(BaseSceneNode baseNode)
        {
            var settings = SceneNavigatorSettings.GetOrDefault();
            var catalog = Resources.Load<SceneCatalog>(settings.CatalogResourcesPath);
            if (catalog == null)
            {
                Debug.LogError(
                    $"[SceneNavigator] SceneCatalog not found at Resources path '{settings.CatalogResourcesPath}'. " +
                    "Run 'Tools/Scene Navigator/Rebuild Catalog' or check SceneNavigatorSettings.");
                return;
            }

            var impl = new SceneNavigatorImpl(catalog, baseNode);
            SceneNavigator.Instance = impl;

            // Wire BaseSceneNode itself if it is in the catalog
            var baseType = baseNode.GetType();
            if (impl._byType.TryGetValue(baseType, out var baseData))
            {
                baseData.Node = baseNode;
                baseData.UnityScene = baseNode.gameObject.scene;
                baseNode.Data = baseData;
            }

            impl.RunStartupAsync(baseNode).Forget();
        }

#if UNITASK_SUPPORT
        private async UniTaskVoid RunStartupAsync(BaseSceneNode baseNode)
        {
            try
            {
                Type startMain = ResolveStartupMain(baseNode);
                if (startMain == null)
                {
                    Debug.LogWarning("[SceneNavigator] No startup Main scene resolved.");
                    return;
                }
                if (!_byType.TryGetValue(startMain, out var data))
                {
                    Debug.LogError($"[SceneNavigator] Startup main '{startMain.FullName}' not registered in catalog.");
                    return;
                }
                await TransitionInternalAsync(data, effect: null, recordHistory: false, ct: default);
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
        }
#else
        private async void RunStartupAsync(BaseSceneNode baseNode)
        {
            try
            {
                Type startMain = ResolveStartupMain(baseNode);
                if (startMain == null) return;
                if (!_byType.TryGetValue(startMain, out var data)) return;
                await TransitionInternalAsync(data, null, false, default);
            }
            catch (Exception e) { Debug.LogException(e); }
        }
#endif

        private Type ResolveStartupMain(BaseSceneNode baseNode)
        {
            // 1) If a Main scene is currently loaded (e.g. user opened a Main scene in editor and pressed Play),
            //    use it.
            for (int i = 0; i < SceneManager.sceneCount; i++)
            {
                var s = SceneManager.GetSceneAt(i);
                if (!s.isLoaded) continue;
                var main = FindMainNodeIn(s);
                if (main != null) return main.GetType();
            }
            // 2) Otherwise fall back to the StartupMain configured on BaseSceneNode.
            return baseNode.StartupMain;
        }

        private static MainSceneNode FindMainNodeIn(Scene scene)
        {
            var roots = scene.GetRootGameObjects();
            for (int i = 0; i < roots.Length; i++)
            {
                var n = roots[i].GetComponent<MainSceneNode>();
                if (n != null) return n;
            }
            return null;
        }

        // ===================================================================================
        // Public API
        // ===================================================================================
#if UNITASK_SUPPORT
        public UniTask Transition<TMain>(ITransitionEffect effect = null, bool recordHistory = true,
            CancellationToken ct = default) where TMain : MainSceneNode
        {
            if (!_byType.TryGetValue(typeof(TMain), out var data))
                throw new SceneNotRegisteredException(typeof(TMain));
            return TransitionInternalAsync(data, effect, recordHistory, ct);
        }

        public UniTask Back(ITransitionEffect effect = null, CancellationToken ct = default)
        {
            if (_history.IsEmpty)
                throw new InvalidOperationException("Navigation history is empty. Check CanGoBack first.");
            var prev = _history.Pop();
            return TransitionInternalAsync(prev, effect, recordHistory: false, ct);
        }

        public UniTask ReloadMain<TMain>(ITransitionEffect effect = null, CancellationToken ct = default)
            where TMain : MainSceneNode
        {
            if (_current == null || _current.NodeType != typeof(TMain))
                throw new InvalidOperationException(
                    $"ReloadMain<{typeof(TMain).Name}> called but current Main is " +
                    (_current == null ? "<none>" : _current.NodeType.Name));
            return ReloadMainInternalAsync(_current, effect, ct);
        }

        public UniTask ReloadSub<TSub>(CancellationToken ct = default) where TSub : SubSceneNode =>
            ReloadSimpleAsync(typeof(TSub), ct);

        public UniTask ReloadInstance<TInst>(CancellationToken ct = default) where TInst : InstanceSceneNode =>
            ReloadSimpleAsync(typeof(TInst), ct);

        public async UniTask<TInst> Load<TInst>(CancellationToken ct = default) where TInst : InstanceSceneNode
        {
            EnterCritical();
            try
            {
                if (!_byType.TryGetValue(typeof(TInst), out var data))
                    throw new SceneNotRegisteredException(typeof(TInst));
                await LoadNodeAsync(data, ct);
                return (TInst)data.Node;
            }
            finally { ExitCritical(); }
        }

        public async UniTask Unload<TInst>(CancellationToken ct = default) where TInst : InstanceSceneNode
        {
            EnterCritical();
            try
            {
                if (!_byType.TryGetValue(typeof(TInst), out var data))
                    throw new SceneNotRegisteredException(typeof(TInst));
                if (!data.IsAlive)
                {
                    Debug.Log($"[SceneNavigator] Unload<{typeof(TInst).Name}>: not loaded, ignoring.");
                    return;
                }
                await UnloadNodeAsync(data, ct);
            }
            finally { ExitCritical(); }
        }
#else
        public Task Transition<TMain>(ITransitionEffect effect = null, bool recordHistory = true,
            CancellationToken ct = default) where TMain : MainSceneNode
        {
            if (!_byType.TryGetValue(typeof(TMain), out var data))
                throw new SceneNotRegisteredException(typeof(TMain));
            return TransitionInternalAsync(data, effect, recordHistory, ct);
        }

        public Task Back(ITransitionEffect effect = null, CancellationToken ct = default)
        {
            if (_history.IsEmpty)
                throw new InvalidOperationException("Navigation history is empty.");
            var prev = _history.Pop();
            return TransitionInternalAsync(prev, effect, false, ct);
        }

        public Task ReloadMain<TMain>(ITransitionEffect effect = null, CancellationToken ct = default)
            where TMain : MainSceneNode
        {
            if (_current == null || _current.NodeType != typeof(TMain))
                throw new InvalidOperationException("ReloadMain target mismatch.");
            return ReloadMainInternalAsync(_current, effect, ct);
        }

        public Task ReloadSub<TSub>(CancellationToken ct = default) where TSub : SubSceneNode =>
            ReloadSimpleAsync(typeof(TSub), ct);

        public Task ReloadInstance<TInst>(CancellationToken ct = default) where TInst : InstanceSceneNode =>
            ReloadSimpleAsync(typeof(TInst), ct);

        public async Task<TInst> Load<TInst>(CancellationToken ct = default) where TInst : InstanceSceneNode
        {
            EnterCritical();
            try
            {
                if (!_byType.TryGetValue(typeof(TInst), out var data))
                    throw new SceneNotRegisteredException(typeof(TInst));
                await LoadNodeAsync(data, ct);
                return (TInst)data.Node;
            }
            finally { ExitCritical(); }
        }

        public async Task Unload<TInst>(CancellationToken ct = default) where TInst : InstanceSceneNode
        {
            EnterCritical();
            try
            {
                if (!_byType.TryGetValue(typeof(TInst), out var data))
                    throw new SceneNotRegisteredException(typeof(TInst));
                if (!data.IsAlive) return;
                await UnloadNodeAsync(data, ct);
            }
            finally { ExitCritical(); }
        }
#endif

        // ===================================================================================
        // Concurrency gate (#6 — reject)
        // ===================================================================================
        private void EnterCritical()
        {
            if (Interlocked.CompareExchange(ref _busy, 1, 0) != 0)
                throw new NavigatorBusyException();
        }
        private void ExitCritical() => Interlocked.Exchange(ref _busy, 0);

        // ===================================================================================
        // Main Transition / Reload internals
        // ===================================================================================
#if UNITASK_SUPPORT
        private async UniTask TransitionInternalAsync(SceneNodeData target, ITransitionEffect effect,
            bool recordHistory, CancellationToken ct)
#else
        private async Task TransitionInternalAsync(SceneNodeData target, ITransitionEffect effect,
            bool recordHistory, CancellationToken ct)
#endif
        {
            EnterCritical();
            try
            {
                // Same Main → no-op (#9)
                if (_current != null && _current.NodeType == target.NodeType)
                {
                    Debug.Log($"[SceneNavigator] Already on {target.NodeType.Name}. Use ReloadMain<T>() to force reload.");
                    return;
                }

                effect = effect ?? _baseNode.DefaultEffect ?? TransitionEffects.None;
                var ctx = new TransitionContext(_current, target, _baseNode.ResolveOverlayRoot(), null, ct);

                await effect.PlayOut(ctx);

                // Reconcile sub scenes
                var (toUnload, toLoad, recreatePairs, reuseSet) =
                    ReconcileSubs(_current, FindMainTypeForNode(target));

                // Phase: unload outgoing
                if (_current != null) await UnloadNodeAsync(_current, ct);
                foreach (var sub in toUnload) await UnloadNodeAsync(sub, ct);
                foreach (var pair in recreatePairs) await UnloadNodeAsync(pair, ct);

                // Phase: load incoming
                await LoadNodeAsync(target, ct);
                foreach (var sub in toLoad) await LoadNodeAsync(sub, ct);
                foreach (var pair in recreatePairs) await LoadNodeAsync(pair, ct);

                if (recordHistory && _current != null) _history.Push(_current);
                var prev = _current;
                _current = target;

                _events.EmitLoading(new TransitionProgress(prev, target, 1f));

                await effect.PlayIn(ctx);
            }
            finally { ExitCritical(); }
        }

#if UNITASK_SUPPORT
        private async UniTask ReloadMainInternalAsync(SceneNodeData mainData, ITransitionEffect effect, CancellationToken ct)
#else
        private async Task ReloadMainInternalAsync(SceneNodeData mainData, ITransitionEffect effect, CancellationToken ct)
#endif
        {
            EnterCritical();
            try
            {
                effect = effect ?? _baseNode.DefaultEffect ?? TransitionEffects.None;
                var ctx = new TransitionContext(mainData, mainData, _baseNode.ResolveOverlayRoot(), null, ct);
                await effect.PlayOut(ctx);

                // Unload all live subs + main, then reload all (ignoring Reuse policy by design).
                var aliveSubs = _byType.Values
                    .Where(d => d.Kind == SceneNodeKind.Sub && d.IsAlive)
                    .ToList();
                foreach (var s in aliveSubs) await UnloadNodeAsync(s, ct);
                await UnloadNodeAsync(mainData, ct);

                await LoadNodeAsync(mainData, ct);
                var freshMain = mainData.Node as MainSceneNode;
                if (freshMain != null)
                {
                    foreach (var subType in freshMain.SubSceneNodeTypes)
                    {
                        if (_byType.TryGetValue(subType, out var subData))
                            await LoadNodeAsync(subData, ct);
                    }
                }

                await effect.PlayIn(ctx);
            }
            finally { ExitCritical(); }
        }

#if UNITASK_SUPPORT
        private async UniTask ReloadSimpleAsync(Type nodeType, CancellationToken ct)
#else
        private async Task ReloadSimpleAsync(Type nodeType, CancellationToken ct)
#endif
        {
            EnterCritical();
            try
            {
                if (!_byType.TryGetValue(nodeType, out var data) || !data.IsAlive)
                {
                    Debug.Log($"[SceneNavigator] Reload<{nodeType.Name}>: not loaded, ignoring.");
                    return;
                }
                await UnloadNodeAsync(data, ct);
                await LoadNodeAsync(data, ct);
            }
            finally { ExitCritical(); }
        }

        // ===================================================================================
        // Sub reconciliation
        // ===================================================================================
        private (List<SceneNodeData> toUnload,
                 List<SceneNodeData> toLoad,
                 List<SceneNodeData> recreatePairs,
                 HashSet<Type> reuseSet)
        ReconcileSubs(SceneNodeData currentMain, MainSceneNode prototype)
        {
            var toUnload = new List<SceneNodeData>();
            var toLoad = new List<SceneNodeData>();
            var recreate = new List<SceneNodeData>();
            var reuseSet = new HashSet<Type>();

            // current sub types
            var currentSubs = _byType.Values
                .Where(d => d.Kind == SceneNodeKind.Sub && d.IsAlive)
                .Select(d => d.NodeType)
                .ToHashSet();

            // target sub types
            var targetSubs = new HashSet<Type>();
            if (prototype != null)
            {
                foreach (var t in prototype.SubSceneNodeTypes) targetSubs.Add(t);
            }

            foreach (var t in currentSubs)
            {
                if (!targetSubs.Contains(t))
                {
                    if (_byType.TryGetValue(t, out var d)) toUnload.Add(d);
                }
                else
                {
                    var liveSub = _byType[t].Node as SubSceneNode;
                    var policy = liveSub != null ? liveSub.ReusePolicy : SubReusePolicy.Reuse;
                    if (policy == SubReusePolicy.Recreate) recreate.Add(_byType[t]);
                    else reuseSet.Add(t);
                }
            }
            foreach (var t in targetSubs)
            {
                if (!currentSubs.Contains(t))
                {
                    if (_byType.TryGetValue(t, out var d)) toLoad.Add(d);
                }
            }
            return (toUnload, toLoad, recreate, reuseSet);
        }

        // Create a transient prototype to read its sub list. We avoid loading the scene just to
        // read [SerializeField], so we prefer to load it lazily — but ReconcileSubs is called
        // before load. Workaround: cache the latest known instance per type.
        private MainSceneNode FindMainTypeForNode(SceneNodeData data)
        {
            if (data == null) return null;
            // If already cached/loaded somewhere, use it.
            if (data.Node is MainSceneNode m) return m;
            // Fall back: probe currently loaded scenes.
            for (int i = 0; i < SceneManager.sceneCount; i++)
            {
                var s = SceneManager.GetSceneAt(i);
                if (!s.isLoaded) continue;
                var roots = s.GetRootGameObjects();
                for (int j = 0; j < roots.Length; j++)
                {
                    var n = roots[j].GetComponent(data.NodeType) as MainSceneNode;
                    if (n != null) return n;
                }
            }
            return null;
        }

        // ===================================================================================
        // Per-node Load / Unload micro-sequences
        // ===================================================================================
#if UNITASK_SUPPORT
        private async UniTask LoadNodeAsync(SceneNodeData data, CancellationToken ct)
#else
        private async Task LoadNodeAsync(SceneNodeData data, CancellationToken ct)
#endif
        {
            var op = SceneManager.LoadSceneAsync(data.ScenePath, LoadSceneMode.Additive);
            if (op == null)
                throw new InvalidOperationException(
                    $"[SceneNavigator] LoadSceneAsync returned null for '{data.ScenePath}'. " +
                    "Is the scene added to Build Settings?");
#if UNITASK_SUPPORT
            await op.ToUniTask(cancellationToken: ct);
#else
            while (!op.isDone)
            {
                if (ct.IsCancellationRequested) ct.ThrowIfCancellationRequested();
                await Task.Yield();
            }
#endif
            var loaded = SceneManager.GetSceneByPath(data.ScenePath);
            data.UnityScene = loaded;
            var node = FindNodeIn(loaded, data.NodeType);
            if (node != null)
            {
                data.Node = node;
                node.Data = data;
            }

            _events.EmitLoadedBegin(data);
            if (node != null) await node.InvokeLoadedAsync(ct);
            _events.EmitLoadedEnd(data);
        }

#if UNITASK_SUPPORT
        private async UniTask UnloadNodeAsync(SceneNodeData data, CancellationToken ct)
#else
        private async Task UnloadNodeAsync(SceneNodeData data, CancellationToken ct)
#endif
        {
            _events.EmitUnloadBegin(data);
            if (data.Node != null) await data.Node.InvokeUnloadingAsync(ct);
            _events.EmitUnloadEnd(data);

            if (data.UnityScene.HasValue && data.UnityScene.Value.IsValid() && data.UnityScene.Value.isLoaded)
            {
                var op = SceneManager.UnloadSceneAsync(data.UnityScene.Value);
                if (op != null)
                {
#if UNITASK_SUPPORT
                    await op.ToUniTask(cancellationToken: ct);
#else
                    while (!op.isDone) { if (ct.IsCancellationRequested) ct.ThrowIfCancellationRequested(); await Task.Yield(); }
#endif
                }
            }
            data.Node = null;
            data.UnityScene = null;
        }

        private static SceneNode FindNodeIn(Scene scene, Type nodeType)
        {
            var roots = scene.GetRootGameObjects();
            for (int i = 0; i < roots.Length; i++)
            {
                var c = roots[i].GetComponent(nodeType) as SceneNode;
                if (c != null) return c;
                var inChildren = roots[i].GetComponentInChildren(nodeType, includeInactive: true) as SceneNode;
                if (inChildren != null) return inChildren;
            }
            return null;
        }

        private void OnSceneUnloaded(Scene s)
        {
            // Defensive: if Unity unloaded a scene outside our flow, drop the data refs.
            foreach (var d in _byType.Values)
            {
                if (d.UnityScene.HasValue && d.UnityScene.Value == s)
                {
                    d.Node = null;
                    d.UnityScene = null;
                }
            }
        }
    }
}
