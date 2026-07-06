## Parent

[Character Collider Split And Animated Contact Sensors PRD](../../prd/prd-character-collider-and-animated-contact-sensors.md)

## What to build

Run a human pickup-sensor reach and timing review after the scene smoke path is wired. Tune only sensor collider sizes, sensor placements, and explicit sensor-source entries unless review uncovers a real implementation defect.

This is a HITL slice because pickup reach and next-physics-step timing are player-feel questions. The goal is to confirm that animated hands, head, or other selected body parts collect pickups readably without destabilizing run movement, slingshot behavior, or run-ending contacts.

## Acceptance criteria

- [ ] Human reviewer confirms intended body-part sensors collect pickups at readable moments during slide and ordinary run animation.
- [ ] Human reviewer confirms pickup timing does not show an unacceptable one-physics-step delay for the first-pass pickup feature.
- [ ] Human reviewer confirms **Run Body Contact Collider** does not accidentally collect pickups.
- [ ] Human reviewer confirms run movement support remains stable while animated sensors move.
- [ ] Human reviewer confirms launch target and band contact behavior remain stable.
- [ ] Human reviewer confirms animated sensors do not self-trigger against **Run Body Contact Collider** or **Launch Target Collider Root**.
- [ ] Final accepted sensor set, approximate placement intent, and any tuning deltas are documented.
- [ ] Any real defect found during review is converted into an implementation fix or follow-up issue instead of being hidden as tuning.

## Verification

- EditMode tests:
  - Rerun only if code or validation changes during tuning.
- PlayMode tests:
  - Rerun scene smoke and sensor-trigger tests after any scene, prefab, collider, or layer tuning.
- Static checks:
  - `git diff --check`.
  - Unity compile through Unity AI Agent Connector if any code, scene, prefab, or ProjectSettings changes are made.
- Manual Unity smoke check:
  - Play the Gameplay scene and review pickup reach, slide reach, ordinary run reach, launch/band behavior, run support, finish, safety net, and default obstacle impact.
- Package version/changelog:
  - No package manifest or changelog update expected.

## Blocked by

- 09 - Wire Gameplay Scene Collider And Pickup Sensor Smoke Path
