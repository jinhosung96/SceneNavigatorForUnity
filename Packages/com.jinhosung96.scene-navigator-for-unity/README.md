# Scene Navigator for Unity

> [한국어 README](README.ko.md)

Multi-scene management framework for Unity. Classifies scenes into **Base / Main / Sub / Instance**, provides type-based async transition API, Main transition history, R3-based lifecycle events, and pluggable transition effects.

```csharp
await SceneNavigator.Instance.Transition<MainGameScene>();
await SceneNavigator.Instance.Transition<TitleScene>(TransitionEffects.Fade(0.4f));
await SceneNavigator.Instance.Back();
var pause = await SceneNavigator.Instance.Load<PauseScene>();
await SceneNavigator.Instance.Unload<PauseScene>();
```

## Requirements

- Unity **2021.3 LTS** or newer
- [R3](https://github.com/Cysharp/R3) (`com.cysharp.r3`)
- [UniTask](https://github.com/Cysharp/UniTask) (`com.cysharp.unitask`)

## Installation

Add this entry to your project's `Packages/manifest.json`:

```json
{
  "dependencies": {
    "com.jinhosung96.scene-navigator-for-unity": "https://github.com/jinhosung96/SceneNavigatorForUnity.git?path=Packages/com.jinhosung96.scene-navigator-for-unity"
  }
}
```

R3 and UniTask must already be installed in your project.

## Concepts

| Category | Lifetime | Notes |
|---|---|---|
| **Base** (`BaseSceneNode`) | Loaded once, never unloaded | Hosts the `SceneNavigator` instance and the effect overlay. Single per project. |
| **Main** (`MainSceneNode`) | Subject of transitions; one alive at a time | Owns a list of associated Sub types. |
| **Sub** (`SubSceneNode`) | Loaded Additive together with its Main | Reused across Mains that share the same Sub type, or recreated, per `ReusePolicy`. |
| **Instance** (`InstanceSceneNode`) | On-demand | Loaded/unloaded explicitly with `Load<T>()` / `Unload<T>()`. |

All five classes (`SceneNode`, `BaseSceneNode`, `MainSceneNode`, `SubSceneNode`, `InstanceSceneNode`) are **abstract**. Always derive your own subclass and attach it to a scene.

## Quick Start

1. Write your subclasses:
   ```csharp
   public sealed class MyBase     : BaseSceneNode { }
   public sealed class MainGame   : MainSceneNode { }
   public sealed class HUDOverlay : SubSceneNode  { }
   public sealed class PausePopup : InstanceSceneNode { }
   ```
2. Create a scene per subclass and attach the component.
3. Run **`Tools > Scene Navigator > Rebuild Catalog`**. The catalog is auto-created at `Assets/Resources/SceneNavigator/SceneCatalog.asset`.
4. Open the Base scene, select your `MyBase` GameObject, pick `MainGame` as **Startup Main**, register `HUDOverlay` under **Sub Scene Nodes** of `MainGame`.
5. Press Play. From any script:
   ```csharp
   await SceneNavigator.Instance.Transition<MainGame>();
   ```

## API Reference

```csharp
public interface ISceneNavigator : ISceneEvents
{
    SceneNodeData             Current { get; }
    IReadOnlyList<SceneNodeData> History { get; }
    bool                      CanGoBack { get; }
    bool                      IsTransitioning { get; }

    UniTask Transition<TMain>(ITransitionEffect effect = null,
                              bool recordHistory = true,
                              CancellationToken ct = default) where TMain : MainSceneNode;

    UniTask Back(ITransitionEffect effect = null, CancellationToken ct = default);

    UniTask ReloadMain<TMain>(ITransitionEffect effect = null,
                              CancellationToken ct = default) where TMain : MainSceneNode;
    UniTask ReloadSub <TSub>(CancellationToken ct = default) where TSub  : SubSceneNode;
    UniTask ReloadInstance<TInst>(CancellationToken ct = default) where TInst : InstanceSceneNode;

    UniTask<TInst> Load  <TInst>(CancellationToken ct = default) where TInst : InstanceSceneNode;
    UniTask        Unload<TInst>(CancellationToken ct = default) where TInst : InstanceSceneNode;
}
```

### Concurrency policy

A second navigation call while one is in flight is **rejected** with `NavigatorBusyException`. Check `IsTransitioning` before calling for double-click guards.

### Calling `Transition<TCurrent>()`

If the requested Main is already current, the call is a no-op + info log. Use `ReloadMain<T>()` to force unload + reload.

## Async Hooks

Override on any SceneNode subclass; the framework awaits these before continuing.

```csharp
public sealed class MainGame : MainSceneNode
{
    protected override async UniTask OnNodeLoadedAsync(CancellationToken ct)
    {
        await PreloadAssetsAsync(ct);   // splash / heavy init
    }

    protected override async UniTask OnNodeUnloadingAsync(CancellationToken ct)
    {
        await SaveProgressAsync(ct);    // graceful teardown
    }
}
```

## Lifecycle Events (R3)

```
Per-node load:    OnLoadedBegin → OnNodeLoadedAsync → OnLoadedEnd
Per-node unload:  OnUnloadBegin → OnNodeUnloadingAsync → OnUnloadEnd → scene unload
Main transition:  OnLoading(prev, next, progress 0..1) — single stream
```

```csharp
SceneNavigator.Instance.OnLoadedEnd
    .Where(d => d.Kind == SceneNodeKind.Main)
    .Subscribe(d => Debug.Log($"Main loaded: {d.NodeType.Name}"));
```

When R3 is not installed, the same five members fall back to `event Action<...>`.

## Transition Effects

```csharp
TransitionEffects.None                                     // instant
TransitionEffects.Fade(0.3f)                               // black fade
TransitionEffects.Fade(0.3f, Color.white)                  // colored fade
TransitionEffects.FadeOut(0.2f)
TransitionEffects.FadeIn (0.5f)
TransitionEffects.Sequence(out, custom, in)
TransitionEffects.Parallel(a, b)
TransitionEffects.FromAction(playOut, playIn)              // ad-hoc
```

Custom effect:

```csharp
public sealed class MyDissolve : ITransitionEffect
{
    public UniTask PlayOut(TransitionContext ctx) { /* fade screen */ }
    public UniTask PlayIn (TransitionContext ctx) { /* reveal */ }
}
```

`TransitionContext` exposes `Prev`, `Next`, `OverlayRoot`, `Cancellation`, and a `Progress` observable. Spawn UI under `OverlayRoot` (a Screen-Space-Overlay Canvas; auto-created or supplied through `BaseSceneNode.overlayRoot`).

## Sub Reuse Policy

Each `SubSceneNode` exposes a `ReusePolicy`:

- **Reuse** (default): on Main switch, if the next Main also lists this Sub type, the Sub stays alive.
- **Recreate**: always unload+load on every Main switch.

## Conditional Compilation

The package is gated by `R3_SUPPORT` (set automatically when `com.cysharp.r3 >= 1.0.0` is installed) and `UNITASK_SUPPORT` (`com.cysharp.unitask >= 2.0.0`).

## Sample

`Window > Package Manager > Scene Navigator > Samples > Minimal` imports a 5-script skeleton (Base, two Mains, one shared Sub, one Instance). Build the matching scenes, attach the components, and rebuild the catalog.

## Roadmap

- v0.2: full PlayMode test fixtures (transition flow, async hooks, reload), inspector improvements
- v0.3: `Reload<T>()` overload that unifies categories via constraint resolution; ScrollableSceneCatalog inspector

## License

MIT — see [LICENSE](LICENSE).
