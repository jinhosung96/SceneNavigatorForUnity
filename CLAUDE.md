# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

- **Purpose:** Multi-Scene-based scene management framework — consumed as a reusable library by other Unity projects.
- **Top priority — reusability & extensibility:** Every API / abstraction / dependency decision is judged against these two criteria first. Prefer pluggable seams (interfaces, extension points, configurable defaults) over hard-coded behavior; do not leak project-specific assumptions into the framework surface.
- **Editor:** Unity **2021.3.38f1 LTS** (`ProjectSettings/ProjectVersion.txt`). Must match exactly.
- **Type:** 2D project (`com.unity.feature.2d` installed).
- **Open project:** `"<UnityPath>" -projectPath .`
- **Run tests headless:** `"<UnityPath>" -batchmode -nographics -projectPath . -runTests -testPlatform editmode -testResults results.xml -logFile -` (swap `editmode` ↔ `playmode`; add `-testFilter "Namespace.Class.Method"` for one test).
- **Don't touch:** `Library/`, `Logs/`, `Temp/`, `UserSettings/` (Editor-regenerated). Never delete or rename `.meta` files — Unity tracks asset GUIDs through them.
- **Third-party packages (UPM Git):** R3 (`com.cysharp.r3`), UniTask (`com.cysharp.unitask`), NuGetForUnity (`com.github-glitchenzo.nugetforunity`), Unity MCP (`com.coplaydev.unity-mcp`).
- **Conditional compilation (REQUIRED):** Gate every R3 usage with `#if R3_SUPPORT`, UniTask with `#if UNITASK_SUPPORT`. Wrap both `using` directives and code blocks; code must still compile when any define is absent. Define them in `Project Settings > Player > Scripting Define Symbols` or per-asmdef `versionDefines`.
- **Coding style — prefer expressive paradigms:** Lean on LINQ and R3 (R3 inside `#if R3_SUPPORT`); freely combine OOP, reactive, and functional styles, picking whichever expresses intent most clearly per case.
