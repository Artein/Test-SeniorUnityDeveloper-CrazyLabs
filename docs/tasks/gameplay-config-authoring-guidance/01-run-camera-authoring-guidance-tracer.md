# 01. Run Camera Authoring Guidance Tracer

Type: AFK

## Parent

`docs/prd/prd-gameplay-config-authoring-guidance.md`

## What to build

Add first-pass **Authoring Guidance** to the **Run Camera** config surface so a reviewer can understand the **Run Camera Anchor**, anchor smoothing, camera-facing yaw smoothing, and low-speed yaw hold behavior directly in the Unity Inspector.

This slice should be metadata-only. It should make the smallest useful C# change to explain the existing serialized tuning values without changing config behavior, validation, assets, or camera runtime logic.

## Acceptance criteria

- [ ] Every tweakable **Run Camera** serialized value has useful Inspector guidance beside the field.
- [ ] Guidance explains units or scale where useful, including offset units, response-rate feel, and yaw speed threshold meaning.
- [ ] Substantive guidance uses labelled paragraphs separated by blank lines: `Controls:`, `Impact:`, and `Typical:`.
- [ ] Simple guidance stays concise and does not restate only the field name.
- [ ] Existing serialized names, code defaults, `[Min]` attributes, public config interface, serialization constants, assets, and runtime camera behavior are unchanged.
- [ ] No new asmdef, package, custom inspector, editor code, asset, or scene change is introduced.

## Verification

- EditMode tests: Not added; no behavior, validator, default, or serialization contract changes are expected.
- PlayMode tests: Not added; no runtime camera behavior changes are expected.
- Static checks: Reformat the edited C# file, inspect Rider/file problems, and run Unity compile through Unity AI Agent Connector.
- Manual Unity smoke check: Open the **Run Camera** config asset in the Inspector and spot-check tooltip readability, blank-line spacing, and absence of stale copied asset values.
- Package version/changelog: Not required for this metadata-only project change.

## Blocked by

None - can start immediately
