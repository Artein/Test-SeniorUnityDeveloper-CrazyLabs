# Add Launch Target And Slingshot Launcher

## Parent

[Slingshot Touch Launch PRD](../../prd/prd-slingshot-touch-launch.md)

## Type

AFK

## User stories covered

49-52, 55, 57

## What to build

Add the launch application boundary for Slingshot requests. A launcher should hold the Launch Target while the game is in Pre-Launch and apply final launch velocity when asked by Gameplay Flow. Rigidbody behavior should stay behind a shallow `ILaunchTarget` adapter so Slingshot intent does not depend on Rigidbody details.

This slice does not authorize gameplay flow transitions. It only provides deterministic hold and launch mechanics plus defensive validation at the launch boundary.

## Acceptance criteria

- [x] Slingshot exposes an `ISlingshotLauncher` with a void launch operation.
- [x] `ILaunchTarget` exposes hold and launch operations.
- [x] Rigidbody Launch Target is a shallow MonoBehaviour adapter over an explicitly assigned Rigidbody.
- [x] Adapter uses `OnValidate` for missing reference diagnostics and production assertions before dereferencing serialized references.
- [x] Hold preserves previous Rigidbody kinematic state and constraints, sets kinematic true, and clears linear/angular velocity.
- [x] Hold is idempotent and runs on initial Pre-Launch plus every Pre-Launch re-entry.
- [x] Launch restores saved state if held, clears stale linear/angular velocity, and applies final velocity using mass-agnostic velocity change.
- [x] Launch still behaves deterministically if hold was never called.
- [x] Launcher computes final velocity from horizontal direction/speed plus up direction/up speed.
- [x] Invalid request data or invalid final velocity warns and skips target launch.
- [x] Launcher does not duplicate Gameplay Flow authorization or transition logic.

## Verification

- EditMode tests: launcher holds target on initial Pre-Launch and re-entry; valid request calls target with expected final velocity; invalid requests warn and skip; no transition authorization logic.
- PlayMode tests: Rigidbody adapter hold/launch behavior if Unity Rigidbody behavior cannot be reliably verified in EditMode.
- Static checks: Rider reformat/problems on changed files; Unity compile via Unity AI Agent Connector before tests.
- Manual Unity smoke check: inspect Rigidbody adapter assignment on Player and verify no missing reference warnings.
- Package version/changelog: no package/changelog change.

## Blocked by

- 02 - Add Asset-Backed Gameplay State
