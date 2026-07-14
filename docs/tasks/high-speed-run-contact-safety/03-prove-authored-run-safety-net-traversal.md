# 03 — Prove Authored Run Safety Net Traversal

## Parent

[High-Speed Run Contact Safety PRD](../../prd/prd-high-speed-run-contact-safety.md) — user stories 3, 5, 10–12, 15–20, 28–33, 35–37, 49, 56, 59–60.

## What to build

Extend the authored-trigger PlayMode tracer with the real Run Safety Net. Drive the production Run Body through the safety volume along its actual crossing normal, calculate its fixed-step traversal margin, and verify exactly one OutOfBounds Run Result through the existing trigger-driven Run End path. Derive and record a credible normal falling-speed acceptance value from current production motion facts; if the project defines no stronger vertical envelope, use the PRD's 40 m/s acceptance tier and keep 80 m/s as separate diagnostic stress headroom.

## Acceptance criteria

- [ ] The scenario uses the production Gameplay scene, production Run Body, and real authored Run Safety Net collider.
- [ ] Run Safety Net remains a trigger collider with explicit SafetyNet Run Contact Category.
- [ ] The crossing normal is derived from the authored collider orientation, scale, and below-course traversal direction rather than assuming a global axis.
- [ ] The accepted normal crossing speed is numeric and traceable to current production motion or the PRD's 40 m/s acceptance tier.
- [ ] The test reuses the generic trigger traversal and projected-margin harness established by Issue 02.
- [ ] The test calculates projected trigger thickness plus projected Run Body diameter along the crossing normal.
- [ ] Fixed-step displacement and authored overlap margin are asserted and included in diagnostics for the accepted normal crossing speed.
- [ ] At the accepted normal crossing speed, exactly one OutOfBounds Run Result is accepted and Gameplay State reaches Run Ended.
- [ ] The 80 m/s crossing is reported separately as diagnostic stress headroom and does not silently redefine product support.
- [ ] The test states explicitly that the result proves sampled trigger traversal for the authored Safety Net, not trigger support from Continuous Dynamic CCD.
- [ ] Scene reload, isolated save state, and bounded condition-based fixed-step polling make the scenario independent of test order.
- [ ] Failure output includes speed tier, fixed timestep, displacement, projected body diameter, projected trigger thickness, overlap margin, start/end positions, trigger count, result count, and final Gameplay State.
- [ ] An insufficient acceptance-tier margin or deterministic missed callback remains visible as failing proof for Issue 04; this issue does not reshape the Safety Net or add runtime fallback behavior.
- [ ] A flaky trigger scenario is treated as a stop condition.
- [ ] No runtime code, scene, prefab, ScriptableObject, package, or ProjectSettings changes are introduced in this proof slice.

## Verification

- Compile gate: Compile the exact project Unity version through the Unity AI Agent Connector; resolve every compile error before running tests.
- PlayMode tests: Run the filtered authored Safety Net traversal scenario at the recorded acceptance speed and the separately labelled 80 m/s stress tier. Suggested acceptance name: `given_ProductionRunBodyAtAcceptedFallingSpeed_when_CrossingAuthoredRunSafetyNet_then_OutOfBoundsEndsRun`.
- PlayMode regression: Run the existing Gameplay scene composition and Run Safety Net contact scenarios.
- EditMode tests: Not required beyond any shared pure projected-margin helper coverage introduced by Issue 02.
- Static checks: Confirm the authored Safety Net remains a trigger with explicit SafetyNet category and report its projected traversal margin.
- Manual Unity smoke check: Not required when deterministic PlayMode evidence passes; required only if Issue 04 requests visual authoring review.
- Package version/changelog: Not required; this is test-only safety evidence.

## Blocked by

Issue 02 — reuse its generic authored-trigger traversal and projected-margin harness.
