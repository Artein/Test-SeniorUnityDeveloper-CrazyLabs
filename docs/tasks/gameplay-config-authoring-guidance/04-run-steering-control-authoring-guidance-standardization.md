# 04. Run Steering Control Authoring Guidance Standardization

Type: AFK

## Parent

`docs/prd/prd-gameplay-config-authoring-guidance.md`

## What to build

Standardize and complete **Authoring Guidance** on the **Run Steering Control** config surface. Existing inline tooltips should be revised where needed so a reviewer can understand steering range, deadzone, responsiveness, DPI fallback, turn rates, air steering, minimum steer speed, defensive speed sanity guard, launch landing stabilization, and **Run Steering Frame** stability directly in the Unity Inspector.

This slice should improve the existing tooltip style rather than introducing a helper abstraction. It should replace stale or overly exact baseline wording when impact/range guidance would be safer, while preserving every runtime contract.

## Acceptance criteria

- [ ] Existing tooltip text is standardized to the agreed **Authoring Guidance** style and project glossary terms.
- [ ] Run steering range, deadzone, and responsiveness guidance explains physical control range, neutral jitter filtering, and heavy versus snappy steering feel.
- [ ] DPI guidance explains fallback and accepted bounds as screen-metric protection, not gameplay tuning.
- [ ] Grounded and airborne turn-rate guidance explains units and deliberate airborne steering reduction.
- [ ] Minimum steer speed and **Run Body Speed Sanity Guard** guidance clearly distinguish player-facing steering behavior from defensive physics containment.
- [ ] Launch landing stabilization guidance explains early post-launch lift suppression without changing landing behavior.
- [ ] **Run Steering Frame** guidance explains support-normal smoothing, snap/suspect handling, ungrounded grace, and confirmation timing.
- [ ] Substantive guidance uses labelled paragraphs separated by blank lines: `Controls:`, `Impact:`, and `Typical:`.
- [ ] Existing serialized names, code defaults, `[Min]` and `[Range]` attributes, public config interface, asset values, and runtime steering behavior are unchanged.
- [ ] No new asmdef, package, custom inspector, editor code, asset, or scene change is introduced.

## Verification

- EditMode tests: Not added; no behavior, validator, default, or serialization contract changes are expected.
- PlayMode tests: Not added; no runtime **Run Steering Control** behavior changes are expected.
- Static checks: Reformat the edited C# file, inspect Rider/file problems, and run Unity compile through Unity AI Agent Connector.
- Manual Unity smoke check: Open the **Run Steering Control** config asset in the Inspector and spot-check tooltip readability, blank-line spacing, terminology consistency, and absence of stale copied asset values.
- Package version/changelog: Not required for this metadata-only project change.

## Blocked by

None - can start immediately
