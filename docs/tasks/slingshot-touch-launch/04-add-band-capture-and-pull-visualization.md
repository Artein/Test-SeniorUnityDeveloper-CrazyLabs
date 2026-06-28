# Add Band Capture And Pull Visualization

> Superseded behavior note: this issue records the original first-slice visualization. The current Band Shape target is the follow-up natural taut
> Band Shape in `docs/tasks/slingshot-natural-band-shape/`, where rest, pull, and recoil shapes use fixed `BandWrapSampleCount + 4` point polylines
> from a silhouette/tangent solver rather than the three-point path below.

## Parent

[Slingshot Touch Launch PRD](../../prd/prd-slingshot-touch-launch.md)

## Type

AFK

## User stories covered

1, 2, 6, 10-16, 20-23, 25-29, 55-57

## What to build

Add the first Slingshot controller path from Pre-Launch input capture to visual Pull feedback. During Pre-Launch, the controller should enable Unity Input, allow only the first pointer to capture the Band Touch Target, project pointer movement to the Pull Plane, clamp forward/lateral/backward movement, and command the shallow view to update the Band Shape, Pull Hint, Touch Indicator, and idle state.

This slice should stop before raising a real launch request. It should make the interaction visibly pullable and test the touch-target/projection/clamping behavior in EditMode through fakes.

## Acceptance criteria

- [x] Slingshot has its own runtime assembly and EditMode test assembly.
- [x] Slingshot config covers touch target radius, Pull thresholds/limits, lateral limits, speed curve fields, and launch lift fields needed by later slices.
- [x] Slingshot view is a shallow MonoBehaviour adapter with explicit serialized references, `OnValidate`, geometry snapshot creation, view command methods, and selected-object gizmos.
- [x] Band Shape uses a three-point visual path from left anchor to pull/rest point to right anchor.
- [x] Pull Hint and Touch Indicator are controlled as scene-authored UI objects.
- [x] Controller subscribes to Unity Input once for lifetime and exposes capture enable/disable through `ISlingshotCapture`.
- [x] Gameplay Flow enables Slingshot capture on entering Pre-Launch; capture acquires an input enable handle before enabling capture visuals.
- [x] Gameplay Flow disables Slingshot capture on leaving Pre-Launch; capture cancels any Active Pull, returns visuals to idle, disables capture, then disposes the input handle.
- [x] A pointer press starts Active Pull only when capture is enabled and the press is within the generous screen-space Band Touch Target.
- [x] Only the first captured pointer controls the Active Pull; other pointers are ignored.
- [x] Pointer movement is projected to the Slingshot Pull Plane without physics raycasts.
- [x] Projection failure during Active Pull cancels the Pull and restores idle visuals.
- [x] Forward displacement clamps to zero; backward distance and lateral offset clamp independently.
- [x] Touch Indicator follows the projected clamped pull point, not raw finger position.

## Verification

- EditMode tests: direct capture enable/disable order; disable cancels Active Pull before handle disposal; disposal unsubscribes; touch target accepts/rejects by screen distance; first pointer capture; projection failure cancel; forward/lateral/backward clamp behavior; Touch Indicator projected position.
- PlayMode tests: none required for this slice.
- Static checks: Rider reformat/problems on changed files; Unity compile via Unity AI Agent Connector before tests.
- Manual Unity smoke check: use editor mouse simulation to confirm Band, Pull Hint, and Touch Indicator update in a temporary scene setup or Gameplay Scene if issue 08 wiring is pulled forward.
- Package version/changelog: no package/changelog change.

## Blocked by

- 02 - Add Asset-Backed Gameplay State
- 03 - Add UnityInput Pointer Stream
