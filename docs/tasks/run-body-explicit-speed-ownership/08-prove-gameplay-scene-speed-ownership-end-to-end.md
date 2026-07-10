# Prove Gameplay Scene Speed Ownership End to End

Type: AFK

## Parent

[PRD: Run Body Explicit Speed Ownership](../../prd/prd-run-body-explicit-speed-ownership.md)

## What to build

Complete and prove the Gameplay scene path from launch through supported running, collisions, run exit, and the next launch using the explicit **Run Body Speed Model**.

Gameplay composition must register the single movement-tuning asset through its narrow interfaces, the pure evaluators, the movement controller, and one movement target. Remove the legacy controller as an active movement writer. Focused integration tests must demonstrate that launch impulse and unsupported flight remain physically driven, first landing preserves tangent momentum while correcting lift, grounded policy applies intentional speed effects, collision overspeed survives initially, upgraded high-speed obstacle approaches still produce reliable collision or run-end outcomes, and lifecycle resets clear every movement-owned state.

This slice proves the architecture in the production scene; it does not introduce new tuning formulas, surface profiles, physics ownership, or unrelated presentation and run-end changes.

## Acceptance criteria

- [x] Gameplay composition resolves one movement-tuning asset through every implemented narrow config interface.
- [x] Exactly one active movement controller and one movement target own fixed-step Run Body writes.
- [x] The legacy steering controller is absent as an active Rigidbody movement writer.
- [x] Launch applies its full initial velocity without speed-model or envelope clamping.
- [x] Unsupported launch flight preserves Rigidbody gravity, collision, and direction-only air-steering behavior.
- [x] First valid landing preserves tangent momentum while the existing landing collaborator corrects surface-normal lift.
- [x] Valid grounded traversal applies downhill acceleration, ordinary slowdown, soft-envelope resistance, and eligible bounded assist through one final write.
- [x] Collision-produced overspeed is visible initially and settles without a hard clamp.
- [x] Neutral and upgraded PlayerMaxSpeed runs produce different effective envelopes in the production scene.
- [x] High-speed obstacle approach still produces reliable physical collision and existing run-end behavior.
- [x] Blocking geometry remains capable of stopping the Run Body after assist budget is exhausted.
- [x] Leaving **Running** clears input, steering frame, landing stabilization, decision diagnostics, and low-speed-assist attempt state.
- [x] A new launch begins with clean movement state and the active-run stat snapshot expected for that run.
- [x] Existing camera, character presentation, reward, economy, pickup, finish, and run-end behavior remains compatible.
- [x] The historical natural-speed PRD and task suite remain preserved and marked superseded; the replacement PRD and ADR remain authoritative.
- [x] No Unity version, package, ProjectSettings, Addressables, save-format, or UPM release change is introduced.

## Completion evidence

- Production-scene ownership scenarios: 2/2 passed (`r_njwedred`).
- Focused scene composition, controller, lifecycle, and compatibility slice: 50/50 passed (`r_ipa77xph`).
- Final changed-test regression: 696/696 passed (`r_c9stm43x`); final full project regression: 970/970 passed (`r_1mhs0tfi`).
- Documentation supersession links resolve, ADR validation passes, and static diff checks found no package, Unity version, ProjectSettings, Addressables, save-format, or UPM release change.

## Verification

- EditMode tests:
  - Composition graph resolves all narrow config interfaces to the same authored source.
  - Movement lifecycle resets all controller-owned collaborators and state.
  - One final movement-target write remains invariant across grounded, unsupported, and transition contexts.
- PlayMode tests:
  - Launch applies initial velocity and unsupported flight remains neutral to grounded speed policy.
  - First valid landing preserves tangent speed while correcting lift.
  - Valid grounded traversal exercises slope acceleration and slowdown.
  - Collision overspeed is not immediately clamped.
  - Neutral and upgraded runs resolve distinct soft envelopes.
  - A high-speed obstacle approach produces reliable collision and existing run-end behavior.
  - Run exit and a subsequent launch start from clean movement state.
  - Scene composition contains no competing legacy writer.
- Static checks:
  - Rider reformat and problem inspection for changed code and tests.
  - Unity connector compile before tests.
  - Search fixed-step registrations and Rigidbody target calls for competing movement writers.
  - Run documentation link and supersession checks.
  - Confirm no package, ProjectSettings, Addressables, or save-format diff.
- Manual Unity smoke check:
  - Run the production Gameplay scene through strong launch, flight, first landing, downhill and flat traversal, collision overspeed, upgraded high-speed obstacle approach, blocked stall, run exit, and relaunch.
- Package version/changelog:
  - Not required.

## Blocked by

- [06 - Add Bounded Low-Speed Assist](06-add-bounded-low-speed-assist.md)
