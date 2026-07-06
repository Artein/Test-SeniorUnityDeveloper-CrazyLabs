# PRD: Character Collider Split and Animated Contact Sensors

## Problem Statement

The current player contact model is overloaded. One character-adjacent collider path is expected to support slingshot launch targeting, band contacts, run movement support, finish/safety/run-ending contacts, obstacle impact, and pickup collection. That worked while the character shape was visually close to the gameplay body, but it breaks down once the **Character** changes animation shape during a **Run**.

The most visible example is **Slide**: the visible **Character** can become shorter and longer, while the gameplay collider remains one generic shape. If we make that one collider follow animation, movement and run-ending authority become unstable. If we keep it stable, pickup collection cannot accurately follow hands, head, or other animated body parts.

The enterprise-grade requirement is not "make one better compound collider." It is to split collider responsibilities by gameplay authority:

- **Launch Target Collider Root** remains responsible for launch target and band-contact behavior.
- **Run Body Contact Collider** remains responsible for movement support and default run-ending contact authority during **Running**.
- **Animated Contact Sensors** report body-part-specific trigger contacts by copying finalized **Character** body-part poses into a gameplay-owned physics root.
- Pickup collection consumes typed **Pickup Contact** reports from a **Pickup Sensor Source**, not raw Unity tags or arbitrary character hierarchy notifiers.

The existing pickup implementation is a migration risk. It currently subscribes to `Pickup` trigger events and filters the contacted collider by **Player Tag**. Sensor-driven pickups invert that direction: pickup-eligible **Animated Contact Sensors** detect trigger enters, resolve the touched **Pickup**, and publish lightweight **Pickup Contact** facts to **Pickup Collection Controller**.

## Solution

Split the player contact architecture into three explicit contact roles.

1. **Launch Target Collider Root**

Keep the launch/band collider surface separate from run movement and animated body-part contacts. Its shape and hierarchy can remain whatever the slingshot/band path already needs. This PRD does not migrate band contact logic onto the run body or animated sensors.

2. **Run Body Contact Collider**

Create a dedicated **Run Body Contact Collider** under the **Run Body** Rigidbody root. First implementation uses a single `SphereCollider`, with no animation-driven resizing, no switches, and no compound movement shape. This collider owns movement support and the default run-ending path for **Run Surface**, **Run Finish**, **Run Safety Net**, and normal **Obstacle Impact**.

3. **Animated Contact Sensors**

Create a gameplay-owned **Animated Contact Sensor Physics Root** with one kinematic Rigidbody and trigger-only body-part sensor colliders. The root is a sibling/gameplay-side object, not a child of the **Run Body** Rigidbody root and not under **Character Visual Anchor**. **Animated Contact Sensor Pose Sync** copies finalized **Character** body-part transforms into those sensor transforms after presentation/animation has produced the rendered-frame pose.

First implementation uses these sensors for pickup collection only. Body-part **Run Obstacle** notices are designed as a future extension through a separate **Run Obstacle Sensor Source**, not mixed into **Pickup Sensor Source**. **Run Finish**, **Run Safety Net**, and core run end stay on **Run Body Contact Collider**.

Pickup collection changes from legacy player-tag filtering to source-driven contacts:

- **Pickup Sensor Source** owns explicit pickup-eligible sensor entries.
- Each entry references a `TriggerNotifier` on a copied **Animated Contact Sensor**.
- The source resolves contacted colliders to **Pickup** with parent lookup in the first implementation.
- The source emits **Pickup Contact** reports containing **Pickup**, **Animated Sensor Identity**, sensor/source reference, and contacted Collider.
- **Pickup Collection Controller** owns **Gameplay State** gating, collection outcome, rewards, and idempotency through **Level Pickup State**.

Scene pickup composition moves out of the broad lifetime scope and into **Gameplay Pickups Scene Composition Installer**, mirroring the existing scene composition pattern used by gameplay physics.

## Unity Surfaces

Runtime surfaces:

- **Gameplay** composition and run-contact runtime.
- **Character Presentation** as the source of finalized visual/body-part poses, without making **Smoothed Character Pose** general gameplay truth.
- **Pickups** runtime for **Pickup Contact**, **Pickup Sensor Source**, setup validation, and the collection controller migration.
- **Foundation Physics** `TriggerNotifier` as the low-level trigger adapter.
- VContainer entry-point ordering for **Character Visual Follower** and **Animated Contact Sensor Pose Sync**.

Scene and prefab surfaces:

- Player hierarchy: distinct **Launch Target Collider Root**, **Run Body Contact Collider Root**, **Animated Contact Sensor Physics Root**, and **Character Visual Anchor** roles.
- Player physics authoring: one **Run Body Contact Collider** `SphereCollider` under the **Run Body** Rigidbody root.
- Animated sensor authoring: copied-pose trigger colliders under **Animated Contact Sensor Physics Root**, each with a `TriggerNotifier` and a same-GameObject enabled trigger Collider.
- Pickup authoring: explicit **Pickup** references in **Gameplay Pickups Scene Composition Installer**; each explicit **Pickup** has exactly one enabled trigger Collider under its hierarchy on **Pickup Layer**.
- Project settings: `PlayerBodyPart` layer, **Pickup Layer**, **Player Layer**, and the Unity Layer Collision Matrix.

Editor and validation surfaces:

- Setup validation for explicit pickups, pickup sensor entries, layer names, active/enabled components, trigger settings, duplicate references, and conditional no-pickup levels.
- Optional future editor helper for pickup auto-discovery, explicitly not part of first-pass runtime composition.

No new Unity package, Addressables schema, save format, or public persistence format is required by this PRD.

## User Stories

1. As a player, when the character slides and visibly changes shape, movement stays stable and does not inherit visual animation jitter.
2. As a player, when an animated hand touches a pickup, the pickup can be collected even if the stable run body sphere did not touch it.
3. As a player, when the run body reaches the finish, the run ends through the stable run body path, not through a visual hand sensor.
4. As a player, when the run body falls into a safety net, the run ends through the stable run body path.
5. As a player, when the run body collides with a normal obstacle, the default obstacle impact path remains stable.
6. As a designer, I can tune the run movement contact shape without changing body-part pickup reach.
7. As a designer, I can author pickup-eligible body-part sensors explicitly instead of relying on all character child triggers.
8. As a designer, I can create a level with no pickups and leave pickup composition as an empty no-op.
9. As a designer, I can keep shared player sensor authoring in a no-pickup level without validation noise.
10. As a designer, I get validation errors when a pickup-bearing level has pickups but no pickup sensor source entries.
11. As a designer, I get validation errors when a pickup-bearing sensor is inactive, disabled, missing a trigger Collider, or on the wrong layer.
12. As a designer, I get validation errors when an explicit pickup is missing, duplicated, inactive, or has invalid pickup-layer trigger collider authoring.
13. As a designer, I can use any Unity Collider subtype for the single pickup trigger volume.
14. As a designer, I do not need to put a Rigidbody on every pickup just so trigger events fire.
15. As a designer, I do not need to tag animated hand/head/body-part sensors as Player.
16. As a programmer, I can reason about launch/band contacts, run movement contacts, and animated body-part trigger contacts independently.
17. As a programmer, I can keep `TriggerNotifier` as a dumb adapter with no gameplay meaning.
18. As a programmer, I can subscribe to typed **Pickup Contact** reports instead of raw sensor trigger events.
19. As a programmer, I can keep **Pickup Sensor Source** state-agnostic and let **Pickup Collection Controller** decide when collection is legal.
20. As a programmer, I can allow multiple body-part sensors to report the same pickup without duplicating rewards.
21. As a programmer, I can preserve **Level Pickup State** as the idempotency boundary for collected pickups.
22. As a programmer, I can add future body-part obstacle notices through **Run Obstacle Sensor Source** without mixing obstacle and pickup resolution.
23. As a programmer, I can keep **Run Finish** and **Run Safety Net** outside animated sensor logic.
24. As a programmer, I can validate `PlayerBodyPart` to **Pickup Layer** collision matrix only when the level actually has explicit pickups.
25. As a programmer, I can order **Animated Contact Sensor Pose Sync** after **Character Visual Follower** without moving physics movement into presentation.
26. As a programmer, I can avoid default per-frame `Physics.SyncTransforms` and accept next-physics-step trigger delivery for the first implementation.
27. As a programmer, I can add an explicit same-frame query policy later if measured gameplay requires it.
28. As a programmer, I can treat Animator `Animate Physics` as a separate authoring/timing policy instead of silently relying on render-frame sync.
29. As QA, I can test pickup collection through body-part sensors separately from run movement support.
30. As QA, I can test no-pickup levels without requiring fake pickup sensors or fake pickup references.
31. As QA, I can test pickup-bearing validation failures without depending on scene-wide discovery order.
32. As QA, I can test that pickup collection ignores non-pickup trigger contacts without logging runtime warnings.
33. As QA, I can test that duplicate sensor contacts do not create duplicate **Pickup Collection Events**.
34. As QA, I can test that the run body sphere still supports run-surface probing through the existing support probe factory.
35. As a future content author, I can replace hierarchy-path **Animated Sensor Identity** with asset-backed IDs before sensor identity becomes a stable content contract.

## Implementation Decisions

- Keep **Launch Target Collider Root** unchanged for band/launch behavior. Do not make it the run movement collider or the pickup collector.
- Represent **Run Body Contact Collider** first as one `SphereCollider` under the **Run Body** Rigidbody root. The existing run-support probe factory already supports sphere colliders, so this is mostly scene composition and reference migration.
- **Run Body Contact Collider** owns movement support and default run-ending contact authority for **Run Surface**, **Run Finish**, **Run Safety Net**, and normal **Obstacle Impact**.
- Do not resize or switch the **Run Body Contact Collider** from animation state in the first implementation.
- Host copied body-part trigger colliders under **Animated Contact Sensor Physics Root**, with one kinematic Rigidbody on the root for trigger delivery.
- Do not add Rigidbodies to individual body-part sensors, animated bones, **Character Visual Anchor**, or pickups for the first sensor-driven pickup path.
- Implement **Animated Contact Sensor Pose Sync** as an explicit VContainer `ILateTickable` registered after **Character Visual Follower**.
- **Animated Contact Sensor Pose Sync** copies finalized **Character** body-part poses into copied sensor transforms and lets the next Unity physics simulation deliver trigger events.
- Do not call `Physics.SyncTransforms` every rendered frame by default. Same-frame physics queries require a separate measured policy.
- Treat Animator update mode as an authoring contract. First implementation assumes normal presentation timing; `Animate Physics` needs explicit policy before being supported as equivalent.
- Keep raw `TriggerNotifier` events private to source components. Gameplay consumers receive typed source events.
- Introduce **Pickup Contact** as the source-level raw contact fact. It is not an accepted collection result and does not carry reward data.
- Introduce **Pickup Sensor Source** as the aggregate source for pickup-eligible **Animated Contact Sensors**.
- Configure **Pickup Sensor Source** with explicit serialized `TriggerNotifier` entries. Do not scan the entire **Character Visual Anchor** or all child notifiers at runtime.
- Derive first-pass **Animated Sensor Identity** from sensor GameObject name or hierarchy path. Add a TODO for asset-backed sensor IDs before identity must survive model or hierarchy renames.
- Resolve contacted pickup colliders through parent lookup to **Pickup** in the first implementation. Do not add a pickup contact adapter until pickup contact colliders need to live outside the **Pickup** hierarchy.
- Ignore non-pickup trigger contacts in **Pickup Sensor Source** without warning by default. Setup validation owns missing and duplicate source-entry problems.
- Allow **Pickup Sensor Source** to emit multiple **Pickup Contact** facts for the same **Pickup**. **Pickup Collection Controller** and **Level Pickup State** own collection idempotency.
- Move pickup scene references and source wiring into **Gameplay Pickups Scene Composition Installer**. The lifetime scope should consume installer registrations, not own pickup arrays or player pickup contact collider arrays.
- Keep **Pickup Collection Controller** responsible for **Gameplay State** gating, reward grant, pickup availability changes, and **Pickup Collection Event** emission.
- Keep no-pickup levels composed with empty/no-op pickup sources so notification boundaries remain available.
- Validate explicit pickup references as the first-pass runtime source of truth. Broad runtime pickup discovery is out of the runtime path.
- For pickup-bearing levels, require at least one valid pickup sensor entry and require `PlayerBodyPart` to interact with **Pickup Layer** in the Unity Layer Collision Matrix.
- For no-pickup levels, tolerate empty pickup lists and inactive/disabled shared pickup sensor authoring without warning.
- For explicit pickups, require exactly one enabled trigger Collider under the pickup hierarchy on **Pickup Layer**. Count only Colliders on **Pickup Layer** for this rule.
- Do not require pickup-side `TriggerNotifier` or pickup-side Rigidbody for sensor-driven pickup collection. Existing pickup-side trigger notifier authoring is legacy compatibility, not the new collection source.
- Keep **Animated Contact Sensors** on `PlayerBodyPart`, not **Player Layer**, and do not require **Player Tag**.
- Disable **Player Layer** interaction with `PlayerBodyPart` by default so animated sensors do not self-trigger against **Run Body Contact Collider** or **Launch Target Collider Root**.
- First-pass `PlayerBodyPart` external interaction is **Pickup Layer** only. Do not add `CameraObstacle` interaction until body-part obstacle notices become an explicit feature.
- Model body-part obstacle reporting as a future **Run Obstacle Sensor Source** that reuses collider-local **Run Contact Category** authoring and exact-collider lookup. It should be separate from **Pickup Sensor Source** and state-agnostic like the pickup source.
- Keep **Run Obstacle Sensor Source** out of first-pass implementation unless the feature is explicitly pulled into scope.

## Testing Decisions

- Use EditMode tests first for pure source, controller, validator, and composition behavior.
- Add **Pickup Sensor Source** tests for null entries, duplicate entries, disabled/inactive entries, wrong layer, missing trigger Collider, non-pickup contacts ignored, pickup parent resolution, and typed **Pickup Contact** payload.
- Add **Pickup Collection Controller** tests proving collection consumes **Pickup Contact** reports, respects **Gameplay State**, grants once, and ignores duplicate contacts for already consumed pickups.
- Add **Level Pickup State** coverage only if migration changes its current source assumptions; otherwise keep existing idempotency tests.
- Add **Gameplay Pickups Scene Composition Installer** tests for explicit pickup registration, empty pickup lists, duplicate/null/inactive pickup references, required source entries for pickup-bearing levels, and no-op composition for no-pickup levels.
- Add validation tests for exactly one pickup-layer trigger Collider under each explicit **Pickup** hierarchy, allowing any Collider subtype and ignoring non-**Pickup Layer** colliders.
- Add layer-matrix validation tests proving `PlayerBodyPart` to **Pickup Layer** is required only when explicit pickups exist.
- Add entry-point order tests proving **Animated Contact Sensor Pose Sync** runs after **Character Visual Follower**.
- Add pose-sync EditMode tests for transform-copy math where possible, using explicit test hooks rather than reflection.
- Use PlayMode tests for Unity physics trigger delivery between copied body-part sensors and pickup triggers, because Rigidbody/trigger behavior is engine behavior.
- Avoid time-based waits. Use condition-with-timeout helpers for PlayMode physics checks.
- Keep test assets behind typed test asset providers when scene or prefab assets are required.
- After code changes, run Unity compile through `unity-ai-agent-connector` before running tests. Run targeted EditMode/PlayMode tests only after compile is clean.

## Release and Compatibility

- Target Unity editor version is 6000.3.18f1.
- This PRD does not require package upgrades, Addressables changes, save format changes, or persistence migrations.
- Scene/prefab migration is required for the player hierarchy: add **Run Body Contact Collider Root**, add **Animated Contact Sensor Physics Root**, configure body-part trigger sensors, and wire **Pickup Sensor Source**.
- Scene composition migration is required for pickups: move explicit pickup references and pickup contact-source wiring to **Gameplay Pickups Scene Composition Installer**.
- The existing pickup PRD and implementation describe a legacy path based on pickup-side `TriggerNotifier` events and **Player Tag** filtering. This work supersedes that contact path for sensor-driven collection.
- Existing pickup definitions, currency grants, reward accumulation, and collection event semantics should remain compatible.
- Existing pickup-side Rigidbodies may remain harmless during migration, but they are no longer required by the sensor-driven pickup contract.
- Existing pickup-side `TriggerNotifier` authoring may remain for compatibility during migration, but collection should move to **Pickup Sensor Source** reports.
- Because this changes scene authoring contracts and physics layer policy, release should include explicit prefab/scene validation before gameplay QA.

## Out of Scope

- Full compound movement collider for **Run Body Contact Collider**.
- Animation-driven resizing or switching of the movement collider.
- Pickup auto-discovery as a runtime composition source of truth.
- Asset-backed **Animated Sensor Identity** implementation.
- Same-frame physics queries from pose sync.
- Default per-frame `Physics.SyncTransforms`.
- Animator `Animate Physics` support without an explicit timing policy.
- Body-part **Run Obstacle** notices in the first implementation.
- Converting body-part obstacle notices into **Obstacle Impact** or run end.
- **Run Finish** or **Run Safety Net** through animated sensors.
- Pickup-side Rigidbody requirement.
- Pickup-side `TriggerNotifier` requirement for new sensor-driven collection.
- Camera obstacle layer interaction for `PlayerBodyPart` in the first implementation.
- Visual/VFX/audio changes for collected pickups.

## Further Notes

The resolved architecture is a good ADR candidate before implementation. It changes collider authority, physics-body ownership, scene authoring, layer policy, and pickup contact event direction. Those decisions are costly to reverse and easy for future work to accidentally blur.

Implementation should be sliced vertically:

1. Introduce typed pickup contact source abstractions and migrate **Pickup Collection Controller** tests.
2. Add **Pickup Sensor Source** and setup validation.
3. Add **Gameplay Pickups Scene Composition Installer** and move pickup references out of the lifetime scope.
4. Add **Animated Contact Sensor Physics Root** and **Animated Contact Sensor Pose Sync**.
5. Migrate the scene/prefab hierarchy and physics layers.
6. Add PlayMode physics integration coverage.

The main risk is update ordering. The intended first-pass policy is render-frame pose copy after **Character Visual Follower**, then next-physics-step trigger delivery. That is acceptable for pickup collection, but not a general promise for same-frame gameplay queries.
