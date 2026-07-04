# 02. Slingshot Pull and Band Authoring Guidance

Type: AFK

## Parent

`docs/prd/prd-gameplay-config-authoring-guidance.md`

## What to build

Add first-pass **Authoring Guidance** to the **Slingshot** config surface so a reviewer can understand **Pull** capture, minimum and maximum pull depth, **Pull Offset** limits, **Band Shape** clearance, silhouette sampling, wrap sampling, and **Band Release Recoil** presentation directly in the Unity Inspector.

This slice should explain which values affect gameplay input and pull interpretation versus which values only affect visual **Band Shape** quality or recoil presentation. It should not change how **Slingshot** calculates pull, launch readiness, band contact, or recoil.

## Acceptance criteria

- [ ] Touch target radius guidance explains input forgiveness and pixel units.
- [ ] Pull distance guidance explains weak pull rejection, full pull depth, and relationship to normalized **Pull Strength** without changing pull normalization.
- [ ] Maximum lateral pull guidance explains authored side-to-side **Pull Offset** limits without implying it is **Run Steering Control**.
- [ ] Band contact, silhouette sample, wrap sample, recoil duration, and recoil curve guidance clearly distinguish visual **Band Shape** presentation from launch force.
- [ ] Substantive guidance uses labelled paragraphs separated by blank lines: `Controls:`, `Impact:`, and `Typical:`.
- [ ] Existing serialized names, code defaults, `[Min]` and `[Range]` attributes, validators, public config interface, asset values, and runtime **Slingshot** behavior are unchanged.
- [ ] No new asmdef, package, custom inspector, editor code, asset, or scene change is introduced.

## Verification

- EditMode tests: Not added; no behavior, validator, default, or serialization contract changes are expected.
- PlayMode tests: Not added; no runtime **Slingshot** behavior changes are expected.
- Static checks: Reformat the edited C# file, inspect Rider/file problems, and run Unity compile through Unity AI Agent Connector.
- Manual Unity smoke check: Open the **Slingshot** config asset in the Inspector and spot-check tooltip readability, blank-line spacing, and correct separation of gameplay versus visual settings.
- Package version/changelog: Not required for this metadata-only project change.

## Blocked by

None - can start immediately
