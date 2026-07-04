# Character Visual Follower Presentation Smoothing Issues

Parent PRD: [Character Visual Follower Presentation Smoothing PRD](../../prd/prd-character-visual-follower-presentation-smoothing.md)

Related ADRs:

- [ADR-0002: Keep Gameplay Logic In Plain C# Controllers](../../adr/adr-0002-keep-gameplay-logic-in-plain-csharp-controllers.md)
- [ADR-0005: Use VContainer For Dependency Injection](../../adr/adr-0005-use-vcontainer-for-dependency-injection.md)
- [ADR-0006: Register Views Without Injecting MonoBehaviours](../../adr/adr-0006-register-views-without-injecting-monobehaviours.md)
- [ADR-0009: Use Cinemachine For Run Camera](../../adr/adr-0009-use-cinemachine-for-run-camera.md)

These local implementation issues are ordered by dependency. They are tracer-bullet slices for adding a presentation-only **Character Visual Follower** that smooths the visible **Character Visual Anchor** while preserving the Rigidbody-backed **Launch Target**, camera source, raw support detection, steering frame, run progress, and run-end logic.

## Issues

| ID | Title | Type | Blocked by | User stories covered |
| --- | --- | --- | --- | --- |
| 01 | [Snap-Only Character Visual Follower Tracer Bullet](01-snap-only-character-visual-follower-tracer-bullet.md) | AFK | None | 7-10, 19-20, 27, 32-45, 52-57, 64-66, 69-71 |
| 02 | [Bounded Position And Heading Smoothing](02-bounded-position-and-heading-smoothing.md) | AFK | 01 | 5-6, 11-12, 20-21, 23-24, 39-42, 51, 58, 60-61 |
| 03 | [Soft Up-Tilt Smoothing And Safe Pose Composition](03-soft-up-tilt-smoothing-and-safe-pose-composition.md) | AFK | 02 | 1-4, 13-15, 22, 25-26, 42, 51, 59, 62-63 |
| 04 | [Presentation-Owned Anchor Composition Hardening](04-presentation-owned-anchor-composition-hardening.md) | AFK | 03 | 7-10, 16-18, 32-38, 45-57, 64-66, 69-71 |
| 05 | [Side-Bank Visual Smoothing Feel Review And Tuning](05-side-bank-visual-smoothing-feel-review-and-tuning.md) | HITL | 02, 03, 04 | 1-18, 19-31, 58-68, 72 |

## Notes

- Keep this presentation-only. No gameplay system should consume the smoothed **Character Visual Anchor** pose.
- Keep **Launch Target** as the physical truth. The follower samples its render pose and writes only the visual anchor pose.
- Keep **CharacterPresenter** responsible for animation state and frame application. The visual follower must not set animator parameters.
- Do not change **PhysicsRunSurfaceContextSource**, **RunSurfaceSteeringFrameSource**, run progress, run end, camera follow targets, Rigidbody settings, colliders, or physics materials in these issues.
- Do not add a new package, CharacterController, hidden containment, public gameplay API, or ScriptableObject config.
- Run Unity compile before implementation tests in each AFK slice. Prefer EditMode tests for pure smoothing/controller behavior; use PlayMode tests for scene composition and VContainer wiring.
- The HITL slice is a feel/tuning pass after deterministic behavior slices land.
- Do not publish these remotely unless explicitly requested.
