# Add UnityInput Pointer Stream

## Parent

[Slingshot Touch Launch PRD](../../prd/prd-slingshot-touch-launch.md)

## Type

AFK

## User stories covered

30-37, 55-57

## What to build

Add the shared Unity Input feature that centralizes Enhanced Touch support and translates input into source-agnostic Pointer Input events. Consumers should acquire input by calling a reference-counted enable method and disposing the returned handle.

This slice should include a production backend over Unity Input System Enhanced Touch, editor mouse simulation for development, and a fakeable backend for deterministic EditMode tests. The Slingshot feature should later depend only on the public input interfaces, not Unity global input APIs.

## Acceptance criteria

- [ ] Unity Input has its own runtime assembly and EditMode test assembly.
- [ ] `IUnityInput` composes the Enhanced Touch support API and Pointer Input event API.
- [ ] `Enable()` returns an idempotent disposable handle; first live handle enables/subscribes and last disposed handle unsubscribes/disables.
- [ ] Calling `Enable()` after service disposal throws.
- [ ] Disposing outstanding handles after service disposal is a no-op.
- [ ] Pointer payload is immutable and contains pointer id plus raw screen position.
- [ ] Pointer events cover pressed, moved, released, and canceled phases.
- [ ] Enhanced Touch finger index maps to pointer id; editor mouse uses the reserved pointer id.
- [ ] No events are forwarded after the last enable handle is disposed.
- [ ] Subscriber exceptions are isolated per subscriber, logged with existing Unity logging, and do not prevent remaining subscribers from receiving the event.
- [ ] Player builds do not include runtime mouse gameplay input for this mechanic.

## Verification

- EditMode tests: reference-counted enable/disable ordering; idempotent handle disposal; enable after dispose throws; outstanding handle disposal after service disposal; phase mapping; no forwarding while disabled; subscriber exception isolation where practical.
- PlayMode tests: none required for this slice.
- Static checks: Rider reformat/problems on changed files; Unity compile via Unity AI Agent Connector before tests.
- Manual Unity smoke check: in the Editor, confirm mouse simulation can be observed through a temporary/local subscriber or test harness without touching Slingshot code.
- Package version/changelog: no package/changelog change.

## Blocked by

- 01 - Add Gameplay Composition Spine
