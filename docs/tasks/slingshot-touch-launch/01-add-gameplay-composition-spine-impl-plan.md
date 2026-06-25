# Implement Issue 01: Gameplay Composition Spine

## Summary

- Add VContainer as the project DI dependency, pinned to `v1.18.0` through the direct UPM Git URL:
  `https://github.com/hadashiA/VContainer.git?path=VContainer/Assets/VContainer#1.18.0`.
- Add the root gameplay assembly and EditMode test assembly under `Assets/Game/Gameplay`.
- Add a minimal `GameplayLifetimeScope` scaffold that is composition-only, validates serialized references, and provides the root place for later gameplay registrations.
- Do not add gameplay rules, Slingshot services, Gameplay State services, or scene wiring beyond the reusable composition spine.

## Key Changes

- Update `Packages/manifest.json`; let Unity regenerate/update `Packages/packages-lock.json`.
- Create `Game.Gameplay.asmdef` with references to Unity assemblies as needed and VContainer.
- Create `Game.Gameplay.Tests.EditMode.asmdef` referencing `Game.Gameplay`, NUnit/Test Framework, and VContainer if required.
- Add `GameplayLifetimeScope : VContainer.Unity.LifetimeScope`.
  - Namespace: `Game.Gameplay`.
  - `protected override void Configure(IContainerBuilder builder)`.
  - Calls a private validation method before registration.
  - Registration body is intentionally empty for this issue.
  - `OnValidate()` performs the same current no-op validation path so future serialized refs have a clear hook.
- Do not use dynamic scene searches.
- Do not inject MonoBehaviours.
- Do not use static helpers/classes.

## Test Plan

- Add one EditMode test proving the gameplay test assembly compiles and the composition scaffold type is available.
- Prefer a simple test such as constructing a temporary `GameObject`, adding `GameplayLifetimeScope`, and asserting the component exists; do not rely on runtime callbacks in EditMode.
- Run implementation workflow:
  - Rider create/reformat/problem checks for new C# and asmdef files.
  - Unity compile via Unity AI Agent Connector.
  - Run targeted EditMode tests only after compile is clean.
- Manual smoke after implementation: open `Assets/Scenes/GameplayScene.unity`, confirm `GameplayLifetimeScope` can be assigned to a GameObject without missing-script errors.

## Assumptions

- VContainer `v1.18.0` is the selected pin because official VContainer docs show that Git URL/tag and GitHub marks `v1.18.0` latest.
- Issue 08 owns actual Gameplay Scene wiring; issue 01 only creates the class scaffold and package/assembly baseline.
- No changelog entry is required because this is project gameplay infrastructure, not a package release.

## References

- [Issue 01: Add Gameplay Composition Spine](01-add-gameplay-composition-spine.md)
- [VContainer installation](https://vcontainer.hadashikick.jp/getting-started/installation)
- [VContainer Hello World / LifetimeScope](https://vcontainer.hadashikick.jp/getting-started/hello-world)
- [VContainer releases](https://github.com/hadashiA/VContainer/releases)
