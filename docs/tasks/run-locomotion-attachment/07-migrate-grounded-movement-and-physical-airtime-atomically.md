# 07 — Migrate Grounded Movement and Physical Airtime Atomically

## Parent

[Run Locomotion Attachment and Steering Support Separation PRD](../../prd/prd-run-locomotion-attachment-and-steering-support.md) — user stories 1–16, 47, 49, 53–59, 72–77, 88–89, 107–109, 116–119, and 127.

## What to build

Make Run Locomotion Attachment the sole physical grounded authority in one atomic consumer migration. Gate grounded speed effects, physical tangent projection, grounded correction, steering-mode selection, and landing stabilization with attachment and its physical support frame. Start and end physical airtime from attachment transitions. Keep heading and facing on the Run Steering Frame, preserve Rigidbody contact authority and one movement writer, and leave retained support available only for steering-oriented migration in issue 08.

## Acceptance criteria

- [ ] Only Run Locomotion Attachment can authorize grounded movement, speed-model effects, grounded correction, landing stabilization, or physical airtime state.
- [ ] Attached movement uses the attachment physical support frame for tangent and normal decomposition.
- [ ] Detached movement receives no grounded acceleration, slowdown, low-speed assist, soft-envelope resistance, or grounded correction.
- [ ] A current greater-than-60-degree support change affects physical projection immediately even while steering orientation remains filtered.
- [ ] Positive lift or expired continuity prevents same-tick grounded effects after the surface pipeline publishes Detached.
- [ ] Steering mode receives attachment explicitly and does not infer physical state from steering-frame validity.
- [ ] Heading and facing continue using steering intent and Run Steering Frame without allowing that frame to define the physical support plane.
- [ ] Landing stabilization acts on the approved physical acquisition or reattachment transition exactly once.
- [ ] Physical airtime begins on attachment loss and ends on attachment acquisition.
- [ ] A valid Bounded Continuity Bridge does not start physical airtime; steering retention alone cannot suppress it.
- [ ] Launch begins Detached and cannot receive grounded behavior before the first valid landing.
- [ ] Movement and physical airtime migrate in the same slice; no merged state leaves one on attachment and the other on Stable Support.
- [ ] Repository search finds no physical consumer still reading retained Stable Support after migration.
- [ ] The fixed-step order remains Progress, Surface Frame, Movement, Air Time, and Lost Momentum.
- [ ] The Run Body Movement Controller remains the only active movement writer, and Rigidbody retains gravity, collision, contact, separation, and external normal-velocity authority.
- [ ] VContainer resolves one coherent attachment snapshot for movement, steering mode, landing, and airtime in each fixed step.

## Verification

- EditMode tests: Cover attached and detached speed decisions, physical-plane selection, immediate sharp-normal use, lift detachment, landing stabilization, launch lifecycle, airtime transitions, one movement write, and snapshot coherence.
- PlayMode tests: Run walk-off, ramp departure, seam bridge, true gap, greater-than-60-degree transition, launch, and landing at 5, 20, and 40 metres per second.
- Static checks: Search every grounded movement and airtime consumer for retained Stable Support usage; confirm no second writer, duplicate state inference, or dependency-direction violation.
- Manual Unity smoke check: Compare grounded acceleration, ramp takeoff, air steering, airtime, and landing across low and high speeds with diagnostics visible.
- Package version/changelog: Record a project gameplay behavior note only if repository release policy requires it; no package version change.

## Blocked by

- [06 — Enforce Approved High-Speed Edge Precision](06-enforce-approved-high-speed-edge-precision.md)
