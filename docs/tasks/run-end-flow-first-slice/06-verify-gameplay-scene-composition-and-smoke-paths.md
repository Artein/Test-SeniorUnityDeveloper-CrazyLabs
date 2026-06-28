## Parent

[Run End Flow First Slice PRD](../../prd/prd-run-end-flow-first-slice.md)

## What to build

Wire and verify the Gameplay Scene for the completed first slice. The scene should contain the required authored progress frame, run-contact
placeholders, target-side notifier/motion source, run-end config, and VContainer composition references so the feature is playable and testable.

This slice should prove the vertical path in scene context: finish, safety net, obstacle impact, and **Lost Momentum** can each end a **Run**, log one
**Run Result**, and return to **Pre-Launch**. It should not add side **Run Boundary**, separate **Run Ramp**, score UI, result screen, analytics, coins,
or progression.

## Acceptance criteria

- [ ] Gameplay Scene has exactly one assigned **Run Progress Frame** source.
- [ ] Progress frame axes are valid and non-degenerate.
- [ ] Gameplay composition registers the run-end config, progress service/source, run-motion source, contact notifier, classifier, lost-momentum detector, and **Run End Flow**.
- [ ] **Launch Target** has target-side contact notification and run-motion source wiring.
- [ ] Scene has a `Run Contacts` organizational root.
- [ ] Scene has **Run Safety Net**, **Run Finish**, and **Run Obstacle** placeholders.
- [ ] **Run Safety Net** and **Run Finish** are trigger colliders with `RunContact` metadata on the same object.
- [ ] **Run Obstacle** is a physical collider with `RunContact` metadata on the same object.
- [ ] Existing traversal surface is marked as **Run Surface** where needed.
- [ ] Scene composition does not require **Run Boundary**.
- [ ] Scene composition does not require a separate **Run Ramp** category.
- [ ] Placeholder colliders are project-owned wrapper objects; optional plugin visuals are children only.
- [ ] Manual smoke paths verify finish, safety net, obstacle impact, light scrape, and **Lost Momentum**.

## Verification

- EditMode tests:
  - Lifetime-scope validation catches missing run-end config, progress-frame source, motion source, and contact notifier references.
  - Composition resolves the new services and interfaces through VContainer.
- PlayMode tests:
  - Gameplay Scene composition has exactly one assigned progress-frame source with valid axes.
  - Player has target-side notifier and run-motion source.
  - `Run Contacts` root and required placeholders exist.
  - Placeholder colliders/triggers have expected `RunContact` category metadata on the collider object.
  - Scene does not require **Run Boundary** or **Run Ramp**.
- Static checks:
  - `git diff --check`.
  - Unity compile through the existing connector.
  - Targeted EditMode and PlayMode selectors for the new run-end tests.
- Manual Unity smoke check:
  - Launch and fall into **Run Safety Net**.
  - Launch and reach **Run Finish**.
  - Launch and collide hard with **Run Obstacle**.
  - Launch and lightly scrape obstacle below threshold.
  - Launch and let **Lost Momentum** trigger.
  - Verify each case logs one result and returns to **Pre-Launch**.
- Package version/changelog:
  - No package manifest or changelog update expected.

## Blocked by

- 01 - Add Run Progress Frame And RunProgressService
- 02 - Add Run Contact Metadata And Target Notifications
- 03 - End Runs From Finish Safety Net And Obstacle Impact
- 04 - End Runs From Lost Momentum
- 05 - Harden Candidate Priority And Result Latching
