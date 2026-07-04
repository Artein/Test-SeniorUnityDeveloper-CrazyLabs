# Run Body Speed Sanity Guard

Type: AFK

## Parent

`docs/prd/prd-run-body-natural-speed-ownership.md`

## What to build

Add a **Run Body Speed Sanity Guard** that protects against non-finite or absurd **Run Body** velocities without becoming gameplay tuning.

The guard should sit far above normal **Launch**, upgrade, and authored **Run Surface** traversal speeds. Normal player-facing motion must pass unchanged. The guard exists only to contain catastrophic values such as NaN, infinity, or impossible finite spikes caused by a bug or corrupt state.

If this requires serialized tuning, the authoring language must make the purpose explicit: defensive impossible-velocity validation, not max movement speed, balance speed, or steering cap.

## Acceptance criteria

- [ ] Normal finite launch velocities pass unchanged.
- [ ] Expected maximum authored launch and upgrade velocities pass unchanged.
- [ ] Non-finite velocity is rejected, sanitized, or contained deterministically.
- [ ] Absurd finite velocity beyond the guard threshold is contained.
- [ ] The guard threshold is unreachable by normal **Launch**, upgrades, and authored **Run Surface** traversal.
- [ ] Guard behavior is independent of steering input and presentation mode.
- [ ] Any serialized field or tooltip uses defensive validation language, not player-facing speed-cap language.

## Verification

- EditMode tests:
  - Normal finite velocity passes unchanged.
  - Expected maximum launch velocity passes unchanged.
  - NaN/infinite velocity is contained.
  - Absurd finite velocity is contained.
  - Guard behavior does not depend on steering input.
- PlayMode tests:
  - If serialized in scene/config, scene composition asserts the authored threshold is above expected maximum launch and upgrade speeds.
- Static checks:
  - Unity connector compile before tests.
  - Rider reformat/problems for changed code and tests.
- Manual Unity smoke check:
  - Not required for the first slice.
- Package version/changelog:
  - Not required.

## Blocked by

- Direction-Only Run Steering.
