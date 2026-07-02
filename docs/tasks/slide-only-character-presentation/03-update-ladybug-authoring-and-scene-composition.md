## Parent

[Slide-Only Character Presentation PRD](../../prd/prd-slide-only-character-presentation.md)

## What to build

Apply the clean authoring update for slide-only Character Presentation across the project-owned **Ladybug Character** and Gameplay Scene composition.

This slice should rename presentation tuning in place so authoring vocabulary matches the new behavior. The old run-forward threshold becomes **Meaningful Grounded Movement Threshold**. **Slide Reference Speed** remains the normal grounded locomotion playback reference. Old slope threshold tuning should no longer appear as canonical mode-selection tuning. If any slope values are retained for a future pass, they must be clearly named as unwired **Slide Flavor Tuning**.

The change intentionally does not use `FormerlySerializedAs` or a migration shim. Current project scenes, prefabs, tests, and serialized assets must be updated in the same slice.

## Acceptance criteria

- [ ] Character presentation tuning exposes **Meaningful Grounded Movement Threshold** language for grounded Slide eligibility.
- [ ] Normal grounded locomotion authoring exposes **Slide Reference Speed** and does not require **Run Reference Speed**.
- [ ] Old run-forward tuning names are removed from normal presentation authoring.
- [ ] Old slope threshold names are removed from canonical mode-selection authoring, or retained only as explicitly unwired **Slide Flavor Tuning**.
- [ ] No `FormerlySerializedAs` attribute or migration shim is introduced.
- [ ] Current project-owned **Ladybug Character** prefab stores the new tuning fields and intended numeric seed values.
- [ ] Gameplay Scene composition resolves the updated tuning through the existing VContainer/view path.
- [ ] Animator mode integer values remain stable and **Run** remains reserved.
- [ ] Character view remains passive and does not classify gameplay state.
- [ ] Root motion remains disabled or guarded as before.
- [ ] Existing **Character Visual Anchor**, **Launch Target**, **Band Center**, Rigidbody, collider, and camera ownership remain unchanged.
- [ ] No package, Unity version, Addressables, save-format, slingshot launch, terrain friction, run progress, or run-end change is introduced in this slice.

## Verification

- EditMode tests:
  - Tuning fakes and presenter/classifier tests use **Meaningful Grounded Movement Threshold** terminology where the public/internal contract requires it.
  - View default tuning tests assert the new names and intended default numeric values.
  - Existing root-motion guard tests remain green.
- PlayMode tests:
  - Gameplay Scene composition asserts resolved character presentation tuning uses **Meaningful Grounded Movement Threshold**.
  - Gameplay Scene composition asserts **Slide Reference Speed** is the normal locomotion reference.
  - Gameplay Scene composition asserts old run-forward and old slope-selection tuning are not required for canonical mode selection.
  - Existing composition checks for Character Visual Anchor, Launch Target Rigidbody ownership, collider separation, Band Center separation, Animator references, and root-motion safety remain green.
- Static checks:
  - Rider reformat and file-problem checks for changed C# files.
  - Unity compile through the connector before tests.
  - `git diff --check`.
  - Review serialized YAML diffs for unintended scene or prefab churn.
- Manual Unity smoke check:
  - Open the Gameplay Scene and confirm the Ladybug Character view exposes the new tuning vocabulary in the Inspector.
- Package version/changelog:
  - No package manifest or changelog update expected.

## Blocked by

- 01 - Replace Grounded Mode Selection With Meaningful Slide
- 02 - Normalize Reserved Run And Slide Playback
