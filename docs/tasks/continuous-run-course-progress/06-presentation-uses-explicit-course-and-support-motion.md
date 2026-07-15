# 06 — Drive character presentation from explicit course and support motion

**Type:** AFK
**User stories covered:** 12, 51, 53, 72, 75, 78–81

## Parent

[Continuous Run Course Progress PRD](../../prd/prd-continuous-run-course-progress.md)

## What to build

Migrate character presentation and run diagnostics from the ambiguous legacy progress frame to explicit motion concepts: signed longitudinal course speed, course-relative slope where needed, and support-relative vertical behavior. Keep animation state ownership in the presentation layer and retain current Ladybug visuals and tuning.

## Acceptance criteria

- [ ] Character presentation consumes explicit course-motion and support-motion values rather than reconstructing them from one shared frame.
- [ ] Bends and elevation changes can drive local longitudinal animation without redefining support-relative rising or falling.
- [ ] Airborne, slide, and idle classification retains its existing support and fall-intent behavior.
- [ ] Diagnostics label and display the distinct longitudinal, lateral, support-relative vertical, and validity signals without duplicating course projection.
- [ ] Current Ladybug presentation output remains equivalent for the straight route.
- [ ] Presentation remains independent from Rigidbody contact-physics ownership and from optional spline APIs.

## Verification

- EditMode tests: Presentation classification for longitudinal, lateral, rising, falling, and invalid samples; straight-route equivalence.
- PlayMode tests: Animation reacts correctly on straight, sloped, and unsupported movement; current Gameplay Scene visual smoke remains stable.
- Static checks: Rider problems clean; Unity compile gate clean before tests; presentation has no spline-vendor or geometry-projection dependency.
- Manual Unity smoke check: Record representative traversal and inspect animation plus diagnostics through ramps, banks, airborne motion, and rollback.
- Package version/changelog: No package change; add release notes only for intentionally changed presentation behavior.

## Blocked by

- 02 — Ship the straight Run Course sample with exact Ladybug parity
- 05 — Decouple physics support probing from Run Course orientation
