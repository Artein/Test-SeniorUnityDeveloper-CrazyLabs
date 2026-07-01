## Parent

[Run Ended UI And Result Acknowledgement PRD](../../prd/prd-run-ended-ui-and-result-acknowledgement.md)

## What to build

Close the integrated **Run Ended** gameplay loop across state, rewards, UI, pose lock, acknowledgement, and return to **Run Preparation**. This slice
should prove the pieces work together in the gameplay scene: a run ends, rewards are committed, the result UI appears, the target stays held, terminal
presentation remains active, and tapping after the guard returns to the upgrade screen.

This is a verification and integration slice, not a place for broad redesign.

## Acceptance criteria

- [ ] A failed run can enter **Run Ended**, show result UI, and return to **Run Preparation** after acknowledgement.
- [ ] A successful run can enter **Run Ended**, show result UI, and return to **Run Preparation** after acknowledgement.
- [ ] Rewards from the accepted **Run Result** are committed before the player reaches **Run Preparation**.
- [ ] The **Run Preparation UI** sees the updated coin balance after acknowledgement.
- [ ] The **Run Ended UI** displays earned-this-run coins, not total balance.
- [ ] The **Launch Target** remains held while **Run Ended UI** is visible.
- [ ] Terminal character presentation remains active while **Run Ended UI** is visible.
- [ ] Tap before the guard does not skip the result screen.
- [ ] Tap after the guard returns to **Run Preparation**.
- [ ] No runtime-created duplicate result UI appears in Play Mode.

## Verification

- EditMode tests:
  - Keep coverage from the dependent slices; add only missing orchestration tests if a pure integration seam exists.
- PlayMode tests:
  - Gameplay scene can enter **Run Ended** from a test-controlled accepted result.
  - Result UI is visible and populated from the accepted result.
  - Early tap is ignored before the guard.
  - Tap after the guard returns to **Run Preparation**.
  - Run rewards are visible to the preparation/economy path after acknowledgement.
  - No duplicate result UI hierarchy is present after one frame in Play Mode.
- Static checks:
  - `git diff --check`.
  - Unity compile through the existing connector.
- Manual Unity smoke check:
  - Perform one failure and one finish in the editor; inspect result UI, pose hold, animation, acknowledgement, and upgraded preparation return.
- Package version/changelog:
  - No package manifest or changelog update expected.

## Blocked by

- 03 - Add Scene-Authored Run Ended UI
- 04 - Hold Launch Target During Run Ended
- 05 - Verify Terminal Character Presentation In Run Ended
