## Parent

[Pickup Resource Collection PRD](../../prd/prd-pickup-resource-collection.md)

## What to build

Wire a gameplay-scene smoke path for the completed pickup/resource feature. The scene should contain placed regular and big coin pickups, explicit
pickup references in `GameplayLifetimeScope`, explicit player pickup-contact collider references, and correct player collider tag/layer authoring.
During live gameplay, collecting a pickup should increase total resources, increase the current-run accumulator, disappear for later runs in the same
**Level Session**, and appear in the just-ended **Run Result** snapshot.

This slice is HITL because exact level placement and play-feel review are design-facing, even though the wiring should stay deterministic.

## Acceptance criteria

- [ ] Gameplay scene contains at least one regular coin pickup and one big coin pickup placed for smoke testing.
- [ ] All scene pickups are serialized into `GameplayLifetimeScope`; no runtime scene discovery is required.
- [ ] Player pickup-contact colliders are serialized into `GameplayLifetimeScope` for validation.
- [ ] Every collecting player collider GameObject has **Player Layer** and the configured **Player Tag**.
- [ ] Pickup collection occurs only while gameplay state is `Running`.
- [ ] Collected pickup roots disable immediately and remain unavailable across later runs in the same **Level Session**.
- [ ] Remaining pickups can still be collected in later runs of the same **Level Session**.
- [ ] `ResourceStorage` total continues accumulating across runs in the current app session.
- [ ] **Run Result** contains only the resources collected during the just-ended **Run**.
- [ ] Manual smoke does not require economy, persistence, HUD, VFX/SFX, floating counters, or final result-screen UI.

## Verification

- EditMode tests:
  - Lifetime-scope validation accepts the smoke-scene pickup/player references.
- PlayMode tests:
  - Gameplay scene composition resolves pickup/resource dependencies.
  - Collecting a regular pickup and big pickup grants their configured amounts.
  - Restarting a run does not restore already-consumed pickups.
  - Ended-run result snapshot contains current-run resources only.
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
