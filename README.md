# SceneNavigatorForUnity

This repository is the development workspace for the **Scene Navigator for Unity** UPM package.

The library itself lives at:

> [`Packages/com.jinhosung96.scene-navigator-for-unity/`](Packages/com.jinhosung96.scene-navigator-for-unity/)

→ See the package [README (English)](Packages/com.jinhosung96.scene-navigator-for-unity/README.md) / [README (한국어)](Packages/com.jinhosung96.scene-navigator-for-unity/README.ko.md) for installation, API reference, and quick start.

## Repository layout

```
Packages/com.jinhosung96.scene-navigator-for-unity/   ← the library (publishable)
Assets/                                             ← Unity dev sandbox (not part of the package)
ProjectSettings/                                    ← Unity project settings
```

`Assets/` exists so this folder can be opened as a Unity project for testing the package end-to-end. Library users install via UPM and never see anything under `Assets/`.

## Installing the package in another project

Add to that project's `Packages/manifest.json`:

```json
{
  "dependencies": {
    "com.jinhosung96.scene-navigator-for-unity": "https://github.com/jinhosung96/SceneNavigatorForUnity.git?path=Packages/com.jinhosung96.scene-navigator-for-unity",
    "com.cysharp.r3":      "1.0.0",
    "com.cysharp.unitask": "2.5.0"
  }
}
```

## License

[MIT](LICENSE)
