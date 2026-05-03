# Scene Navigator for Unity

> [English README](README.md)

Unity용 멀티 씬 관리 프레임워크. 씬을 **Base / Main / Sub / Instance** 4개 카테고리로 분류하고, 타입 기반 비동기 전환 API, Main 전환 히스토리, R3 기반 라이프사이클 이벤트, 교체 가능한 전환 연출을 제공합니다.

```csharp
await SceneNavigator.Instance.Transition<MainGameScene>();
await SceneNavigator.Instance.Transition<TitleScene>(TransitionEffects.Fade(0.4f));
await SceneNavigator.Instance.Back();
var pause = await SceneNavigator.Instance.Load<PauseScene>();
await SceneNavigator.Instance.Unload<PauseScene>();
```

## 요구 사항

- Unity **2021.3 LTS** 이상
- [R3](https://github.com/Cysharp/R3) (`com.cysharp.r3`)
- [UniTask](https://github.com/Cysharp/UniTask) (`com.cysharp.unitask`)

## 설치

프로젝트의 `Packages/manifest.json`에 다음 항목을 추가하세요.

```json
{
  "dependencies": {
    "com.jinhosung96.scene-navigator-for-unity": "https://github.com/jinhosung96/SceneNavigatorForUnity.git?path=Packages/com.jinhosung96.scene-navigator-for-unity"
  }
}
```

R3와 UniTask는 미리 설치되어 있어야 합니다.

## 개념

| 카테고리 | 라이프타임 | 메모 |
|---|---|---|
| **Base** (`BaseSceneNode`) | 1회 로드, 절대 언로드되지 않음 | `SceneNavigator` 인스턴스와 연출 오버레이를 보유. 프로젝트당 1개. |
| **Main** (`MainSceneNode`) | 전환의 주체. 동시에 1개만 활성 | 자신과 함께 로드될 Sub 타입 목록을 보유. |
| **Sub** (`SubSceneNode`) | Main과 함께 Additive 로드 | 같은 Sub 타입을 공유하는 Main으로 전환 시 `ReusePolicy`에 따라 재사용/재생성. |
| **Instance** (`InstanceSceneNode`) | 명시적 | `Load<T>()` / `Unload<T>()`로 직접 로드/언로드. |

5개 클래스(`SceneNode`, `BaseSceneNode`, `MainSceneNode`, `SubSceneNode`, `InstanceSceneNode`) 모두 **abstract**입니다. 항상 자신의 서브클래스를 만들어 씬에 부착하세요.

## 빠른 시작

1. 서브클래스 작성:
   ```csharp
   public sealed class MyBase     : BaseSceneNode { }
   public sealed class MainGame   : MainSceneNode { }
   public sealed class HUDOverlay : SubSceneNode  { }
   public sealed class PausePopup : InstanceSceneNode { }
   ```
2. 각 서브클래스마다 씬을 만들고 컴포넌트를 부착합니다.
3. **`Tools > Scene Navigator > Rebuild Catalog`**를 실행하세요. 카탈로그는 `Assets/Resources/SceneNavigator/SceneCatalog.asset`에 자동 생성됩니다.
4. Base 씬을 열어 `MyBase` GameObject를 선택하고 **Startup Main**으로 `MainGame`을 지정합니다. 그리고 `MainGame`의 **Sub Scene Nodes**에 `HUDOverlay`를 등록합니다.
5. 플레이. 어디서든 호출 가능합니다.
   ```csharp
   await SceneNavigator.Instance.Transition<MainGame>();
   ```

## API 레퍼런스

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

### 동시성 정책

이미 다른 전환이 진행 중인 상태에서 새 호출은 **거부**되어 `NavigatorBusyException`이 발생합니다. UI 더블 클릭 가드용으로 `IsTransitioning`을 사전에 체크하세요.

### 같은 Main으로 `Transition<TCurrent>()` 호출

요청한 Main이 이미 현재 Main이면 no-op + 정보 로그입니다. 강제 unload+reload가 필요하면 `ReloadMain<T>()`를 사용하세요.

## 비동기 Hook

SceneNode 서브클래스에서 override하면 프레임워크가 await합니다.

```csharp
public sealed class MainGame : MainSceneNode
{
    protected override async UniTask OnNodeLoadedAsync(CancellationToken ct)
    {
        await PreloadAssetsAsync(ct);   // 스플래시 / 무거운 초기화
    }

    protected override async UniTask OnNodeUnloadingAsync(CancellationToken ct)
    {
        await SaveProgressAsync(ct);    // 깨끗한 정리
    }
}
```

## 라이프사이클 이벤트 (R3)

```
노드별 로드:  OnLoadedBegin → OnNodeLoadedAsync → OnLoadedEnd
노드별 언로드: OnUnloadBegin → OnNodeUnloadingAsync → OnUnloadEnd → 실제 unload
Main 전환:   OnLoading(prev, next, progress 0..1) — 단일 스트림
```

```csharp
SceneNavigator.Instance.OnLoadedEnd
    .Where(d => d.Kind == SceneNodeKind.Main)
    .Subscribe(d => Debug.Log($"Main loaded: {d.NodeType.Name}"));
```

R3가 설치되지 않은 환경에서는 같은 5개 멤버가 `event Action<...>`로 동작합니다.

## 전환 연출

```csharp
TransitionEffects.None                                     // 즉시 완료
TransitionEffects.Fade(0.3f)                               // 검정 페이드
TransitionEffects.Fade(0.3f, Color.white)                  // 색상 지정
TransitionEffects.FadeOut(0.2f)
TransitionEffects.FadeIn (0.5f)
TransitionEffects.Sequence(out, custom, in)
TransitionEffects.Parallel(a, b)
TransitionEffects.FromAction(playOut, playIn)              // 즉석 람다
```

커스텀 연출:

```csharp
public sealed class MyDissolve : ITransitionEffect
{
    public UniTask PlayOut(TransitionContext ctx) { /* 화면 어둡게 */ }
    public UniTask PlayIn (TransitionContext ctx) { /* 다시 보여주기 */ }
}
```

`TransitionContext`는 `Prev`, `Next`, `OverlayRoot`, `Cancellation`, `Progress` Observable을 제공합니다. UI는 `OverlayRoot`(Screen-Space-Overlay Canvas, 자동 생성 또는 `BaseSceneNode.overlayRoot`로 직접 지정) 아래에 띄우세요.

## Sub 재사용 정책

각 `SubSceneNode`는 `ReusePolicy`를 노출합니다.

- **Reuse** (기본): Main 전환 시, 다음 Main이 같은 Sub 타입을 사용하면 그대로 살려둠.
- **Recreate**: Main 전환마다 항상 unload+load.

## 조건부 컴파일

패키지는 `R3_SUPPORT`(설치 시 `com.cysharp.r3 >= 1.0.0` 자동 정의)와 `UNITASK_SUPPORT`(`com.cysharp.unitask >= 2.0.0`)로 가드됩니다.

## 샘플

`Window > Package Manager > Scene Navigator > Samples > Minimal`을 임포트하면 5개 스크립트 스켈레톤이 들어옵니다. 매칭되는 씬을 만들고 컴포넌트를 부착한 뒤 카탈로그를 재빌드하세요.

## 로드맵

- v0.2: 전체 PlayMode 테스트 fixture(전환 흐름, 비동기 hook, reload), 인스펙터 개선
- v0.3: 카테고리를 통합한 `Reload<T>()` 오버로드, 스크롤 가능한 SceneCatalog 인스펙터

## 라이센스

MIT — [LICENSE](LICENSE) 참조.
