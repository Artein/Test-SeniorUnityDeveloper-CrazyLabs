# 11 — Retire Stable Support Compatibility and Align Documentation

## Parent

[Run Locomotion Attachment and Steering Support Separation PRD](../../prd/prd-run-locomotion-attachment-and-steering-support.md) — user stories 68, 84–86, and 123–128.

## What to build

Complete the migration after human acceptance by removing Stable Support compatibility contracts, obsolete grounded members, duplicate temporal machinery, and superseded serialized fields. Make every consumer declare Observed Support, Run Locomotion Attachment, Steering Support Target, Run Steering Frame, physical airtime, or reward qualification explicitly. Align diagnostics, architecture documentation, earlier PRD precedence, and final tuning records with the accepted four-concept model, then run the complete relevant regression suite.

## Acceptance criteria

- [ ] No active runtime consumer reads Stable Support or an equivalent ambiguous retained-grounding contract.
- [ ] Temporary compatibility adapters, obsolete interfaces, and compatibility-only registrations are removed.
- [ ] Steering Support Target contains no grounded authority and Run Locomotion Attachment remains the only grounded authority.
- [ ] Duplicate missing-support, suspect-normal, attachment, steering, airtime, or reward state is removed.
- [ ] Obsolete serialized fields are removed or migrated exactly as approved without losing final authored values.
- [ ] Repository search finds no unexplained Stable Support terminology, old field names, raw labels on filtered values, or consumer-local support grace.
- [ ] Every runtime consumer's selected surface or airtime concept is explicit in its contract and tests.
- [ ] Diagnostics use Observed Support, Run Locomotion Attachment, Steering Support Target, and Run Steering Frame consistently.
- [ ] The earlier Run Surface stability documentation is marked superseded where it assigns physical authority to retained support.
- [ ] The earlier Run Steering Frame rationale remains available for steering feel but no longer describes physical grounding.
- [ ] ADR-0010's Rigidbody authority and one-writer decision remain unchanged.
- [ ] Final attachment, steering, precision, airtime, reward, and serialized-authoring decisions are documented with their accepted values and units.
- [ ] Compilation is clean before tests, all focused regressions pass, and the full relevant EditMode and PlayMode assemblies pass.
- [ ] No new package, asmdef, Unity version, ProjectSettings, Addressables, save-format, installation, or sync change is introduced.
- [ ] Final behavior matches issue 10's approved matrix without a compatibility flag or parallel authority.

## Verification

- EditMode tests: Run the complete observation, attachment, steering, movement, airtime, reward, configuration, composition, lifecycle, and allocation coverage.
- PlayMode tests: Run the full edge, ramp, seam, gap, sharp-transition, launch, landing, reset, and reward matrix at all approved speeds.
- Static checks: Search for legacy contracts, fields, filters, registrations, terminology, and physical consumers; run Rider cleanup and inspections on changed C# files and repository diff checks.
- Manual Unity smoke check: Complete one approved end-to-end run with diagnostics through seam, ramp, edge, airborne, landing, and reward result.
- Package version/changelog: Update project release notes only if required by accepted player-visible behavior; no package version or installation change.

## Blocked by

- [10 — Tune and Approve Production Edge, Ramp, and Seam Behavior](10-tune-and-approve-production-edge-ramp-and-seam-behavior.md)
