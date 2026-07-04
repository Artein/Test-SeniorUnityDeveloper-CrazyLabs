# 03. Launch Impulse Authoring Guidance

Type: AFK

## Parent

`docs/prd/prd-gameplay-config-authoring-guidance.md`

## What to build

Add first-pass **Authoring Guidance** to the gameplay-owned **Launch Impulse** config surface so a reviewer can understand how minimum and maximum forward impulse, normalized **Pull Strength**, lateral launch angle, lateral angle curve, upward impulse, and optional total impulse clamps shape launch behavior.

This slice should make clear that **Pull Strength** maps authored pull depth to launch force, **Pull Offset** bends the accepted launch direction, and total impulse clamps are defensive bounds only when enabled. It should not rebalance launch values or change validation behavior.

## Acceptance criteria

- [ ] Forward impulse guidance explains minimum versus maximum **Launch Impulse** and how **Pull Strength** selects between them.
- [ ] Pull strength curve guidance explains normalized pull-depth mapping without prescribing a new balance curve.
- [ ] Lateral launch angle and lateral angle curve guidance explain how **Pull Offset** bends the launch direction and distinguish it from **Run Steering Control**.
- [ ] Upward impulse guidance explains lift, arc, airtime, and run-entry feel.
- [ ] Optional total impulse clamp guidance explains the enable toggles and clamp values as defensive bounds when active.
- [ ] Substantive guidance uses labelled paragraphs separated by blank lines: `Controls:`, `Impact:`, and `Typical:`.
- [ ] Existing serialized names, code defaults, `[Min]` and `[Range]` attributes, validators, public config interface, asset values, and runtime launch behavior are unchanged.
- [ ] No new asmdef, package, custom inspector, editor code, asset, or scene change is introduced.

## Verification

- EditMode tests: Not added; no behavior, validator, default, or serialization contract changes are expected.
- PlayMode tests: Not added; no runtime launch behavior changes are expected.
- Static checks: Reformat the edited C# file, inspect Rider/file problems, and run Unity compile through Unity AI Agent Connector.
- Manual Unity smoke check: Open the gameplay **Launch Impulse** config asset in the Inspector and spot-check tooltip readability, blank-line spacing, and absence of stale copied asset values.
- Package version/changelog: Not required for this metadata-only project change.

## Blocked by

None - can start immediately
