# Launch Energy Regression Guards

Type: AFK

## Parent

`docs/prd/prd-run-body-natural-speed-ownership.md`

## What to build

Protect analog **Band** energy so small, medium, and maximum **Pull** strengths produce meaningfully different **Launch Impulse** results, and those differences are not flattened by steering behavior.

The test oracle for pure launch energy should be the **Slingshot Launch Applied** velocity change, not Rigidbody velocity after frames have advanced. Rigidbody velocity can already include contact, friction, steering, or landing-stabilizer effects.

This slice should close the regression path that made small and max pulls feel similar, without retuning launch values unless existing tests/config are out of sync with the current authored values.

## Acceptance criteria

- [ ] Minimum accepted **Pull** produces the configured minimum launch impulse.
- [ ] Maximum **Pull** produces the configured maximum launch impulse.
- [ ] Medium **Pull** produces monotonic launch impulse between minimum and maximum.
- [ ] Upward impulse, if scaled by pull strength, is covered by tests.
- [ ] Launch power upgrades, if present in current behavior, scale launch impulse without being flattened by steering.
- [ ] Editor-mouse slingshot smoke proves small and maximum pulls emit meaningfully different **Slingshot Launch Applied** velocity changes.
- [ ] Tests do not use post-frame Rigidbody velocity as the oracle for pure launch energy.

## Verification

- EditMode tests:
  - Launch impulse calculator covers min, mid, max, and upgrade-scaled launch energy.
  - Direction-only steering tests from the blocking slice prove steering does not flatten those velocities.
- PlayMode tests:
  - Focused slingshot input test compares **Slingshot Launch Applied** velocity changes for small and maximum pulls.
  - Scene composition assertions cover current launch config values if they are part of the protected contract.
- Static checks:
  - Unity connector compile before tests.
  - Rider reformat/problems for changed tests.
- Manual Unity smoke check:
  - Optional: small pull should visibly travel less than max pull in a controlled scene run.
- Package version/changelog:
  - Not required.

## Blocked by

- Direction-Only Run Steering.
