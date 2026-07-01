## Parent

[Run Ended UI And Result Acknowledgement PRD](../../prd/prd-run-ended-ui-and-result-acknowledgement.md)

## What to build

Add the scene-authored **Run Ended UI** path. The screen should be serialized in the gameplay scene, registered as a scene-owned view, rendered by a
presenter from accepted **Run Result** data, and tapped to request **Acknowledge Run Result**.

The runtime may toggle authored roots and fill serialized text/image references. It must not create the result UI hierarchy at runtime.

## Acceptance criteria

- [ ] Gameplay scene contains one authored **Run Ended UI** view under the existing gameplay UI structure.
- [ ] The view has serialized references for root visibility, title, earned coins, reached distance, best-run improvement, and tap area.
- [ ] The view exposes an acknowledgement request event without owning gameplay-state transitions.
- [ ] A presenter renders visible state only while the gameplay state is **Run Ended**.
- [ ] The presenter renders accepted current **Run Result** data, not live run counters.
- [ ] Success and failure can select different designer-authored title states.
- [ ] Earned coins render from **Run Currency Snapshot** data.
- [ ] Reached distance renders as whole meters.
- [ ] Best-run improvement is visible only when the accepted result beat the previous session best.
- [ ] The scene-owned view is registered through VContainer without treating the MonoBehaviour as an owned disposable service.
- [ ] No duplicate **Run Ended UI** hierarchy is created during Play Mode.

## Verification

- EditMode tests:
  - Presenter hides the view outside **Run Ended**.
  - Presenter shows the view in **Run Ended** after an accepted **Run Result**.
  - Presenter maps success and failure results to the expected view state.
  - Presenter forwards tap requests through **Acknowledge Run Result**.
  - View validation rejects missing required serialized references.
- PlayMode tests:
  - Gameplay scene contains one authored **Run Ended UI** view.
  - Required serialized references are non-null.
  - Composition resolves the scene-owned view and presenter dependencies.
  - Entering **Run Ended** shows the UI root.
  - Play Mode does not create a duplicate result UI hierarchy.
- Static checks:
  - `git diff --check`.
  - Unity compile through the existing connector.
- Manual Unity smoke check:
  - End a run and confirm the result screen appears immediately with coins, meters, improvement state, and tap prompt.
- Package version/changelog:
  - No package manifest or changelog update expected.

## Blocked by

- 01 - Add Acknowledge Run Result Flow
- 02 - Add Run Result Stats And Session Best Distance
