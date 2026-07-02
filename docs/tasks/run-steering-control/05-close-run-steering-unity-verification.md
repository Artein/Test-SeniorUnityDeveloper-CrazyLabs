## Parent

[Run Steering Control PRD](../../prd/prd-run-steering-control.md)

## What to build

Close the implementation with an end-to-end Unity verification slice. This should prove the accepted Run Steering Control behavior across compile, targeted tests, scene composition, editor input, asset values, and documentation terminology.

This slice should not add new gameplay scope. It should harden the completed path, remove stale references, and leave a clear verification record for the feature.

## Acceptance criteria

- [ ] Unity script compilation is clean.
- [ ] All targeted EditMode tests added or changed for Run Steering Control pass.
- [ ] Any required PlayMode tests pass.
- [ ] The active steering configuration asset uses the agreed baseline values.
- [ ] Existing Pre-Launch Pull behavior is still covered by tests or a smoke path.
- [ ] Editor mouse input still works through generic pointer input for Running steering.
- [ ] Mobile touch input remains routed through the existing Foundation input backend.
- [ ] Gameplay context documentation uses Run Steering Control terminology consistently.
- [ ] No visible joystick UI, extra prefab, canvas overlay, or unrelated scene change is present.
- [ ] No package manifest, Addressables schema, save format, or new ADR change is introduced.
- [ ] No outdated steering sensitivity or response-rate active API remains.

## Verification

- EditMode tests:
  - Run all Run Steering Gesture tests.
  - Run all changed player steering/controller tests.
  - Run any directly affected input or screen abstraction tests.
- PlayMode tests:
  - Run any changed gameplay scene, input smoke, or composition tests.
  - Add a PlayMode smoke only if EditMode coverage cannot verify the required runtime path.
- Static checks:
  - `git diff --check`.
  - Unity compile through Unity AI Agent Connector.
  - Source search for stale `SteeringSensitivity` and `SteeringResponseRate` active API usage.
  - Source search for accidental direct mobile touch dependency in gameplay steering.
  - Source search for unexpected visible joystick UI additions.
- Manual Unity smoke check:
  - In Editor, enter a playable run, start steering away from screen center, move horizontally, release, and confirm requested steering clears.
  - Confirm Pre-Launch Pull still captures, drags, and releases as before.
- Package version/changelog:
  - No package manifest or changelog update expected.

## Blocked by

- 04 - Prove Responsiveness And Movement Integration
