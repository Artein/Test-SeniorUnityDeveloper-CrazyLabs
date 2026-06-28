## Parent

[Run End Flow First Slice PRD](../../prd/prd-run-end-flow-first-slice.md)

## What to build

Add the first vertical slice for **Run Contact Category** authoring and target-side contact notifications. The **Launch Target** should report copied
collision and trigger payloads through a shallow notifier, and colliders/triggers should declare their run meaning through a shallow `RunContact`
metadata component on the same GameObject as the reporting collider.

This slice should make contact data available and classifiable without ending the run yet. It establishes the adapter boundary that later slices use
for **Run Finish**, **Run Safety Net**, and **Run Obstacle** behavior.

## Acceptance criteria

- [ ] `RunContactCategory` first-slice values are exactly `Surface`, `Obstacle`, `SafetyNet`, and `Finish`.
- [ ] `RunContact` is a shallow MonoBehaviour with serialized category metadata.
- [ ] Run-contact metadata is read from the same GameObject as the reporting Collider/Trigger.
- [ ] Parent groups such as `Run Contacts` are organizational only and are not the semantic source of category data.
- [ ] `RigidbodyContactNotifier` copies Unity collision callback data into immutable notification payloads.
- [ ] Trigger notifications copy the collider identity needed for classification.
- [ ] Raw mutable Unity `Collision` callback objects are not exposed to gameplay services.
- [ ] `RunContactClassifier` can classify copied notifications into zero or more run-ending candidates without owning result priority or state transitions.
- [ ] **Run Surface** emits no candidate.
- [ ] **Run Finish** emits a `Finished` candidate.
- [ ] **Run Safety Net** emits an `OutOfBounds` candidate.
- [ ] **Run Obstacle** only emits `ObstacleHit` when contact-normal impact speed reaches the configured threshold.

## Verification

- EditMode tests:
  - Classifier maps `Surface`, `Finish`, `SafetyNet`, and `Obstacle` to expected candidates.
  - Obstacle contact below normal-impact threshold emits no candidate.
  - Obstacle contact above normal-impact threshold emits `ObstacleHit`.
  - Classifier ignores metadata on unrelated parent objects.
  - Copied collision/trigger payloads contain only the data needed by gameplay services.
- PlayMode tests:
  - Rigidbody notifier raises copied collision and trigger payloads from Unity callbacks.
- Static checks:
  - `git diff --check`.
  - Unity compile through the existing connector.
- Manual Unity smoke check:
  - Temporarily place contact metadata on a collider object and confirm classification follows that object, not its parent.
- Package version/changelog:
  - No package manifest or changelog update expected.

## Blocked by

None - can start immediately
