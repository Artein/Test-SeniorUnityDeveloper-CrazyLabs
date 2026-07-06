# Character Collider And Animated Contact Sensors Implementation Issues

Parent PRD: [Character Collider Split And Animated Contact Sensors PRD](../../prd/prd-character-collider-and-animated-contact-sensors.md)

Related ADRs:

- [ADR-0002: Keep Gameplay Logic In Plain C# Controllers](../../adr/adr-0002-keep-gameplay-logic-in-plain-csharp-controllers.md)
- [ADR-0003: Use Direct C# Events Before Event Bus](../../adr/adr-0003-use-direct-csharp-events-before-event-bus.md)
- [ADR-0005: Use VContainer For Dependency Injection](../../adr/adr-0005-use-vcontainer-for-dependency-injection.md)
- [ADR-0006: Register Views Without Injecting MonoBehaviours](../../adr/adr-0006-register-views-without-injecting-monobehaviours.md)

These local implementation issues are ordered by dependency. They are tracer-bullet slices for splitting overloaded character collider authority into **Launch Target Collider Root**, **Run Body Contact Collider**, and pickup-focused **Animated Contact Sensors** while preserving run movement, run end, and pickup collection behavior.

## Issues

| ID | Title | Type | Blocked by | User stories covered |
| --- | --- | --- | --- | --- |
| 01 | [Approve Collider Authority ADR And Migration Boundary](01-approve-collider-authority-adr-and-migration-boundary.md) | HITL | None | 16, 22-23, 25-28, 35 |
| 02 | [Split Run Body Contact Collider From Launch Target](02-split-run-body-contact-collider-from-launch-target.md) | AFK | 01 | 1, 3-6, 16, 23, 34 |
| 03 | [Route Pickup Collection Through Pickup Contact Source](03-route-pickup-collection-through-pickup-contact-source.md) | AFK | 01 | 18-21, 29, 33 |
| 04 | [Add Pickup Sensor Source And Sensor Entry Validation](04-add-pickup-sensor-source-and-sensor-entry-validation.md) | AFK | 03 | 2, 7, 10-11, 15, 17-20, 24, 29-33, 35 |
| 05 | [Update Pickup Authoring Contract For Sensor-Driven Collection](05-update-pickup-authoring-contract-for-sensor-driven-collection.md) | AFK | 03, 04 | 12-15, 31 |
| 06 | [Move Pickup Composition To Gameplay Pickups Scene Installer](06-move-pickup-composition-to-gameplay-pickups-scene-installer.md) | AFK | 04, 05 | 8-12, 24, 30-31 |
| 07 | [Add Animated Contact Sensor Pose Sync](07-add-animated-contact-sensor-pose-sync.md) | AFK | 01 | 2, 7, 15, 25-28, 35 |
| 08 | [Prove Sensor-Driven Trigger Delivery In PlayMode](08-prove-sensor-driven-trigger-delivery-in-playmode.md) | AFK | 04, 05, 07 | 2, 14, 26, 29, 32-33 |
| 09 | [Wire Gameplay Scene Collider And Pickup Sensor Smoke Path](09-wire-gameplay-scene-collider-and-pickup-sensor-smoke-path.md) | AFK | 02, 06, 08 | 1-15, 24, 29-34 |
| 10 | [Review Pickup Sensor Reach And Timing Feel](10-review-pickup-sensor-reach-and-timing-feel.md) | HITL | 09 | 1-2, 6-7, 29, 35 |

## Notes

- First implementation uses **Animated Contact Sensors** for pickup collection only.
- Body-part run obstacle notices are a future explicit feature through a separate **Run Obstacle Sensor Source**.
- **Run Finish**, **Run Safety Net**, movement support, and default **Obstacle Impact** stay on **Run Body Contact Collider**.
- Keep **Launch Target Collider Root** responsible for slingshot launch target and band contact behavior.
- Keep **Run Body Contact Collider** as a single `SphereCollider` for the first pass, with no animation-driven size switching.
- Do not call `Physics.SyncTransforms` every rendered frame by default; first-pass sensor delivery accepts the next Unity physics step.
- Runtime pickup composition uses explicit pickup and sensor references, not broad scene discovery.
- First-pass **Animated Sensor Identity** may use sensor GameObject name or hierarchy path, with a TODO for asset-backed IDs before identity becomes a stable content contract.
- Keep `PlayerBodyPart` interaction limited to **Pickup Layer** in the first pass. Do not add camera-obstacle interaction here.
- No remote issue publishing is part of this local issue breakdown unless explicitly requested.
- Run Unity compile through Unity AI Agent Connector before code/test changes in each implementation slice, then run targeted tests only after compile is clean.
