## Parent

[Pickup Resource Collection PRD](../../prd/prd-pickup-resource-collection.md)

## What to build

Add the generic authored-resource and in-memory grant path that later pickup slices can use. The project should have **Resource Definition** identities,
**Pickup Definition** authored grants, app-session **ResourceStorage** totals, current-run **RunResourceAccumulator** deltas, and sparse immutable
**Run Resource Snapshot** output.

This slice should prove the resource model without relying on scene pickups or Unity physics. It should not add save IDs, persistence, a central
catalog, multipliers, icons, display names, VFX/SFX, or economy concepts.

## Acceptance criteria

- [x] **Resource Definition** is an authored asset-backed identity suitable for keying resource balances by asset reference.
- [x] **Pickup Definition** references exactly one **Resource Definition** and one positive base amount.
- [x] Missing **Resource Definition** and zero/negative **Pickup Definition** amounts are rejected loudly.
- [x] `ResourceStorage` grants positive amounts and returns zero for resources with no stored balance.
- [x] `RunResourceAccumulator` grants positive amounts, accumulates by **Resource Definition**, resets for the next **Run**, and can create a snapshot.
- [x] **Run Resource Snapshot** is immutable and sparse; missing resources read as zero.
- [x] Runtime code and tests use generic resource/pickup terminology, not coin-specific controller names.

## Verification

- EditMode tests:
  - `ResourceStorage` adds and queries amounts by **Resource Definition**.
  - `ResourceStorage` returns zero for missing balances.
  - `RunResourceAccumulator` accumulates grants by resource.
  - `RunResourceAccumulator` reset clears current-run deltas.
  - **Run Resource Snapshot** remains unchanged after later accumulator changes.
  - **Run Resource Snapshot** returns zero for missing resources.
  - Invalid **Pickup Definition** authoring is rejected.
- PlayMode tests:
  - None expected for this slice.
- Static checks:
  - `git diff --check`.
  - Unity compile through Unity AI Agent Connector.
- Manual Unity smoke check:
  - Create temporary resource and pickup definition assets and confirm invalid references/amounts are surfaced clearly.
- Package version/changelog:
  - No package manifest or changelog update expected.

## Blocked by

None - can start immediately
