## Parent

[Run End Reward Breakdown Reveal PRD](../../prd/prd-run-end-reward-breakdown-reveal.md)

## What to build

Add the **Run Reward Reveal** sequence to the scene-owned **Run Ended UI**.

When visible state is applied, the view should reveal the result in order: run result label, reached distance counter, optional best-improvement feedback, each **Run Reward Source Row**, total run coins, then **Tap to Continue**. A tap during reveal should fast-forward all pending reveal steps to final values without raising **Acknowledge Run Result**. Only a later tap after reveal completion should request acknowledgement.

Use Unity coroutines for this slice. Add a code TODO near the coroutine path noting that UniTask migration can be considered if the project later standardizes on UniTask.

## Acceptance criteria

- [x] **Run Ended UI** starts a reveal sequence when visible result state is applied.
- [x] Run result label fades in before numeric counters start.
- [x] Reached distance fades in and counts up to the accepted display value.
- [x] Best-improvement feedback fades in after reached distance and before reward source rows when present.
- [x] Source rows reveal in provided order, each with label/icon/amount fade-in and amount count-up.
- [x] Source row amounts render as unsigned white coin values.
- [x] Total run coins appears after all source rows, uses `RUN TOTAL` copy, and reveals its icon with the final calculated total value.
- [x] **Tap to Continue** is hidden or disabled until the reveal reaches the final state.
- [x] A tap during reveal fast-forwards every pending step to final visual values.
- [x] The fast-forward tap does not raise **Acknowledge Run Result**.
- [x] A second tap after reveal completion raises **Acknowledge Run Result**.
- [x] Applying hidden state stops any active reveal coroutine and hides reveal content.
- [x] Reveal code uses coroutines, not UniTask or a new async package.
- [x] A code TODO near the coroutine sequence records possible future UniTask migration.

## Verification

- EditMode tests:
  - Pure formatter/counter helpers, if introduced, clamp and format values deterministically.
  - Presenter state remains source-agnostic and does not own reveal timing.
- PlayMode tests:
  - Applying visible **Run Ended** state begins with content hidden according to the first reveal step.
  - Reveal advances in order through result label, distance, optional best improvement, rows, total, and **Tap to Continue**.
  - Row label, icon, and amount reveal together.
  - Final total displays `RUN TOTAL`, the total coin icon, and the accepted total value.
  - A tap during reveal fast-forwards to all final values.
  - Fast-forward tap does not acknowledge the result.
  - A second tap after reveal completion acknowledges the result.
  - Applying hidden state cancels active reveal and hides the root/content.
- Static checks:
  - Reformat changed C# files and inspect Rider problems.
  - Unity compile through the existing connector.
- Manual Unity smoke check:
  - End a run, watch the reveal, tap during reveal, then tap again to continue; confirm only the second tap leaves **Run Ended**.
- Package version/changelog:
  - No package manifest or changelog update expected.

## Blocked by

- 01 - Picked-Up Coins Run Reward Source Tracer Bullet
