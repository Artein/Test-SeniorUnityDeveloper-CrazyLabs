## Parent

[Pickup Resource Collection PRD](../../prd/prd-pickup-resource-collection.md)

## What to build

Add the shallow Unity **Pickup** adapter and the plain C# **Level Pickup State** that owns one-shot availability for explicit scene pickups. A
pickup should forward trigger-enter contacts while enabled and apply availability by enabling/disabling the whole pickup scene root, but it should not
decide collectability, mutate resources, check gameplay state, or own consumed state.

This slice should make pickup availability deterministic across repeated runs in one **Level Session**, without adding collection transaction logic yet.

## Acceptance criteria

- [x] **Pickup** exposes its assigned **Pickup Definition** and fails loud when required authored data is missing.
- [x] **Pickup** forwards `OnTriggerEnter` contacts while enabled, including the raw contacted `Collider`.
- [x] **Pickup** does not grant resources, check gameplay state, compare tags, store consumed state, or update score/UI.
- [x] `SetAvailable(bool)` enables/disables the whole pickup scene root GameObject so visuals and trigger participation stop together.
- [x] `LevelPickupState` is initialized from explicit pickup references.
- [x] `LevelPickupState.TryConsume` succeeds only once per pickup until a **Level Session** reset.
- [x] `LevelPickupState` rejects null and duplicate pickup references deterministically.

## Verification

- EditMode tests:
  - `LevelPickupState` marks a pickup consumed once.
  - `LevelPickupState.TryConsume` rejects the same pickup after the first success.
  - `LevelPickupState` reset restores pickup availability for a new **Level Session**.
  - `LevelPickupState` rejects duplicate explicit pickup references.
  - **Pickup** validation rejects missing **Pickup Definition**.
- PlayMode tests:
  - **Pickup** forwards one trigger-enter contact when enabled.
  - Consumed pickup root deactivation disables the full pickup object.
- Static checks:
  - `git diff --check`.
  - Unity compile through Unity AI Agent Connector.
- Manual Unity smoke check:
  - Toggle a sample pickup available/unavailable and confirm the whole pickup instance follows that state.
- Package version/changelog:
  - No package manifest or changelog update expected.

## Blocked by

- 01 - Add Generic Resource Grant Data Path
