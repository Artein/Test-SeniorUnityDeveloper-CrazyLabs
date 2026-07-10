# Migrate Existing Movement Tuning to a Validated Config

Type: AFK

## Parent

[PRD: Run Body Explicit Speed Ownership](../../prd/prd-run-body-explicit-speed-ownership.md)

## What to build

Introduce one designer-facing **Run Body Movement Tuning** asset as the source of raw movement settings. Migrate the existing steering, steering-frame, movement-validity, launch-landing, defensive velocity, and new speed groups without changing their current behavior or authored values.

The asset must expose narrow read-only config interfaces so each runtime collaborator receives only the values it owns. Register those interfaces from the same asset through gameplay composition. The asset stores raw values only; cross-field resolution and active-run stat application remain gameplay logic.

Give designers immediate, readable authoring feedback. Use non-negative or positive Inspector bounds where the constraint is unconditional, use ranges only for genuine design limits, and describe behavior and units in tooltips. A shared pure validator must report non-finite values and all cross-field violations in the Editor, then enforce the same invariants before movement begins. Production fixed-step logic may assume validated authoring.

Keep a temporary compatibility adapter only where needed to migrate existing consumers safely. The Gameplay scene must reference the replacement asset before the broad legacy config or adapter can be removed.

## Acceptance criteria

- [ ] One **Run Body Movement Tuning** ScriptableObject groups speed, movement-validity, launch-landing, steering, steering-frame, and defensive sanity settings in visibly distinct sections.
- [ ] Existing authored steering, steering-frame, movement-validity, launch-landing, and sanity-guard values survive migration without changing player behavior.
- [ ] Speed tuning exposes raw downhill acceleration, ordinary surface slowdown, low-speed assist target, low-speed assist acceleration, base soft maximum, and above-maximum resistance values.
- [ ] The single asset implements and is registered through narrow read-only interfaces for each implemented consumer.
- [ ] Runtime services depend on narrow interfaces, not the concrete asset or a broad all-settings interface.
- [ ] The asset returns authored values without silently resolving, clamping, defaulting, or repairing cross-field combinations.
- [ ] Inspector labels and tooltips use designer-facing behavior and units; field names do not expose squared-unit implementation details.
- [ ] Min attributes enforce unconditional lower bounds, and Range attributes appear only where a real upper design limit exists.
- [ ] A pure validator rejects non-finite values, invalid signs, required zero values, and defined cross-field violations while accepting valid raw combinations that gameplay will resolve later.
- [ ] Editor validation reports all detected errors, not only the first failure.
- [ ] Composition or movement initialization fails before fixed-step evaluation when authored config is invalid.
- [ ] The **Run Body Speed Sanity Guard** remains explicitly defensive and far above normal launch, upgrade, and course speeds.
- [ ] No surface-profile, terrain-material inference, package, ProjectSettings, or save-format behavior is introduced.

## Verification

- EditMode tests:
  - Valid assets expose unchanged raw values through every narrow interface.
  - The pure validator accepts valid values and reports all non-finite, sign, zero, and cross-field violations.
  - A raw low-speed assist target above the base soft maximum remains valid authoring rather than being repaired by config.
  - Inspector-compatible bounds and validator invariants agree.
  - Invalid config fails during pre-use validation.
- PlayMode tests:
  - Gameplay composition resolves the same movement-tuning asset through every implemented narrow interface.
  - The Gameplay scene references the replacement asset and does not lose migrated settings.
- Static checks:
  - Rider reformat and problem inspection for changed code and tests.
  - Unity connector compile before tests.
  - Confirm no new runtime assembly, package dependency, ProjectSettings, Addressables, or save-data change.
- Manual Unity smoke check:
  - Inspect the asset groups, labels, tooltips, bounds, and complete validation feedback.
  - Confirm existing launch, landing, and steering behavior remains unchanged after migration.
- Package version/changelog:
  - Not required; this is project gameplay configuration, not a UPM package release.

## Blocked by

None - can start immediately.
