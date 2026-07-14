# 02 — Prove Authored Run Finish Traversal

## Parent

[High-Speed Run Contact Safety PRD](../../prd/prd-high-speed-run-contact-safety.md) — user stories 4–6, 10–12, 14, 16–20, 27, 29–33, 35–37, 49, 56, 59–60.

## What to build

Add a production-scene PlayMode tracer bullet that moves the authoritative Run Body through the real authored Band 5 Run Finish volume along its crossing normal. Calculate and report the body's projected overlap span with the trigger, prove the 40 m/s acceptance crossing end to end, and record 80 m/s stress behavior separately. Verify the Finished domain result through the existing trigger notification, classification, candidate, priority, and Run End path without claiming that the Rigidbody CCD mode guarantees trigger delivery.

## Acceptance criteria

- [ ] The scenario uses the production Gameplay scene, production Run Body, and real authored Band 5 Run Finish collider.
- [ ] Run Finish remains a trigger collider with explicit Finish Run Contact Category; the test does not depend on object names, tags, or layers as gameplay meaning.
- [ ] The crossing normal is derived from the authored collider orientation and scale rather than assuming a global axis.
- [ ] The test calculates projected trigger thickness plus projected Run Body diameter along the crossing normal.
- [ ] The 40 m/s fixed-step displacement and resulting authored overlap margin are asserted and included in failure diagnostics.
- [ ] At the supported 40 m/s normal crossing speed, exactly one Finished Run Result is accepted and Gameplay State reaches Run Ended.
- [ ] The 80 m/s crossing is reported separately as diagnostic stress headroom and does not silently redefine the supported envelope.
- [ ] The test states explicitly that the result proves sampled trigger traversal for the authored volume, not trigger support from Continuous Dynamic CCD.
- [ ] Existing same-tick Run End priority coverage remains green, with Finished above ObstacleHit, OutOfBounds, and LostMomentum.
- [ ] Scene reload, isolated save state, and bounded condition-based fixed-step polling make the scenario independent of test order.
- [ ] Failure output includes speed tier, fixed timestep, displacement, projected body diameter, projected trigger thickness, overlap margin, start/end positions, trigger count, result count, and final Gameplay State.
- [ ] An insufficient 40 m/s traversal margin or deterministic missed callback remains visible as a failing proof for Issue 04; this issue does not widen the trigger or add runtime fallback behavior.
- [ ] A flaky trigger scenario is treated as a stop condition.
- [ ] No runtime code, scene, prefab, ScriptableObject, package, or ProjectSettings changes are introduced in this proof slice.

## Verification

- Compile gate: Compile the exact project Unity version through the Unity AI Agent Connector; resolve every compile error before running tests.
- PlayMode tests: Run the filtered authored Finish traversal scenario at the 40 m/s acceptance tier and the separately labelled 80 m/s stress tier. Suggested acceptance name: `given_MaxUpgradedSpeedProductionRunBody_when_CrossingAuthoredRunFinish_then_FinishedEndsRun`.
- PlayMode regression: Run the existing Gameplay scene composition and Run Finish contact scenarios.
- EditMode tests: Run existing Run End candidate-priority tests; add no new EditMode test unless a pure projected-margin helper warrants one.
- Static checks: Confirm the authored Finish remains a trigger with explicit Finish category and report its projected traversal margin.
- Manual Unity smoke check: Not required when deterministic PlayMode evidence passes; required only if Issue 04 requests visual authoring review.
- Package version/changelog: Not required; this is test-only safety evidence.

## Blocked by

None — can start immediately and in parallel with Issue 01.
