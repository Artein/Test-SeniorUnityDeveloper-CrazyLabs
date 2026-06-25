# Add Band Capture And Pull Visualization

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

- [ ] Slingshot has its own runtime assembly and EditMode test assembly.
- [ ] Slingshot config covers touch target radius, Pull thresholds/limits, lateral limits, speed curve fields, and launch lift fields needed by later slices.
- [ ] Slingshot view is a shallow MonoBehaviour adapter with explicit serialized references, `OnValidate`, geometry snapshot creation, view command methods, and selected-object gizmos.
- [ ] Band Shape uses a three-point visual path from left anchor to pull/rest point to right anchor.
- [ ] Pull Hint and Touch Indicator are controlled as scene-authored UI objects.
- [ ] Controller subscribes to Unity Input and Gameplay State once for lifetime and enables capture only while the current state is Pre-Launch.
- [ ] Entering Pre-Launch acquires an input enable handle before enabling capture visuals.
- [ ] Leaving Pre-Launch cancels any Active Pull, returns visuals to idle, disables capture, then disposes the input handle.
- [ ] A pointer press starts Active Pull only when capture is enabled and the press is within the generous screen-space Band Touch Target.
- [ ] Only the first captured pointer controls the Active Pull; other pointers are ignored.
- [ ] Pointer movement is projected to the Slingshot Pull Plane without physics raycasts.
- [ ] Projection failure during Active Pull cancels the Pull and restores idle visuals.
- [ ] Forward displacement clamps to zero; backward distance and lateral offset clamp independently.
- [ ] Touch Indicator follows the projected clamped pull point, not raw finger position.

## Verification

- EditMode tests: initial Pre-Launch enables capture/input; enter/leave order; leave cancels Active Pull before handle disposal; disposal unsubscribes; touch target accepts/rejects by screen distance; first pointer capture; projection failure cancel; forward/lateral/backward clamp behavior; Touch Indicator projected position.
- PlayMode tests: none required for this slice.
- Static checks: Rider reformat/problems on changed files; Unity compile via Unity AI Agent Connector before tests.
- Manual Unity smoke check: use editor mouse simulation to confirm Band, Pull Hint, and Touch Indicator update in a temporary scene setup or Gameplay Scene if issue 08 wiring is pulled forward.
- Package version/changelog: no package/changelog change.

## Blocked by

- 02 - Add Asset-Backed Gameplay State
- 03 - Add UnityInput Pointer Stream
