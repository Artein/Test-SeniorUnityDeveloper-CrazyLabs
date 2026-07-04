# Test-SeniorUnityDeveloper-CrazyLabs

Unity mobile gameplay prototype built for the CrazyLabs Senior Unity Developer test assignment.

The brief asks for a Sled Surfers-style gameplay loop with Miraculous Ladybug-inspired visuals: slingshot launch, downhill forward movement, left/right steering, obstacles, collectibles, run end states, retry, persistent upgrades, and basic UI. This project implements that loop in a single playable Unity scene with a focus on maintainable gameplay architecture, explicit composition, and regression coverage.

## Quick Start

- Unity version: `6000.3.18f1`
- Main scene: `Assets/Scenes/GameplayScene.unity`
- Render pipeline: URP `17.3.0`
- Target context: mobile gameplay prototype, with iOS/Android project settings present

Open the project through Unity Hub with Unity `6000.3.18f1`, let Unity restore packages from `Packages/manifest.json`, then open `Assets/Scenes/GameplayScene.unity` and press Play.

Basic play flow:

1. Continue from Run Preparation into Pre-Launch.
2. Pull the launch target with touch or mouse input and release to launch.
3. Steer left/right during the downhill run.
4. Collect coins, avoid obstacles, and reach the run finish.
5. Use the Run Ended / Run Preparation UI to retry and buy persistent upgrades.

No generated playable build is committed in this repository. Build from Unity Build Profiles / Build Settings after selecting the desired mobile target.

## Assignment Coverage

| Brief requirement | Implementation locations |
| --- | --- |
| Slingshot launch | `Assets/Game/Gameplay/Slingshot`, `GameplaySlingshotLauncher`, `SlingshotLaunchImpulseCalculator` |
| Downhill movement and steering | `PlayerSteeringController`, `RunSurfaceSteeringFrameSource`, `RunSteeringModeSelector`, `RigidbodyPlayerSteeringTarget` |
| Obstacles and run-ending contacts | `RunContact`, `RunContactClassifier`, `RunEndFlow`, `LostMomentumDetector` |
| Collectibles | `Assets/Game/Gameplay/Pickups`, `PickupCollectionController`, `CoinPickupCurrencyGrantResolver` |
| Retry and progression | asset-backed gameplay states, `GameplayFlowController`, `RunPreparationPresenter`, `RunEndedPresenter` |
| Persistent upgrades | `Assets/Game/Gameplay/Upgrades`, `Assets/Game/Gameplay/Economy`, `UpgradePurchaseService`, `EconomySaveRepository` |
| Basic UI | `RunPreparationUIView`, `RunEndedUIView`, reward row and upgrade card views |
| Ladybug-themed course/visuals | `Assets/Plugins/Ladybug`, `Assets/Game/Level/RunCourses/LadybugRooftopHalfTube`, `GameplayScene.unity` |

The included assignment brief is stored at `docs/Unity Gameplay Test - V2.pdf`.

## Project Structure

```text
Assets/
  Scenes/                         Main playable scene.
  Settings/                       URP assets and scene volume settings.
  Game/
    Foundation/                   Shared Unity adapters and low-level services.
    Foundation/Input/             Centralized Unity Input System wrapper.
    Gameplay/                     Main gameplay loop, UI, camera, run flow, economy glue.
    Gameplay/GameplayState/       Asset-backed gameplay state model and transitions.
    Gameplay/Slingshot/           Pull capture, launch request, band visuals, launch target.
    Gameplay/Pickups/             Pickup authoring, state, physics integration, collection flow.
    Gameplay/Economy/             Currency definitions, save/load, reward accumulation.
    Gameplay/Upgrades/            Upgrade definitions, validation, preview, purchase flow.
    Level/RunCourses/             Ladybug Rooftop Half-Tube course authoring.
    Utils/                        Small reusable math, physics, and invocation helpers.
  Plugins/                        Provided/third-party art and presentation assets.
docs/
  adr/                            Architecture Decision Records.
  prd/                            Feature PRDs used to drive the implementation.
  tasks/                          Tracer-bullet implementation plans and QA notes.
Packages/
  manifest.json                   Unity package dependencies.
ProjectSettings/
  ProjectVersion.txt              Unity editor version.
```

The project currently has 28 assembly definitions, 327 C# source files, and separate EditMode/PlayMode test assemblies under the relevant feature folders.

## Architecture Notes

The gameplay code is intentionally split between Unity-facing adapters and testable plain C# logic:

- `GameplayLifetimeScope` is the scene composition root and uses VContainer to wire configs, views, controllers, services, and entry points.
- MonoBehaviours own Unity concerns: serialized references, scene views, Rigidbody/Collider/Camera bridges, and authoring validation.
- Gameplay decisions live in controllers/services such as `GameplayFlowController`, `RunEndFlow`, `PlayerSteeringController`, `PickupCollectionController`, and `UpgradePurchaseService`.
- Gameplay state identity is asset-backed rather than hardcoded strings or enum-only flow.
- Input is centralized behind `Game.Foundation.Input` so touch/mouse translation does not leak through gameplay systems.
- Slingshot band shape is deterministic and collider-aware instead of runtime rope physics.
- Cinemachine owns camera composition while project code owns run camera anchors and launch-gated activation.

See `docs/adr/index.md` for the decision log. The most relevant ADRs are:

- ADR-0002: keep gameplay logic in plain C# controllers.
- ADR-0005: use VContainer for dependency injection.
- ADR-0006: register views without injecting MonoBehaviours.
- ADR-0007: centralize Unity Input behind `UnityInput`.
- ADR-0008: use a deterministic taut band-shape solver instead of rope physics.
- ADR-0009: use Cinemachine for the run camera.

## Gameplay Loop

Runtime terminology used by the code:

- Run Preparation: upgrade/retry state before the next run.
- Pre-Launch: slingshot capture state.
- Launch: accepted pull release that applies the run impulse.
- Running: downhill movement, steering, pickups, camera follow, and run-end detection.
- Run Ended: immutable run result presentation, reward reveal, and acknowledgement before retry.

The current course is the Ladybug Rooftop Half-Tube: a downhill half-tube run course with soft containment, coin lines, ramps, obstacles, safety net, and an authoritative finish contact. Its acceptance profile targets a 420 meter course, a finish near 416 meters, and first completion after several upgraded attempts.

## Testing and Verification

The project uses Unity Test Framework with EditMode tests for deterministic gameplay logic and PlayMode tests for scene composition, physics integration, input, UI, camera, pickups, and slingshot behavior.

Current source scan:

- `654` `[Test]` attributes
- `68` `[UnityTest]` attributes
- `120` `*Tests.cs` files

Recommended verification:

```bash
.unity-ai-agent-connector/bin/uaiac compile
.unity-ai-agent-connector/bin/uaiac tests list --include selectors
.unity-ai-agent-connector/bin/uaiac tests run changed
```

If the local AI Agent Connector is not installed or not running, use Unity Test Runner instead:

- EditMode: run project EditMode tests first.
- PlayMode: run focused scene/pickup/slingshot tests when validating gameplay scene behavior.

## Package and Tooling Notes

Important packages from `Packages/manifest.json`:

- VContainer `1.18.0`
- Unity Input System `1.19.0`
- Cinemachine `3.1.7`
- URP `17.3.0`
- Unity Test Framework `1.6.0`
- SaintsField `5.21.0`
- Eflatun SceneReference `5.0.0`

The manifest also includes a project-local Unity AI Agent Connector package used for compile/test automation during development. It is tooling, not gameplay runtime logic.

## What I Would Improve With More Time

- Produce and attach a signed mobile build for direct evaluator playthrough.
- Add a short gameplay capture video alongside the README.
- Run a dedicated mobile device feel pass for touch steering, launch pull distance, UI scaling, and performance.
- Add final art/audio polish for obstacle feedback, reward reveal timing, and upgrade affordances.
- Add a small in-game diagnostics panel for build-time QA of state, speed, distance, and active upgrade modifiers.
