# 06. Inspector Readability Smoke Review

Type: HITL

## Parent

`docs/prd/prd-gameplay-config-authoring-guidance.md`

## What to build

Perform a human Unity Inspector smoke review of the completed first-pass **Authoring Guidance**. The reviewer should open the five core gameplay config assets and judge whether the tooltip copy quickly answers what each value controls, whether it should be tweaked, and what impact a tweak is likely to have.

This slice is HITL because scanability, signal-to-noise, and reviewer confidence are human-quality checks. If review finds confusing guidance, update only the relevant tooltip copy while preserving the metadata-only scope.

## Acceptance criteria

- [ ] The reviewer opens the **Slingshot**, gameplay **Launch Impulse**, **Run Steering Control**, **Run Camera**, and **Run End Flow** config assets in Unity Inspector.
- [ ] Tooltip text is readable with blank-line spacing where substantive guidance uses labelled paragraphs.
- [ ] Simple fields do not feel over-documented or noisy.
- [ ] Guidance uses project glossary terms consistently and avoids casual synonyms that blur domain concepts.
- [ ] Visual-only settings are not described as gameplay force, and defensive guard rails are not described as player-facing balance.
- [ ] Tooltip copy does not duplicate exact current asset values unless the number is a stable constraint, fixed unit, attribute-backed bound, or genuinely useful example.
- [ ] Any unclear guidance found during review is either fixed in the same slice or recorded as a follow-up before the issue is considered complete.

## Verification

- EditMode tests: Not required for the review itself; only run focused tests if review-driven edits expand into behavior or validator changes.
- PlayMode tests: Not required for the review itself; only run if review-driven edits expand into runtime behavior.
- Static checks: If C# tooltip copy is edited during review, reformat edited files, inspect Rider/file problems, and run Unity compile through Unity AI Agent Connector.
- Manual Unity smoke check: Required; this issue is the manual Inspector readability check.
- Package version/changelog: Not required for this metadata-only project change.

## Blocked by

- 01. Run Camera Authoring Guidance Tracer
- 02. Slingshot Pull and Band Authoring Guidance
- 03. Launch Impulse Authoring Guidance
- 04. Run Steering Control Authoring Guidance Standardization
- 05. Run End Flow and Reward Authoring Guidance
