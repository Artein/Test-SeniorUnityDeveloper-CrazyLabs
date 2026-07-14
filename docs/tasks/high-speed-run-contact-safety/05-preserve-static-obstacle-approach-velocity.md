# 05 — Preserve Static-Obstacle Approach Velocity Across CCD Contact

## Parent

[04 — Approve Contact-Safety Evidence and Remediation Boundary](04-approve-contact-safety-evidence-and-remediation-boundary.md), selected from the deterministic 40 m/s Continuous Dynamic obstacle failure in [High-Speed Run Contact Safety PRD](../../prd/prd-high-speed-run-contact-safety.md).

## What to build

Preserve authoritative approach-speed evidence when Unity Continuous Dynamic detects and physically resolves a static Run Obstacle but reports zero callback relative velocity. Reconstruct the production Run Body's pre-solve approach velocity from its callback-time post-solve velocity and the collision impulse divided by body mass. Keep this Unity-specific capture inside the existing shallow Rigidbody Contact Notifier, carry the evidence only through internal notification state, and let Run Contact Classifier apply the existing contact-normal Obstacle Impact threshold.

Do not add a swept query: the reproduced failure has an authoritative collision callback and solid response, so detection is already present. Keep moving-obstacle fallback unsupported, preserve the callback path as the only candidate source, and retain existing Run End priority and one-result latching.

## Acceptance criteria

- [ ] The original production-scene 40 m/s Continuous Dynamic thin-obstacle scenario passes with exactly one ObstacleHit Run Result and Run Ended state while the identical Discrete control still misses.
- [ ] The notifier reconstructs static-obstacle approach velocity as callback-time body velocity minus collision impulse divided by finite positive body mass.
- [ ] Reconstruction is restricted to colliders without an attached Rigidbody; moving-obstacle relative velocity remains unsupported.
- [ ] Non-finite velocity, non-finite impulse, missing Rigidbody, or invalid mass produces no fallback evidence.
- [ ] Run Contact Classifier evaluates reconstructed velocity only as fallback evidence and applies the unchanged configured threshold along each finite contact normal.
- [ ] Direct impacts at or above threshold end the Run; below-threshold contacts and tangent scrapes do not.
- [ ] Existing nonzero callback relative velocity remains authoritative and compatible.
- [ ] No sweep, per-fixed-step query, trigger surrogate, static service, public API, serialized field, scene/prefab/ScriptableObject, package, save-format, or ProjectSettings change is introduced.
- [ ] The production sphere, solid obstacle response, explicit Obstacle category, Run End candidate boundary, priority order, and one-result latch remain unchanged.
- [ ] The deterministic failing scenario remains the end-to-end regression test; focused calculator, classifier, and notifier tests cover the new seam.
- [ ] Compile, targeted EditMode, targeted PlayMode, related regressions, repeated physics proofs, and the full project test suite are green.

## Verification

- EditMode tests: approach-velocity reconstruction for valid, invalid, direct, below-threshold, and tangential evidence; Run Contact Classifier threshold behavior; Run End priority regressions.
- PlayMode tests: Rigidbody Contact Notifier copied evidence; production 40 m/s Continuous Dynamic positive; identical Discrete negative; authored Finish/Safety Net acceptance and 80 m/s stress scenarios.
- Static checks: no public/serialized API or shipped content/settings changes; no physics sweep; static-obstacle eligibility is explicit.
- Manual Unity smoke check: not required when deterministic production-scene and related physics regressions pass; record as not run.
- Package version/changelog: no package bump; record the internal shipped-behavior correction in the task evidence and identify release-note follow-up if project policy requires it.

## Blocked by

04 — Approve Contact-Safety Evidence and Remediation Boundary. The human owner approved the evidence-driven implementation by agreeing to the local issue plan and instructing Codex to implement the full task set.
