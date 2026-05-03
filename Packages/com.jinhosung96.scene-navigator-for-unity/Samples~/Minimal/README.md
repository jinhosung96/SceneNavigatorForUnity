# Minimal Sample

This sample contains 5 scripts (`MyBase`, `MainAScene`, `MainBScene`, `SubHUDScene`, `PauseScene`).

To turn it into a runnable demo:

1. Create 5 scenes inside this folder: `Base.unity`, `MainA.unity`, `MainB.unity`, `SubHUD.unity`, `PauseInstance.unity`.
2. In each scene, create an empty GameObject and attach the corresponding `*Scene` component (saved name will be auto-corrected on save).
3. On `MyBase`'s Inspector, pick `MainAScene` as the **Startup Main**.
4. On `MainAScene` and `MainBScene`'s Inspector, register `SubHUDScene` under **Sub Scene Nodes**.
5. Run `Tools > Scene Navigator > Rebuild Catalog`.
6. From any of these scenes, press Play. The Base scene will be loaded automatically along with the Startup Main.
7. From a script, switch with: `await SceneNavigator.Instance.Transition<MainBScene>();`
