# Add Pull Release To LaunchRequested

## Parent

[Slingshot Touch Launch PRD](../../prd/prd-slingshot-touch-launch.md)

## Type

AFK

## User stories covered

3-9, 15, 47, 48, 59

## What to build

Complete Slingshot Pull interpretation by converting a valid Pull Release into a `LaunchRequested` event. The request should be pure data: normalized power, Pull distance, signed Pull Offset, launch direction, launch speed, up direction, and launch up speed.

Weak, forward-only, canceled, or invalidly projected Pulls should return the view to idle without notifying launch consumers. Lateral Pull Offset should rotate direction without changing power, while backward Pull distance and the speed curve determine launch speed.

## Acceptance criteria

- [x] Slingshot exposes an `ISlingshotLaunchNotifier` interface with the required `LaunchRequested` event.
- [x] The event uses a typed launch-request handler delegate and a pure launch request payload.
- [x] Valid Pull Release raises exactly one launch request for the captured pointer.
- [x] Weak Pulls below the configured minimum do not raise a request.
- [x] Forward-only Pulls do not raise a request.
- [x] Canceled Pulls and projection-failed Pulls do not raise a request.
- [x] Normalized power comes from effective backward Pull distance clamped between min and max Pull distance.
- [x] Launch speed maps normalized power through the configured curve, then into the configured min/max speed range.
- [x] Pull Offset is signed lateral world displacement and rotates Launch Frame forward by the configured max launch angle.
- [x] Launch up speed and up direction are included without being coupled to forward speed.
- [x] The Band Shape and Touch Indicator return to idle after release regardless of whether a launch request was raised.

## Verification

- EditMode tests: valid release payload; weak release no event; forward-only no event; canceled no event; normalized power and speed curve mapping; Pull Offset steering sign and clamp; launch lift fields; visuals reset after release.
- PlayMode tests: none required for this slice.
- Static checks: Rider reformat/problems on changed files; Unity compile via Unity AI Agent Connector before tests.
- Manual Unity smoke check: with a local subscriber, pull/release in Editor and confirm request values are plausible in logs or debugger.
- Package version/changelog: no package/changelog change.

## Blocked by

- 04 - Add Band Capture And Pull Visualization
