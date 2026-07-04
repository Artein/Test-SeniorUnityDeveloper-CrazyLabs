# 05. Run End Flow and Reward Authoring Guidance

Type: AFK

## Parent

`docs/prd/prd-gameplay-config-authoring-guidance.md`

## What to build

Add first-pass **Authoring Guidance** to the **Run End Flow** and run reward config surface so a reviewer can understand obstacle-impact ending, **Lost Momentum** detection, result acknowledgement gating, **Distance Bonus**, and **Air Time Bonus** directly in the Unity Inspector.

This slice should distinguish run-ending thresholds from reward conversion rates and avoid implying any rebalance. It should not alter run-end detection, reward calculation, UI acknowledgement behavior, or serialized tuning values.

## Acceptance criteria

- [ ] Obstacle impact guidance explains the speed threshold that makes contact end a **Run**.
- [ ] **Lost Momentum** launch grace, duration, speed threshold, and progress threshold guidance explains sustained stall detection versus momentary slowdown.
- [ ] **Run Ended Acknowledge Guard** guidance explains input gating after result presentation.
- [ ] **Distance Bonus** guidance explains distance-to-coins conversion without confusing it with **Run Distance Display**.
- [ ] **Air Time Bonus** guidance explains unsupported-travel-to-coins conversion without implying character presentation changes.
- [ ] Substantive guidance uses labelled paragraphs separated by blank lines: `Controls:`, `Impact:`, and `Typical:`.
- [ ] Existing serialized names, code defaults, `[Min]` attributes, public config interfaces, asset values, and runtime run-end/reward behavior are unchanged.
- [ ] No new asmdef, package, custom inspector, editor code, asset, or scene change is introduced.

## Verification

- EditMode tests: Not added; no behavior, validator, default, or serialization contract changes are expected.
- PlayMode tests: Not added; no runtime **Run End Flow** or reward behavior changes are expected.
- Static checks: Reformat the edited C# file, inspect Rider/file problems, and run Unity compile through Unity AI Agent Connector.
- Manual Unity smoke check: Open the **Run End Flow** config asset in the Inspector and spot-check tooltip readability, blank-line spacing, and separation of ending thresholds from reward rates.
- Package version/changelog: Not required for this metadata-only project change.

## Blocked by

None - can start immediately
