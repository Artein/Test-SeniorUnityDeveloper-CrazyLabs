## Parent

[Pickup Resource Collection PRD](../../prd/prd-pickup-resource-collection.md)

## What to build

Wire a gameplay-scene smoke path for the completed pickup/resource feature. The scene should contain placed regular and big coin pickups, explicit
pickup references in `GameplayLifetimeScope`, explicit player pickup-contact collider references, and correct player collider tag/layer authoring.
During live gameplay, collecting a pickup should increase total resources, increase the current-run accumulator, disappear for later runs in the same
**Level Session**, and appear in the just-ended **Run Result** snapshot.

This slice is HITL because exact level placement and play-feel review are design-facing, even though the wiring should stay deterministic.

## Acceptance criteria

- [x] Gameplay scene contains at least one regular coin pickup and one big coin pickup placed for smoke testing.
- [x] All scene pickups are serialized into `GameplayLifetimeScope`; no runtime scene discovery is required.
- [x] Player pickup-contact colliders are serialized into `GameplayLifetimeScope` for validation.
- [x] Every collecting player collider GameObject has **Player Layer** and the configured **Player Tag**.
- [x] Pickup collection occurs only while gameplay state is `Running`.
- [x] Collected pickup roots disable immediately and remain unavailable across later runs in the same **Level Session**.
- [x] Remaining pickups can still be collected in later runs of the same **Level Session**.
- [x] `ResourceStorage` total continues accumulating across runs in the current app session.
- [x] **Run Result** contains only the resources collected during the just-ended **Run**.
- [x] Manual smoke does not require economy, persistence, HUD, VFX/SFX, floating counters, or final result-screen UI.

## Verification

- EditMode tests:
  - `RunEndFlowTests` covers current-run resource snapshot capture and reset on Pre-Launch.
  - `GameplayLifetimeScopeTests` covers pickup/resource service registration and validation seams.
- PlayMode tests:
  - `PickupSceneCompositionTests.given_GameplayScene_when_Loaded_then_PickupResourceCompositionIsReady` validates scene pickup/player references, layers, configured amounts, shared coin resource, and VContainer services.
  - `PickupPhysicsIntegrationTests` covers live trigger collection, Pre-Launch ignored contacts, root-only tag rejection, and consumed-root no-second-grant behavior.
- Static checks:
  - `git diff --check`.
  - Unity compile through Unity AI Agent Connector.
  - Targeted EditMode and PlayMode tests for pickup/resource gameplay.
- Manual Unity smoke check:
  - Launch and collect a regular coin.
  - End the run and confirm current-run result data.
  - Start another run and confirm the collected coin remains gone.
  - Collect a remaining big coin and confirm app-session total plus run snapshot behavior.
- Package version/changelog:
  - No package manifest or changelog update expected.

## Blocked by

- 06 - Wire GameplayLifetimeScope Composition And Validation
- 08 - Author Regular And Big Coin Pickup Prefabs
