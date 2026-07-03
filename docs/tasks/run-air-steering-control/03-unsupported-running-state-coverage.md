# Unsupported Running State Coverage

Type: AFK

## Parent

`docs/prd/prd-run-air-steering-control.md`

## What to build

Broaden the implemented **Run Air Steering Control** behavior so it is not accidentally launch-only. It must apply during any unsupported **Running** motion: fired-by-slingshot motion, later bumps that temporarily lose valid **Run Surface** support, and ordinary falling while gameplay is still **Running**.

The feature must stop at the correct gameplay boundaries. **RunEnded**, accepted victory/defeat, **RunPreparation**, and **PreLaunch** must not keep applying air steering or leak steering-mode state into the next run.

This slice is primarily about integration coverage through gameplay state changes and support transitions. It should prove the behavior remains selected by physical support and motion facts, not by **Launch Flight**, **Airborne**, or other presentation modes.

## Acceptance criteria

- [ ] Unsupported motion immediately after launch can use **Run Air Steering Control**.
- [ ] Unsupported motion from a later course bump can use **Run Air Steering Control** while **Running**.
- [ ] Unsupported falling before **RunEnded** can use **Run Air Steering Control** while **Running**.
- [ ] Air steering is not selected from **Launch Flight**, **Airborne**, or any other **Character Presentation Mode**.
- [ ] Accepted run result stops air steering.
- [ ] **RunEnded** stops air steering.
- [ ] **RunPreparation** and **PreLaunch** clear any steering-mode state that could affect the next run.
- [ ] A second run after win/fail starts with clean steering state.
- [ ] Weak grounded launches with valid support and no positive lift use grounded steering, not air steering.
- [ ] Stale grounded samples with positive lift use air steering, not grounded steering.

## Verification

- EditMode tests:
  - Launch unsupported sequence selects air and allows air steering with active gesture.
  - Later bump unsupported sequence selects air and allows air steering with active gesture.
  - Later fall while still **Running** selects air and allows air steering with active gesture.
  - Accepted result, **RunEnded**, **RunPreparation**, and **PreLaunch** stop or clear air steering.
  - New run after terminal state does not inherit stale air-steering state.
  - Presentation mode facts are absent from selector/controller decisions.
- PlayMode tests:
  - Not required unless state reset can only be observed through scene composition.
- Static checks:
  - Unity connector compile before tests in implementation.
  - Rider reformat/problems for changed code and tests in implementation.
- Manual Unity smoke check:
  - Launch, steer in the air, land, then retry after win/fail and confirm steering state feels clean.
- Package version/changelog:
  - Not required.

## Blocked by

- Direction-Only Run Air Steering
