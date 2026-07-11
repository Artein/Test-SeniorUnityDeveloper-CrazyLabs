# 12 — Retire Legacy Surface Context and Align Documentation

## Parent

[Run Surface Probing and Stability PRD](../../prd/prd-run-surface-probing-and-stability.md) — user stories 46–47, 55, 68–72, 75.

## What to build

Complete the migration by moving remaining consumers to explicit observed or stable support, removing the compatibility surface contract and duplicate temporal machinery, and aligning architecture and planning documentation with the approved model and final tuning ownership.

## Acceptance criteria

- [ ] Every runtime consumer declares whether it uses observed support, stable support, transition, or steering frame.
- [ ] No runtime consumer reads the legacy compatibility surface context.
- [ ] The compatibility adapter and obsolete interface are removed.
- [ ] Legacy missed-sample and suspect-normal state is removed from physics and steering sources.
- [ ] Duplicate serialized stability fields are removed or migrated according to issue 01 without losing authored values.
- [ ] The final authoring location and units for every probe and stability field are documented.
- [ ] Architecture documentation describes the one-way flow from physics observation to stability policy to steering and consumers.
- [ ] Earlier overlapping PRDs or tasks carry the approved supersession or precedence status.
- [ ] Diagnostics and code use the same observed/stable/transition terminology.
- [ ] No package dependency, project setting, Rigidbody authority, or movement-writer invariant changes.
- [ ] The full relevant EditMode and PlayMode suites pass after legacy removal.
- [ ] Repository search finds no unexplained legacy type names or raw-label drift.

## Verification

- EditMode tests: Run all run-surface policy, steering, locomotion, airtime, presentation, and diagnostic tests.
- PlayMode tests: Run the full canonical physics and consumer regression suite.
- Static checks: Search for legacy contracts, counters, suspect-normal state, obsolete serialized fields, and inconsistent terminology; run code cleanup and IDE inspections on changed C# files.
- Manual Unity smoke check: Complete one representative run across seam, trough, gap, jump, edge, and landing with diagnostics enabled.
- Package version/changelog: Update release notes or changelog if required for the completed internal architecture and any approved behavior changes.

## Blocked by

- [07 — Migrate Core Locomotion to Explicit Stable Support](07-migrate-core-locomotion-to-explicit-stable-support.md)
- [08 — Apply Approved Airtime Support Semantics](08-apply-approved-airtime-support-semantics.md)
- [09 — Apply Approved Character Presentation Support Semantics](09-apply-approved-character-presentation-support-semantics.md)
- [10 — Expose Observed, Stable, Transition, and Steering Diagnostics](10-expose-observed-stable-transition-and-steering-diagnostics.md)
- [11 — Run Seam, Trough, and Brief-Airborne Feel Review](11-run-seam-trough-and-brief-airborne-feel-review.md)
