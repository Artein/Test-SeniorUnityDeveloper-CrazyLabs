# Coin Pickup Visuals And Spinner PRD

## Problem Statement

Coin Pickups currently do not have the intended coin presentation for a casual mobile runner. They use a temporary imported Ladybug mesh/material setup and remain visually static during the Run. This makes collectible currency less readable, less polished, and more tightly coupled to third-party asset authoring than the game should be.

The team also needs Coin Pickup presentation to stay separate from Pickup collection and reward logic. A spinning mesh must not change trigger behavior, Pickup Definitions, Currency Grants, Level Pickup State, Run Reward Breakdown, or any economy rule. Designers should be able to improve Coin Pickup Visuals without risking gameplay behavior.

## Solution

Introduce a project-owned Coin Pickup Visual setup for Regular Coin Pickups and Big Coin Pickups. The visual uses a proper coin mesh, a project-owned URP material tuned to match the coin currency icon, and a reusable generic Spinner component to rotate the visual at runtime.

Spinner is a generic presentation component, not a pickup system. It rotates a target transform using scaled game time, applies deterministic per-instance initial phase variation so scene coins do not all align to the same degree, and supports an authored initial phase override for special cases. Coin Pickup prefabs use Spinner only on the visual transform, leaving the Pickup root, trigger collider, layer, tag, Pickup Definition, and reward flow untouched.

Big Coin Pickups reuse the same Coin Pickup Visual language. Their higher value remains owned by Pickup Definition, and their larger appearance is expressed by visual scale rather than by implying a different currency or collection rule.

## Unity Surfaces

- Runtime assemblies and asmdefs
  - Foundation runtime assembly gains a small presentation module for generic transform spinning.
  - Pickup runtime assembly continues to own Pickup, Pickup Definition, Pickup Collection Controller, Level Pickup State, and pickup authoring contracts.
  - Gameplay and Economy assemblies should not depend on the new Spinner beyond ordinary prefab/component usage.

- Editor assemblies, windows, inspectors, importers, or menu items
  - No custom editor window is required.
  - Existing prefab/scene validation can be extended if needed.
  - No importer automation is required for the first implementation; imported mesh setup can be performed through Unity asset import settings.

- Scenes, prefabs, ScriptableObjects, package manifests, or ProjectSettings
  - Coin Pickup prefabs are updated to reference project-owned coin visual assets.
  - Regular Coin Pickup and Big Coin Pickup variants continue to reference their existing Pickup Definitions.
  - The Pickup base prefab remains the root collection contract: Pickup component, Pickup Layer, and trigger collider live on the root.
  - The visual child owns MeshFilter, MeshRenderer, material, Spinner, and visual scale.
  - Coin visual material uses a URP-compatible shader and does not depend on unsupported imported model materials.
  - Existing authored level pickup instances should continue resolving through their prefab variants.
  - The currency icon remains the visual palette reference for the coin material.

- RPC/helper commands, hooks, or shell wrappers
  - No new helper command is required.
  - Unity compile and targeted test execution remain the verification path.

- Package versioning, changelog, and installation/sync behavior
  - This is a project feature change, not a package installation change.
  - No save format, catalog, Addressables schema, or remote sync behavior changes are expected.

## User Stories

1. As a player, I want coins to look like intentional collectible currency, so that I immediately understand what to collect during a Run.
2. As a player, I want Coin Pickups to rotate, so that they are easier to notice while sliding downhill.
3. As a player, I want nearby coins to have slightly different visual phases, so that coin lines feel alive instead of mechanically duplicated.
4. As a player, I want Regular Coin Pickups and Big Coin Pickups to feel visually related, so that I understand they are the same currency family.
5. As a player, I want Big Coin Pickups to look larger, so that their higher value is readable without needing UI text during the Run.
6. As a player, I want coin visuals to match the run-end and HUD coin icon, so that currency meaning stays consistent.
7. As a designer, I want Coin Pickup Visuals to be separate from Pickup Definitions, so that I can tune visuals without changing reward amounts.
8. As a designer, I want Big Coin Pickup value to remain authored through Pickup Definition, so that reward balance stays in economy data.
9. As a designer, I want Big Coin Pickup size to be a visual setting, so that collider feel can be tuned independently.
10. As a designer, I want the Pickup root to stay stable, so that moving or spinning the visual does not affect trigger contacts.
11. As a designer, I want all coin prefabs to use project-owned visual assets, so that gameplay content is not tied directly to plugin model materials.
12. As a designer, I want the coin material to be URP-compatible, so that Play Mode does not show missing shader magenta.
13. As a designer, I want a single canonical coin material for the first slice, so that color tuning can happen in one place.
14. As a designer, I want visual phase variation by default, so that I do not have to hand-rotate every placed coin.
15. As a designer, I want an authored initial phase override, so that special showcase coins can be aligned deliberately.
16. As a designer, I want visual scale to be authored on the visual child, so that the pickup collider remains readable and explicit.
17. As a gameplay programmer, I want Spinner to be generic, so that it can rotate any presentation transform without depending on Pickups.
18. As a gameplay programmer, I want Spinner to live in Foundation presentation code, so that feature assemblies can reuse it without reverse dependencies.
19. As a gameplay programmer, I want Spinner to use scaled game time, so that cosmetic motion pauses with gameplay time.
20. As a gameplay programmer, I want Spinner to stop naturally when inactive, so that collected or hidden pickups do not need special update handling.
21. As a gameplay programmer, I want Spinner to rotate a target transform, so that prefabs can keep logic and visuals separated.
22. As a gameplay programmer, I want deterministic initial phase generation, so that tests and scene reloads are stable.
23. As a gameplay programmer, I want deterministic phase to avoid Unity runtime randomness, so that prefab behavior is reproducible.
24. As a gameplay programmer, I want deterministic phase to be based on stable instance data, so that coin lines desync without scene scripts.
25. As a gameplay programmer, I want an additive authored phase, so that deterministic variation can be adjusted without disabling the system.
26. As a gameplay programmer, I want Spinner to avoid changing gameplay state, so that visual polish cannot affect pickup collection.
27. As a gameplay programmer, I want Pickup Collection Controller behavior unchanged, so that collection gating remains covered by existing tests.
28. As a gameplay programmer, I want Level Pickup State behavior unchanged, so that one-shot collection rules do not move into prefabs.
29. As a gameplay programmer, I want Run Reward Breakdown behavior unchanged, so that result-screen coin sources remain source-agnostic.
30. As a gameplay programmer, I want Coin Pickup Reward behavior unchanged, so that coin multipliers and fractional carry remain economy rules.
31. As a gameplay programmer, I want imported third-party meshes treated as source material, so that runtime prefabs reference game-owned decisions.
32. As a gameplay programmer, I want no runtime material patching, so that prefab assets own renderer setup.
33. As a gameplay programmer, I want no runtime mesh assignment, so that Play Mode startup does not hide missing authoring.
34. As a gameplay programmer, I want prefab validation to catch missing meshes/materials, so that broken visuals fail close to authoring.
35. As a gameplay programmer, I want prefab validation to preserve trigger collider checks, so that visual work cannot break pickup contact behavior.
36. As a tester, I want Spinner EditMode tests, so that deterministic phase and scaled-time rotation are verified without loading a scene.
37. As a tester, I want tests to avoid exact visual-frame expectations, so that designers can tune spin speed and layout without rewriting tests.
38. As a tester, I want prefab contract checks for coin pickup visuals, so that missing material or mesh assignments are caught.
39. As a tester, I want prefab contract checks for Pickup roots, so that root collider and Pickup Definition contracts stay valid.
40. As a tester, I want tests to avoid asserting a specific mesh silhouette, so that the team can replace the art asset later.
41. As a tester, I want tests to avoid asserting exact scene rotation angles, so that deterministic variation does not become a brittle level-layout test.
42. As a technical artist, I want coin material ownership in the game project, so that shader and color changes do not require editing plugin assets.
43. As a technical artist, I want the material to match the currency icon palette, so that 3D coins and UI coins read as the same currency.
44. As a technical artist, I want renderer setup serialized in prefabs, so that Play Mode shows the same visuals designers see in prefab editing.
45. As a technical artist, I want Big Coin to reuse the same material, so that higher value is shown by size rather than a different currency identity.
46. As a maintainer, I want no new save data fields, so that coin visual migration does not affect player persistence.
47. As a maintainer, I want no new gameplay service API, so that visual polish remains low-risk.
48. As a maintainer, I want no new dependency from Foundation to Gameplay, so that shared presentation utilities stay reusable.
49. As a maintainer, I want tests focused on contracts, so that visual iteration remains designer-owned.
50. As a maintainer, I want the glossary to keep Coin Pickup Visual distinct from Coin Pickup, so that future discussions do not conflate presentation and reward behavior.

## Implementation Decisions

- Define Coin Pickup Visual as the rendered presentation of a Coin Pickup.
- Coin Pickup Visual includes mesh, material, spin, phase, and visual scale.
- Coin Pickup Visual does not own collection, availability, Pickup Definition, Currency Grant, Level Pickup State, or reward amount.
- Add a generic Spinner component rather than a coin-specific spinning component.
- Spinner belongs to the Foundation presentation layer because it is reusable visual behavior and does not depend on gameplay concepts.
- Spinner rotates a configured target transform; if no target is assigned, it may rotate its own transform.
- Coin Pickup prefabs use Spinner on the visual child or target the visual child.
- Spinner must never rotate the Pickup root for Coin Pickups.
- The Pickup root remains responsible for Pickup, Pickup Layer, trigger collider, and scene-authoring identity.
- Spinner uses scaled game time.
- Spinner naturally stops when its GameObject is inactive.
- Spinner applies deterministic per-instance initial phase by default to prevent all coins from sharing the same visible rotation.
- Deterministic phase should not use runtime random values or Unity instance IDs that change unpredictably between runs.
- Spinner supports an authored phase override or additive authored offset for cases where designers need a precise visible angle.
- The first implementation should keep Spinner small: axis, speed, target, deterministic phase mode, and authored phase settings are enough.
- Use a project-owned coin mesh and material for runtime Coin Pickup prefabs.
- Treat downloaded or third-party meshes as source material, not as runtime gameplay prefab references.
- Use a URP-compatible material for the coin visual.
- Tune the material against the existing coin currency icon palette.
- Do not keep relying on unsupported imported model materials for runtime Coin Pickup visuals.
- Avoid runtime mesh assignment and runtime material patching; prefab authoring owns visual references.
- Regular Coin Pickup and Big Coin Pickup use the same canonical coin mesh/material/spinner setup.
- Big Coin Pickup remains a higher-value Coin Pickup through its Pickup Definition.
- Big Coin Pickup presentation is expressed by visual scale, not by a separate currency or special collection rule.
- Move Big Coin size away from root scale if needed so the collider and visual size remain independently authored.
- Existing Pickup Collection Controller, Pickup Definition, Level Pickup State, Run Currency Accumulator, Run Reward Breakdown, and Run Ended UI reward source behavior remain unchanged.
- No new gameplay service API is needed.
- No new VContainer registration is needed for Spinner.
- No custom editor or runtime manager is needed for the first implementation.

## Testing Decisions

- Good tests protect behavior and authoring contracts, not exact art.
- Do test Spinner as a deep, reusable module in EditMode.
- Spinner tests should verify rotation progresses with scaled delta time through an explicit tickable/test hook or a deterministic test surface.
- Spinner tests should verify deterministic phase is stable for the same configured instance inputs.
- Spinner tests should verify authored phase settings affect the initial visual rotation as intended.
- Spinner tests should verify inactive behavior through Unity lifecycle only if implementation needs PlayMode; otherwise keep coverage in EditMode.
- Do test Coin Pickup prefab contracts.
- Prefab contract checks should verify Coin Pickup prefabs have valid Pickup roots, trigger colliders, Pickup Definitions, visual renderers, non-null meshes, and non-null URP-compatible materials.
- Prefab contract checks should verify Big Coin visual scale does not require changing the meaning of Big Coin Pickup.
- Prefab contract checks should verify visual motion components are on or target the visual child, not the Pickup root.
- Do not test exact mesh topology, exact mesh name, exact renderer count beyond necessary authoring contracts, exact spin speed, exact phase values in scene, or pixel-perfect screenshots.
- Existing Pickup collection tests remain the prior art for gameplay behavior and should not need broad rewrites.
- Existing PlayMode scene composition tests may add loose checks only if they already validate pickup prefab authoring in the loaded scene.
- Manual Unity smoke validation remains important for visual readability: coin color, rotation axis, size, visibility on the run course, and Big Coin readability.
- Run Unity compile before running tests after implementation.
- Run targeted EditMode tests for Foundation Spinner and Pickup prefab validation.
- Run targeted Pickup PlayMode tests only if prefab or scene changes affect trigger/collider authoring.
- Run targeted GameplayScene composition tests if scene-authored pickup instances or provider references are changed.

## Release and Compatibility

- Assumes the current Unity 6 project and URP rendering setup.
- No player save migration is expected.
- No Economy, Upgrade, Pickup Definition, Run Reward Breakdown, or Run Ended UI data migration is expected.
- Existing scene pickup instances should keep working through prefab variant updates, but prefab overrides must be reviewed so old plugin mesh/material references are removed.
- Replacing imported material references with project-owned URP material reduces missing-shader risk.
- Moving Big Coin scale from root to visual child can affect apparent collider size if not reviewed; collider radius should remain intentionally authored.
- If a new mesh asset is imported from external downloads, licensing and attribution must be checked before committing.
- Git LFS rules already cover binary model and image source assets; Unity YAML assets should remain normal text assets unless a specific exception is required.
- No package manifest or dependency addition is expected.

## Out of Scope

- Pickup magnet behavior.
- Pickup pooling.
- Coin collection VFX.
- Coin collection SFX.
- Runtime reward logic changes.
- Coin multiplier changes.
- Run Ended UI reward source changes.
- New currency types.
- New Pickup Definitions.
- Addressables setup.
- Automated mesh import pipeline.
- Pixel-perfect visual screenshot tests.
- Exact designer-owned coin placement, counts, or level pacing.

## Further Notes

- The key implementation boundary is that Coin Pickup Visual is presentation, while Coin Pickup is collection authoring.
- The likely deep module is Spinner: small public surface, deterministic behavior, and reusable outside Pickups.
- The likely shallow surfaces are coin pickup prefabs and project-owned mesh/material assets.
- The current prefab setup already has a visual child under the Pickup root, which is the right place for mesh, material, visual scale, and Spinner.
- The current Big Coin prefab scales the root, so the implementation should review whether this couples visual size to trigger size more than intended.
- The current coin visual uses imported Ladybug plugin mesh/material references, so the migration should explicitly remove those runtime dependencies from coin pickup prefabs.
