## Parent

[Pickup Resource Collection PRD](../../prd/prd-pickup-resource-collection.md)

## What to build

Connect current-run resource deltas to **Run Result**. When a **Run** ends, the immutable result payload should include the **Run Resource Snapshot**
for that just-ended run. When gameplay returns to **Pre-Launch** for the next **Run**, the accumulator should reset without mutating the already
captured result.

This slice should keep the result contract generic and compatible with future end-of-run UI. If the project still lacks a run-result delivery surface,
add only the minimal direct C# notification path needed by current consumers.

## Acceptance criteria

- [ ] **Run Result** includes a **Run Resource Snapshot** captured at accepted run end.
- [ ] Existing run-result fields and behavior continue to work.
- [ ] `RunResourceAccumulator` resets when entering **Pre-Launch** for the next **Run**, not at launch time.
- [ ] A captured **Run Result** keeps its snapshot stable after the accumulator is reset or receives later grants.
- [ ] Missing resources in the result snapshot read as zero for UI consumers.
- [ ] Any new run-result observation path uses direct C# events and does not introduce an event bus.

## Verification

- EditMode tests:
  - Run-end flow captures current-run resource deltas into **Run Result**.
  - Reset on **Pre-Launch** clears the next run's accumulator without changing an existing result.
  - Existing run-end success/failure, elapsed, distance, final position, and speed expectations still pass.
  - Direct result notification, if added, emits one immutable **Run Result** per accepted run end.
- PlayMode tests:
  - None expected unless existing run-end scene tests need composition coverage for the new result dependency.
- Static checks:
  - `git diff --check`.
  - Unity compile through Unity AI Agent Connector.
- Manual Unity smoke check:
  - End a run and confirm logged/result-observed data contains a resource snapshot even when no pickups were collected.
- Package version/changelog:
  - No package manifest or changelog update expected.

## Blocked by

- 01 - Add Generic Resource Grant Data Path
