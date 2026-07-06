# Pickups

Pickups cover collectible scene objects, pickup definitions, collection contacts, and per-level pickup state.

## Language

**Pickup**:
The collectible object touched during a **Run** to request a collection.
_Avoid_: Coin visual, reward, currency object

**Pickup Definition**:
The authored collection definition for a **Pickup**, including its **Currency Grant**.
_Avoid_: Coin Definition, visual value, pickup state

**Pickup Collection Controller**:
The gameplay boundary that decides whether a **Pickup** contact becomes an accepted collection.
_Avoid_: Pickup, trigger, Currency Storage

**Pickup Collection Event**:
The accepted collection fact emitted after a **Pickup** is collected.
_Avoid_: Trigger enter, grant preview, balance update

**Level Pickup State**:
The per-**Level Session** record of which **Pickups** have already been collected.
_Avoid_: Save data, pickup component state, scene search

**Pickup Layer**:
The physics layer used by collectible pickup triggers.
_Avoid_: Run Surface layer, player layer, tag

**Player Tag**:
The Unity tag used by the legacy player-collider pickup path to identify configured player pickup contact colliders.
_Avoid_: Pickup Layer, Launch Target term, character name, Animated Contact Sensor

**Pickup Contact**:
The raw contact fact that a pickup-eligible **Animated Contact Sensor** touched a **Pickup**.
_Avoid_: Accepted collection, reward, balance grant

**Pickup Sensor Source**:
The dedicated aggregate source component that exposes configured pickup-eligible **Animated Contact Sensor** reports to **Pickup Collection Controller**.
_Avoid_: Player Tag, every TriggerNotifier, Character hierarchy scan, visual child sensor, pickup controller sensor list

**Gameplay Pickups Scene Composition Installer**:
The scene composition boundary that owns scene-authored pickup references and pickup contact-source wiring for a **Gameplay** scene.
_Avoid_: GameplayLifetimeScope pickup fields, Pickup Collection Controller, reward service, Character hierarchy scan

**Coin Pickup**:
A **Pickup** whose **Pickup Definition** grants coin currency.
_Avoid_: Currency Balance, coin reward text

**Coin Pickup Visual**:
The rendered presentation of a **Coin Pickup**.
_Avoid_: Pickup, Pickup Definition, Currency Grant

**Big Coin Pickup**:
A higher-value **Coin Pickup** variant.
_Avoid_: Different currency, upgrade token

## Relationships

- A **Pickup** has one **Pickup Definition**.
- A **Pickup Definition** has one **Currency Grant**.
- A **Pickup** identifies the collectible touched by a **Pickup Contact**; it does not decide collectability.
- **Pickup Sensor Source** originates **Pickup Contact** by resolving pickup-eligible **Animated Contact Sensor** trigger events to contacted **Pickups**.
- **Pickup Sensor Source** should keep raw `TriggerNotifier.TriggerEntered` subscriptions private and expose typed **Pickup Contact** reports to consumers.
- **Pickup Sensor Source** may resolve the contacted collider to **Pickup** with `GetComponentInParent<Pickup>()` in the first implementation.
- **Pickup Sensor Source** should not require a separate pickup contact adapter until pickup contact colliders need to live outside the **Pickup** hierarchy.
- **Pickup Contact** should preserve enough sensor origin context to debug which configured sensor touched the **Pickup**.
- **Pickup Collection Controller** converts valid **Pickup Contact** into **Pickup Collection Event**.
- **Level Pickup State** prevents the same **Pickup** from being collected twice in one **Level Session**.
- **Pickup Layer** and **Player Tag** are authoring contracts for collection contacts.
- **Player Tag** should identify configured player pickup contact colliders in the legacy pickup path, not animated visual child sensors.
- **Animated Contact Sensor** pickup reports require an explicit collection source or policy; they should not collect pickups by being tagged as the player collider.
- Pickup-eligible **Animated Contact Sensors** should use the `PlayerBodyPart` physics layer, not Player Tag or Player Layer.
- Pickup-eligible **Animated Contact Sensors** may stay untagged in the first implementation.
- Pickup-eligible **Animated Contact Sensors** should not require `PlayerBodyPart` to interact with Player Layer in the Unity Layer Collision Matrix.
- First-pass `PlayerBodyPart` interaction should be pickup-only; pickup-eligible **Animated Contact Sensors** should not require `PlayerBodyPart` to interact with `CameraObstacle`.
- **Pickup Sensor Source** is the configured source of pickup-eligible **Animated Contact Sensors** accepted by **Pickup Collection Controller**.
- **Pickup Sensor Source** owns pickup-eligible sensor configuration through explicit serialized `TriggerNotifier` entries.
- **Pickup Sensor Source** should live on the gameplay-owned Player / **Run Body** side, not under **Character Visual Anchor**.
- **Pickup Sensor Source** should reference pickup-eligible **Animated Contact Sensors** hosted by **Animated Contact Sensor Physics Root**; those sensors copy pose from **Character Visual Anchor** body-part objects.
- First-pass **Pickup Sensor Source** configuration should use explicit serialized `TriggerNotifier` entries, not broad runtime hierarchy scans.
- A pickup sensor entry should reference exactly one `TriggerNotifier` and derive **Animated Sensor Identity** from that notifier's GameObject name or hierarchy path.
- **Pickup Collection Controller** should consume pickup-eligible sensor reports through **Pickup Sensor Source**, not by scanning every raw sensor `TriggerNotifier`.
- **Pickup Collection Controller** should not own the sensor list or discover pickup-eligible sensors by hierarchy scan.
- **Pickup Sensor Source** should not require a separate `AnimatedContactSensor` component for first-pass pickup eligibility.
- **Pickup Sensor Source** owns its raw `TriggerNotifier` subscription lifecycle and setup validation for null or duplicate sensor entries.
- **Pickup Collection Controller** should not subscribe to, unsubscribe from, validate, or deduplicate raw sensor notifiers.
- **Pickup Sensor Source** should treat contacted colliders that do not resolve to **Pickup** as non-matching contacts.
- **Pickup Sensor Source** should emit no **Pickup Contact** and no warning by default for non-pickup contacts.
- Pickup source setup validation should catch invalid configured sensor entries; runtime non-pickup contacts are ordinary filtering.
- **Pickup Sensor Source** should not deduplicate multiple **Pickup Contact** facts for the same **Pickup** across different eligible sensors.
- **Pickup Collection Controller** and **Level Pickup State** own idempotency when multiple sensors touch the same **Pickup**.
- Duplicate **Pickup Contact** facts for an already collected **Pickup** should not create duplicate **Pickup Collection Events**.
- **Pickup Sensor Source** should be state-agnostic contact resolution and should not decide whether the current **Gameplay State** allows collection.
- **Pickup Collection Controller** owns **Gameplay State** gating and decides whether a **Pickup Contact** becomes a **Pickup Collection Event**.
- First-pass **Pickup Sensor Source** should report new trigger enters only after its source subscriptions are active.
- First-pass **Pickup Sensor Source** should not query or replay sensors that are already overlapping **Pickups** when the source becomes enabled.
- Source/component enable order should make **Pickup Sensor Source** active before pickup collection contacts matter.
- First-pass **Pickup Contact** payload should stay lightweight: **Pickup**, **Animated Sensor Identity**, sensor/source reference, and contacted Collider.
- **Pickup Contact** should not carry **Currency Grant**, reward amount, **Gameplay State**, or accepted/rejected collection result.
- **Pickup Collection Controller** and **Level Pickup State** decide collection outcome; **Pickup Definition** owns reward meaning.
- **Pickup Sensor Source** is pickup-only; it should not expose body-part **Run Obstacle** notices.
- Body-part **Run Obstacle** notices should use **Run Obstacle Sensor Source** if that feature is enabled, because they resolve contacted colliders as collider-local **Run Contact Category** metadata, not as **Pickup** aggregates.
- The same **Animated Contact Sensor** may appear in **Pickup Sensor Source** and **Run Obstacle Sensor Source** only if a later feature makes it eligible for both pickup collection and obstacle notices.
- **Pickup Sensor Source** should still apply only pickup target resolution and filtering to that sensor's trigger reports.
- Duplicate registration of one **Animated Contact Sensor** inside **Pickup Sensor Source** should be rejected or flagged to avoid duplicate **Pickup Contact** reports.
- **Pickup Sensor Source** does not make **Animated Contact Sensors** use **Player Tag**.
- **Pickup Sensor Source** decides pickup eligibility from configured sensor entries, not from Unity tags.
- **Pickup Sensor Source** does not make **Animated Contact Sensors** or **Pickups** own trigger-delivery Rigidbodies; **Animated Contact Sensor Physics Root** provides the gameplay-side physics-body participant.
- **Pickup Sensor Source** does not own **Animated Contact Sensor Pose Sync**; it consumes trigger reports from already-copied sensors.
- Sensor-driven pickup collection may receive trigger reports from the physics step after **Animated Contact Sensor Pose Sync** copied the rendered-frame body-part pose.
- Pickup collection should not force same-frame pickup physics queries from **Animated Contact Sensor Pose Sync**; `Physics.SyncTransforms` requires an explicit measured same-frame query policy.
- **Pickup Sensor Source** may use first-pass **Animated Sensor Identity** from GameObject name or hierarchy path, but it should not hardcode body-part enums as pickup rules.
- First-pass **Animated Contact Sensors** are pickup collection sensors only; other contact facts require an explicit non-pickup sensor source.
- **Gameplay Pickups Scene Composition Installer** owns scene-authored **Pickup** references and first-pass **Pickup Sensor Source** wiring.
- **Gameplay Pickups Scene Composition Installer** provides pickup sources to gameplay composition; **GameplayLifetimeScope** should not own serialized level pickup lists or player pickup contact collider lists.
- First-pass **Gameplay Pickups Scene Composition Installer** should use explicit scene-authored **Pickup** references, not broad runtime scene discovery.
- First-pass **Gameplay Pickups Scene Composition Installer** may have an empty explicit **Pickup** reference set for levels authored without pickups.
- No-pickup levels should not require **Pickup Sensor Source** authoring; pickup sensor setup may be omitted or registered as no-op when the explicit **Pickup** set is empty.
- No-pickup levels should keep the **Pickup Collection Controller** and pickup collection notification boundary available through empty/no-op pickup sources, not conditional pickup collection composition.
- No-pickup levels may keep authored **Pickup Sensor Source** entries without warning or failure; shared Player authoring should not be treated as pickup-bearing level content.
- Authored pickup sensor entries alone do not make a level pickup-bearing; the explicit **Pickup** set controls pickup-bearing validation.
- Pickup levels with at least one explicit **Pickup** reference should require and validate **Pickup Sensor Source** authoring.
- Pickup levels with at least one explicit **Pickup** reference should reject an empty **Pickup Sensor Source** or zero configured pickup sensor entries.
- Pickup-bearing validation should reject a configured **Pickup Sensor Source** entry whose `TriggerNotifier` GameObject has no enabled trigger Collider on the `PlayerBodyPart` layer.
- Pickup-bearing validation should not require Player Tag or a Rigidbody on configured **Pickup Sensor Source** sensor entries.
- Pickup-bearing validation should reject configured **Pickup Sensor Source** entries whose `TriggerNotifier` component is disabled or whose sensor GameObject is inactive in hierarchy.
- Disabled or inactive authored **Pickup Sensor Source** entries should be tolerated as quiet no-op shared Player authoring only when the explicit **Pickup** set is empty.
- First-pass `PlayerBodyPart` to **Pickup Layer** collision-matrix validation should run only when the explicit **Pickup** set is non-empty.
- No-pickup levels should not fail pickup physics validation because `PlayerBodyPart` and **Pickup Layer** interaction is absent.
- Pickup auto-discovery may exist later as an editor authoring helper, but runtime pickup composition should consume the explicit pickup set.
- Sensor-driven pickup collection should require exactly one enabled trigger Collider under each explicit **Pickup** hierarchy on **Pickup Layer**.
- The single pickup-side trigger Collider for an explicit **Pickup** should stay under that **Pickup** hierarchy, be enabled, be marked trigger, and be on **Pickup Layer**.
- The single pickup-side trigger Collider for an explicit **Pickup** may be any Unity Collider subtype; pickup collection should not whitelist primitive Collider classes.
- Single-collider pickup validation should count only Colliders on **Pickup Layer** under the explicit **Pickup** hierarchy.
- Colliders under an explicit **Pickup** hierarchy on non-**Pickup Layer** layers should not participate in pickup collection single-collider validation.
- Additional Colliders on **Pickup Layer** under one explicit **Pickup** hierarchy should be invalid first-pass pickup authoring, not merged into one volume.
- Sensor-driven pickup collection should not require pickup-side Rigidbody authoring; each explicit **Pickup** supplies the pickup target trigger Collider while **Animated Contact Sensor Physics Root** supplies trigger delivery.
- Sensor-driven pickup collection should not require a pickup-side `TriggerNotifier`; pickup-side `TriggerNotifier` authoring is legacy compatibility, not required collection authoring.
- **Gameplay Pickups Scene Composition Installer** should validate pickup references, **Pickup Sensor Source** authoring, and first-pass `PlayerBodyPart` to **Pickup Layer** collision-matrix setup.
- First-pass **Gameplay Pickups Scene Composition Installer** validation should reject null, duplicate, or inactive explicit **Pickup** references when references are present.
- Initial pickup unavailability should be represented by an explicit availability policy later, not by disabling a referenced **Pickup** in first-pass scene authoring.
- **Gameplay Pickups Scene Composition Installer** should not own **Pickup Collection Controller**, **Level Pickup State**, reward resolution, **Gameplay State** gating, or pickup idempotency.
- **Coin Pickup** and **Big Coin Pickup** differ by grant amount, not by currency concept.
- A **Coin Pickup** may have one **Coin Pickup Visual**.
- A **Big Coin Pickup** may use a larger **Coin Pickup Visual** without changing the meaning of **Big Coin Pickup**.
- **Coin Pickup Visual** does not determine collection, availability, or reward amount.
- **Coin Pickup Visual** may vary its visual phase between instances without changing pickup behavior.
- **Coin Pickup Visual** motion belongs to the visual transform, while the **Pickup** root owns collection contacts.
- External coin meshes are source material for **Coin Pickup Visual**, not gameplay objects.

## Example dialogue

> **Dev:** "Does a pickup grant coins as soon as any collider touches it?"
> **Domain expert:** "No - **Pickup Sensor Source** reports **Pickup Contact**, and **Pickup Collection Controller** decides whether it becomes a **Pickup Collection Event**."

> **Dev:** "Does the pickup trigger still own collection contact events?"
> **Domain expert:** "No - **Pickup Sensor Source** originates **Pickup Contact** from pickup-eligible sensor triggers and resolves the touched **Pickup**."

> **Dev:** "Should `PickupCollectionController` subscribe to raw hand sensor `TriggerNotifier`s?"
> **Domain expert:** "No - it consumes typed **Pickup Contact** reports from **Pickup Sensor Source**."

> **Dev:** "Do we need a pickup contact adapter before a sensor can resolve the touched pickup?"
> **Domain expert:** "No - first resolve with `GetComponentInParent<Pickup>()`; add an adapter only when pickup contact targets are no longer under the **Pickup** root."

> **Dev:** "Is a collected pickup state saved forever?"
> **Domain expert:** "No - **Level Pickup State** belongs to the current **Level Session**."

> **Dev:** "Should a spinning coin mesh change how many coins the player earns?"
> **Domain expert:** "No - that is **Coin Pickup Visual**; reward meaning belongs to **Pickup Definition**."

> **Dev:** "Should an animated hand sensor use **Player Tag** so it can collect pickups?"
> **Domain expert:** "No - animated sensor collection must enter through an explicit collection source or policy, not by tagging visual child sensors as the player collider."

> **Dev:** "Should pickup hand sensors be tagged Player?"
> **Domain expert:** "No - they are configured in **Pickup Sensor Source** and can stay untagged on the `PlayerBodyPart` physics layer."

> **Dev:** "Do pickup hand sensors need to trigger against the Player layer?"
> **Domain expert:** "No - pickup collection comes from pickup contacts, not `PlayerBodyPart` self-contact with the player collider."

> **Dev:** "Should pickup collection subscribe to every `TriggerNotifier` under **Character Visual Anchor**?"
> **Domain expert:** "No - it subscribes to **Pickup Sensor Source**, a configured set of pickup-eligible **Animated Contact Sensors**."

> **Dev:** "Can **Pickup Sensor Source** build its list by scanning all character hierarchy notifiers at runtime?"
> **Domain expert:** "No - pickup eligibility is explicit source configuration, not hierarchy discovery."

> **Dev:** "Should pickup-eligible hand sensors have a separate pickup sensor component?"
> **Domain expert:** "No - **Pickup Sensor Source** entries reference `TriggerNotifier` directly and own pickup eligibility."

> **Dev:** "Should `PickupCollectionController` manage hand sensor subscriptions?"
> **Domain expert:** "No - **Pickup Sensor Source** owns raw notifier subscriptions; the controller consumes typed **Pickup Contact** reports."

> **Dev:** "Should a pickup sensor log every time it touches something that is not a pickup?"
> **Domain expert:** "No - non-pickup contacts are ignored by **Pickup Sensor Source** and produce no **Pickup Contact**."

> **Dev:** "If both hands touch one pickup, should **Pickup Sensor Source** deduplicate that?"
> **Domain expert:** "No - it emits contact facts; **Pickup Collection Controller** and **Level Pickup State** prevent duplicate collection."

> **Dev:** "Should **Pickup Sensor Source** ignore contacts outside **Running**?"
> **Domain expert:** "No - **Pickup Sensor Source** emits contact facts; **Pickup Collection Controller** decides whether the current state can collect."

> **Dev:** "Should **Pickup Contact** include coin amount?"
> **Domain expert:** "No - it only reports the touched **Pickup** and sensor origin; reward meaning belongs to **Pickup Definition**."

> **Dev:** "If a hand sensor is already inside a pickup when **Pickup Sensor Source** enables, should it replay that contact?"
> **Domain expert:** "No - first pass reports new trigger enters only; source enable order should make it active before collection matters."

> **Dev:** "Should `PickupCollectionController` serialize the hand/head sensor list?"
> **Domain expert:** "No - that belongs to **Pickup Sensor Source**; the controller consumes its reports."

> **Dev:** "Should **Pickup Sensor Source** live under the animated character because hand sensors live there?"
> **Domain expert:** "No - it lives with gameplay ownership and references the animated sensors as inputs."

> **Dev:** "Does using the same hand sensor for obstacle notices stop it from collecting pickups?"
> **Domain expert:** "No - source membership is purpose eligibility; **Pickup Sensor Source** still resolves pickup contacts for that sensor."

> **Dev:** "Should pickup-eligible hand sensors get Rigidbodies so pickup triggers fire?"
> **Domain expert:** "No - pickup sensor eligibility is a collection policy; **Animated Contact Sensor Physics Root** provides the Rigidbody participant needed for trigger delivery."

> **Dev:** "Should pickup collection force the copied hand pose into physics immediately every rendered frame?"
> **Domain expert:** "No - **Pickup Sensor Source** consumes trigger reports from physics; same-frame physics queries need a separate measured policy."

> **Dev:** "Should pickup rules hardcode `Head` or `Hand` sensor ids?"
> **Domain expert:** "No - first-pass eligibility may use configured object names or hierarchy paths; stable ids can come later as authored assets."

> **Dev:** "Should `GameplayLifetimeScope` keep owning the scene pickup list and player pickup contact colliders?"
> **Domain expert:** "No - scene pickup authoring belongs to **Gameplay Pickups Scene Composition Installer**; the lifetime scope consumes the installer registrations."

> **Dev:** "Does the pickup scene installer collect rewards?"
> **Domain expert:** "No - it wires pickup scene sources; collection rules stay in **Pickup Collection Controller** and **Level Pickup State**."

> **Dev:** "Should the pickup scene installer find every `Pickup` in the scene at runtime?"
> **Domain expert:** "No - first-pass runtime composition uses explicit **Pickup** references; auto-collect can be an editor authoring helper later."

> **Dev:** "Is an empty pickup list a broken level?"
> **Domain expert:** "No - an empty explicit **Pickup** set is valid for a level authored without pickups."

> **Dev:** "Does a no-pickup level still need hand pickup sensors wired?"
> **Domain expert:** "No - **Pickup Sensor Source** authoring is required only when the explicit **Pickup** set is non-empty."

> **Dev:** "Should a no-pickup level remove pickup collection services?"
> **Domain expert:** "No - it uses empty/no-op pickup sources so pickup collection is a quiet no-op and the notification boundary remains available."

> **Dev:** "Should a no-pickup level warn if the shared Player still has pickup sensor entries?"
> **Domain expert:** "No - authored sensor entries are allowed without warning when the explicit **Pickup** set is empty."

> **Dev:** "Can a level with pickups keep **Pickup Sensor Source** empty?"
> **Domain expert:** "No - explicit **Pickup** references require at least one configured pickup sensor; empty/no-op sensors are valid only for no-pickup levels."

> **Dev:** "Can a pickup-bearing level reference a hand notifier whose collider is not a `PlayerBodyPart` trigger?"
> **Domain expert:** "No - each configured pickup sensor entry needs an enabled trigger Collider on `PlayerBodyPart`, but it does not need Player Tag or its own Rigidbody."

> **Dev:** "Can a pickup-bearing level keep a configured hand sensor disabled?"
> **Domain expert:** "No - pickup-bearing validation requires an enabled `TriggerNotifier` and an active sensor GameObject; otherwise the source silently misses contacts."

> **Dev:** "Does a sensor-driven pickup still need its own `TriggerNotifier`?"
> **Domain expert:** "No - **Pickup Sensor Source** owns contact reports; each explicit **Pickup** still needs exactly one enabled trigger Collider on **Pickup Layer** under its hierarchy."

> **Dev:** "Can one **Pickup** have several trigger Colliders?"
> **Domain expert:** "No - first-pass authoring keeps pickup volume ownership simple: one explicit **Pickup**, one pickup-side trigger Collider."

> **Dev:** "Does that one pickup-side Collider have to be a primitive Collider?"
> **Domain expert:** "No - any Unity Collider subtype is valid as long as it is the single enabled trigger Collider under the **Pickup** hierarchy on **Pickup Layer**."

> **Dev:** "Should another collider under the pickup visual hierarchy fail validation?"
> **Domain expert:** "Only if it is on **Pickup Layer**; non-**Pickup Layer** colliders are outside pickup collection authoring."

> **Dev:** "Should animated pickup sensors own a Rigidbody so trigger callbacks fire?"
> **Domain expert:** "No - **Animated Contact Sensors** stay trigger-only; the gameplay-owned **Animated Contact Sensor Physics Root** provides the required Rigidbody participant."

> **Dev:** "Should every pickup have a Rigidbody because it has a trigger Collider?"
> **Domain expert:** "No - first-pass pickup authoring keeps pickups as trigger targets; **Animated Contact Sensor Physics Root** owns trigger delivery."

> **Dev:** "Should a no-pickup level fail if `PlayerBodyPart` cannot overlap **Pickup Layer**?"
> **Domain expert:** "No - `PlayerBodyPart` to **Pickup Layer** matrix validation is required only for levels with explicit **Pickup** references."

> **Dev:** "Can I disable a referenced pickup to make it initially unavailable?"
> **Domain expert:** "No - first-pass explicit **Pickup** references must be active; initial availability needs an explicit policy later."

## Flagged ambiguities

- "Coin" resolves to **Coin Pickup** for the collectible object and **Currency Definition** for the spendable currency identity.
- "Big coin" resolves to **Big Coin Pickup** for reward value and to scaled **Coin Pickup Visual** for presentation.
- "Coin mesh", "coin material", "coin spin", and "coin phase" resolve to **Coin Pickup Visual**, not **Coin Pickup**.
- "Downloaded coin" resolves to source material for **Coin Pickup Visual**, not a scene-authored **Coin Pickup**.
- "Collected" means an accepted **Pickup Collection Event**, not merely a trigger contact.
- "Pickup trigger entered" resolves to low-level Unity physics; **Pickup Contact** comes from **Pickup Sensor Source**.
- "Pickup collider resolution" resolves first to `GetComponentInParent<Pickup>()`, not to a new contact adapter.
- "Pickup state" resolves to **Level Pickup State** when discussing collected/not-collected status.
- "Layer" and "tag" are distinct collection contracts: **Pickup Layer** belongs to pickups and **Player Tag** identifies the collecting collider.
- "PlayerBodyPart" resolves to the physics layer for pickup-eligible **Animated Contact Sensors**, not a pickup rule or Unity tag.
- "Player hand pickup contact" resolves to an **Animated Contact Sensor** report, not a **Player Tag** contract.
- "All character visual TriggerNotifiers" resolves to configured **Pickup Sensor Source** for pickup collection, not an unchecked hierarchy scan.
- "Pickup sensor config" resolves to **Pickup Sensor Source** authoring, not to **Pickup Collection Controller** fields.
- "Pickup scene setup" resolves to **Gameplay Pickups Scene Composition Installer**, not to **GameplayLifetimeScope** serialized pickup fields.
- "Pickup auto-discovery" resolves to optional editor authoring support, not the first-pass runtime composition contract.
- "Empty pickup list" resolves to a valid no-pickup level, not broken scene setup.
- "No-pickup sensor setup" resolves to optional/no-op pickup sensor composition, not required scene authoring.
- "Empty Pickup Sensor Source" resolves to a valid no-op only when the explicit **Pickup** set is empty; with authored **Pickups**, it is invalid setup.
- "Pickup sensor entry shape validation" resolves to pickup-bearing setup validation for `TriggerNotifier` plus same-GameObject enabled trigger Collider on `PlayerBodyPart`, not to Player Tag or Rigidbody requirements.
- "Disabled pickup sensor entry" resolves to invalid pickup-bearing setup, but allowed quiet shared Player authoring when the explicit **Pickup** set is empty.
- "Pickup-side trigger" resolves to an enabled trigger Collider under the **Pickup** hierarchy on **Pickup Layer**, not a required pickup-owned `TriggerNotifier` event source.
- "Pickup Collider type" resolves to any Unity Collider subtype, not a primitive Collider whitelist.
- "Multiple pickup trigger Colliders" resolves to multiple Colliders on **Pickup Layer** under one **Pickup** hierarchy, not to non-pickup-layer visual or physics detail colliders.
- "Pickup-side TriggerNotifier" resolves to legacy compatibility in sensor-driven collection, not required pickup collection authoring.
- "Pickup trigger Rigidbody" resolves to **Animated Contact Sensor Physics Root**, not per-pickup Rigidbody or **Character Visual Anchor** Rigidbody.
- "`PlayerBodyPart` to **Pickup Layer** matrix validation" resolves to pickup-bearing level validation, not global scene validation.
- "Optional pickup setup" resolves to empty/no-op pickup sources for no-pickup levels, not optional **Pickup Collection Controller** composition.
- "Authored pickup sensors in a no-pickup level" resolves to allowed shared Player authoring, not pickup-bearing level content.
- "Inactive pickup reference" resolves to invalid scene setup in the first pass, not hidden initial availability.
