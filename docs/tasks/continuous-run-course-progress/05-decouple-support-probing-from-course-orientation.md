# 05 — Decouple physics support probing from Run Course orientation

**Type:** AFK
**User stories covered:** 13, 22, 52–53, 73, 75, 77–81

## Parent

[Continuous Run Course Progress PRD](../../prd/prd-continuous-run-course-progress.md)

## What to build

Introduce an explicit support/gravity reference contract for grounding and collision probing while retaining course tangent as the longitudinal direction. Course-authored up or banking may inform presentation, but it must not implicitly become the physics cast direction, gravity direction, or contact-response frame.

Deliver the change through the existing support-context service and scene composition so current Ladybug physics remains unchanged and later curved/vertical courses can combine local course slope with independently valid support data.

## Acceptance criteria

- [ ] Support probing receives an explicit support/gravity frame independent from course orientation.
- [ ] Changing authored course up or banking cannot redirect support casts, grounding decisions, gravity ownership, contacts, or collision response.
- [ ] Longitudinal slope uses the course tangent together with the support/gravity frame where required.
- [ ] Existing support-normal stabilization and steering-frame behavior remain intact.
- [ ] Rigidbody contact physics remains the owner of gravity, contacts, and collision response.
- [ ] VContainer composition makes both course and support dependencies explicit without global service access or static mutable state.
- [ ] The current Gameplay Scene preserves grounding and traversal behavior.

## Verification

- EditMode tests: Course orientation changes do not change support/gravity direction; slope calculation uses tangent plus support frame; invalid frames fail explicitly.
- PlayMode tests: Support casts remain stable while authored course orientation varies; current Ladybug grounding, seams, banks, and ramps remain supported.
- Static checks: Rider problems clean; Unity compile gate clean before tests; dependency direction respects plain-controller, composition, and contact-physics ADRs.
- Manual Unity smoke check: Traverse flat, banked, and ramp sections while observing grounding and support diagnostics.
- Package version/changelog: No package change; document the clarified frame semantics if public or serialized contracts change.

## Blocked by

- 02 — Ship the straight Run Course sample with exact Ladybug parity
