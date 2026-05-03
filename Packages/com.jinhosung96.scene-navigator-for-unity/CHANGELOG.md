# Changelog

All notable changes to this package will be documented here.
Format follows [Keep a Changelog](https://keepachangelog.com/en/1.1.0/) and the project uses [Semantic Versioning](https://semver.org/).

## [0.1.0] — 2026-05-03

### Added
- 4-category scene model: `BaseSceneNode` / `MainSceneNode` / `SubSceneNode` / `InstanceSceneNode` (all `abstract`).
- `ISceneNavigator` API: `Transition<TMain>`, `Back`, `ReloadMain<TMain>`, `ReloadSub<TSub>`, `ReloadInstance<TInst>`, `Load<TInst>`, `Unload<TInst>`.
- Static service-locator entry point `SceneNavigator.Instance`.
- `SceneCatalog` (ScriptableObject) with editor scanner, save-time sync, asset-move postprocessor, build settings preprocessor, and rebuild menu.
- 4 lifecycle events per node — `OnLoadedBegin`, `OnLoadedEnd`, `OnUnloadBegin`, `OnUnloadEnd` — plus a Main-transition `OnLoading(prev, next, progress)` stream. R3 `Observable<T>` when available, `event Action<T>` fallback otherwise.
- Async user hooks: `SceneNode.OnNodeLoadedAsync` / `OnNodeUnloadingAsync`.
- Pluggable transition effects (`ITransitionEffect`) with presets `None`, `Fade`, `FadeOut`, `FadeIn`, `Sequence`, `Parallel`, `FromAction`. Hybrid overlay root (auto-create if user did not assign one).
- Custom inspector for `BaseSceneNode` / `MainSceneNode` (catalog-driven dropdowns).
- Editor PlayMode bootstrap that switches to the Base scene before entering Play.
- EditMode tests for navigation history, name normalizer, catalog integrity.
- PlayMode scaffold tests (smoke + structural).
- Minimal sample skeleton (5 scripts).

### Notes
- VContainer is intentionally not integrated.
- Full PlayMode coverage of transition flows / async hooks / reload requires fixture scenes; planned for v0.2.
