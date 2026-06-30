## Parent

[Ladybug Character Presentation PRD](../../prd/prd-ladybug-character-presentation.md)

## What to build

Expose accepted Run Result notifications from the existing run-end flow through a direct C# event so Character presentation can map completed runs to terminal presentation modes. The presentation layer should learn that a result was accepted and whether it was success or failure, without requiring the view to understand Run End Reason or run-ending internals.

This slice should not add Animator, Ladybug assets, Character presenter wiring, or visual behavior yet.

## Acceptance criteria

- [ ] A run-result notifier contract exposes an accepted Run Result event through direct C# event semantics.
- [ ] The existing run-end flow publishes exactly the accepted Run Result that it latches/logs/transitions from.
- [ ] Success and failure information is preserved for presentation consumers.
- [ ] Run Result and Run End Reason do not become dependencies of any Unity view in this slice.
- [ ] Existing run-end behavior, result latching, candidate priority, and state transitions remain unchanged except for the new notification.
- [ ] Subscribers can safely subscribe and unsubscribe without leaking callbacks across scene/lifetime teardown.
- [ ] No event bus, analytics backend, or structured logging backend is introduced.

## Verification

- EditMode tests:
  - Accepted success result emits one notification with success preserved.
  - Accepted failure result emits one notification with failure preserved.
  - Non-accepted or superseded candidates do not emit extra notifications.
  - Existing result latching and transition tests remain green.
  - Subscriber removal prevents later callback invocation.
- PlayMode tests:
  - None expected unless existing run-end PlayMode smoke tests already cover the flow.
- Static checks:
  - `git diff --check`.
  - Unity compile through the connector before tests.
- Manual Unity smoke check:
  - Trigger a finish/failure path in Gameplay Scene if already easy to do and confirm existing run-end logs still read correctly.
- Package version/changelog:
  - No package manifest or changelog update expected.

## Blocked by

None - can start immediately
