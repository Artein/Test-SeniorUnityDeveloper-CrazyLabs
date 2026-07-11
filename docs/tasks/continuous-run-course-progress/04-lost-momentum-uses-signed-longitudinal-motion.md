# 04 — Detect Lost Momentum from signed longitudinal course motion

**Type:** AFK
**User stories covered:** 3–5, 15, 26–27, 39–42, 64–65, 70, 75, 77–78

## Parent

[Continuous Run Course Progress PRD](../../prd/prd-continuous-run-course-progress.md)

## What to build

Migrate Lost Momentum to consume the shared Run Progress Sample. Its motion signal must be signed longitudinal speed along the local course tangent, and its advancement signal must remain maximum-progress gain over the configured observation period. Preserve the established launch grace, duration, threshold values, and finish precedence while making any reverse-recovery exception explicit.

## Acceptance criteria

- [ ] Forward, lateral, stopped, and reverse motion are distinguished by signed longitudinal speed from the shared sample.
- [ ] Lost Momentum requires both insufficient longitudinal speed and insufficient maximum-progress gain for the configured duration.
- [ ] Meaningful forward speed or meaningful maximum-progress gain independently keeps the run alive.
- [ ] Brief impacts, reversals, and launch transients remain protected by existing grace and duration rules.
- [ ] Reverse travel is treated according to the approved explicit policy; taking an absolute speed is prohibited.
- [ ] Existing serialized threshold values migrate without resetting tuned assets.
- [ ] Detector evaluation does not perform an independent position projection or derive motion from a different frame.
- [ ] Finish precedence and run-end event behavior remain unchanged.

## Verification

- EditMode tests: Forward, stopped, lateral, reverse, rollback, progress-with-low-speed, speed-with-low-progress, grace, duration, and explicit reverse-recovery cases.
- PlayMode tests: Serialized config survives scene/domain lifecycle; Ladybug stop and rollback scenarios terminate under established timing; finish precedence remains authoritative.
- Static checks: Rider problems clean; Unity compile gate clean before tests; only the shared sample supplies course motion/progress to the detector.
- Manual Unity smoke check: Exercise short impact recovery, deliberate stop, lateral slide, and rollback on the current course.
- Package version/changelog: No package change; note behavior only if the approved signed-speed policy intentionally changes a player-visible edge case.

## Blocked by

- 02 — Ship the straight Run Course sample with exact Ladybug parity
