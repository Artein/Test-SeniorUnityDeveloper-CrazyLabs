## Problem Statement

> Superseded behavior note: older Slingshot PRDs described **Band Release Recoil** as detaching when the **Band** reaches the rest/idle/default
> shape. The current glossary sharpens that rule: the **Band** detaches only once the rest/idle/default **Band Shape** is clear of the current
> **Launch Target Silhouette**, including visible **Band** thickness.

When the player performs a valid but small **Pull Release**, the **Launch Target** can leave the **Slingshot** slowly enough that it remains near the
**Band** path for several rendered frames. During those frames, the current **Band Release Recoil** can visually return toward the rest/idle/default
shape and switch to simple two-span geometry while the **Launch Target** is still occupying that space. The result is a visible pass-through: the
**Band** appears to cut through the **Launch Target** after launch.

From the player's perspective, this breaks the physical read of the shot. The **Band** looked like it loaded and pushed the **Launch Target**, but
then it passes through the target during recoil. The issue is most visible on low-impulse launches because the **Launch Target** has not moved far
enough away before the **Band** visually relaxes.

The implementation must preserve the established Slingshot model: **Band Shape** remains visual-only, **Launch** power and direction still come from
**Pull** data, and the deterministic taut **Band Shape** solver remains the project direction instead of runtime rope physics.

## Solution

Update **Band Release Recoil** so detachment is gated by target clearance, not by recoil progress alone.

After a valid **Launch** is applied, the **Band** should continue using live collider-aware **Band Shape** data while the current **Launch Target
Silhouette** can intersect or occlude the rest/idle/default **Band Shape**. The virtual recoil **Pull Point** still relaxes from the final launch
**Pull Point** toward the **Rest Point** and continues to define the **Pulled Side** during recoil. The current moving **Launch Target Silhouette**
continues to supply **Band Contact Points** and **Band Wrap** geometry.

Once the rest/idle/default **Band Shape** is clear of the current **Launch Target Silhouette**, including visible **Band** thickness, the **Band** may
detach and stop following the **Launch Target**. If recoil progress reaches its end before that clearance is proven, the **Band** must remain in a
safe collider-aware visual state and retry clearance on later ticks. If live collider-aware solving fails before clearance is proven, the **Band**
must keep the last valid collider-aware **Band Shape** for that frame instead of blending toward rest, switching to simple geometry, or detaching.

The user-visible result is that low-impulse launches no longer show the **Band** passing through the **Launch Target** during recoil. The **Band**
still visibly returns to rest and detaches once the target is clear, but it does not use the rest/idle/default path as a shortcut through the target.

## Unity Surfaces

- Runtime assemblies and asmdefs
  - Existing Slingshot runtime assembly updates **Band Release Recoil** ownership in the plain C# controller.
  - Existing taut **Band Shape** provider remains the source of collider-aware **Band Contact Points** and **Band Wrap** during recoil.
  - A small pure clearance policy module may be introduced if the detach rule would otherwise mix lifecycle, geometry, and rendered-thickness checks
    inside the controller.
  - The Slingshot view or configuration surface may need a narrow way to expose visible **Band** radius for production clearance.
- Editor assemblies, windows, inspectors, importers, or menu items
  - No new Editor window, importer, or inspector is required.
  - Existing gizmos may remain unchanged for this slice.
- Scenes, prefabs, ScriptableObjects, package manifests, or ProjectSettings
  - No scene or prefab behavior change is expected beyond runtime behavior, unless the chosen visible **Band** radius source requires serialized
    authoring.
  - No package manifest or ProjectSettings change is expected.
  - Existing Slingshot config values such as minimum pull distance, contact padding, wrap sample count, and recoil duration remain valid.
- RPC/helper commands, hooks, or shell wrappers
  - Unity AI Agent Connector compile is the required compile gate.
  - Targeted EditMode and PlayMode tests should be run through the Unity AI Agent Connector after the implementation.
  - No new shell wrappers or RPC contracts are required.
- Package versioning, changelog, and installation/sync behavior
  - This is project gameplay work, not a package release.
  - No package version, changelog, install, or sync behavior is expected to change.

## User Stories

1. As a player, I want low-impulse launches to keep the **Band** outside the **Launch Target**, so that the shot still feels physical.
2. As a player, I want the **Band** to stop passing through the launched **Launch Target**, so that recoil does not look broken.
3. As a player, I want the **Band** to remain visually attached only while it can safely wrap around the target, so that the target does not look
   tethered forever.
4. As a player, I want the **Band** to detach after the rest path is clear, so that the **Launch Target** can continue the **Run** naturally.
5. As a player, I want small valid **Pull Release** launches to look as polished as deeper launches, so that launch strength does not expose visual
   defects.
6. As a player, I want **Band Release Recoil** to feel like the **Band** pushed the **Launch Target**, so that the launch handoff remains readable.
7. As a player, I want the **Band** to return toward the **Rest Point** without slicing through the target, so that recoil feels like elastic motion.
8. As a player, I want the **Launch Target** to keep launching from the final **Pull Point**, so that fixing recoil does not change shot control.
9. As a player, I want launch speed and steering to stay predictable, so that visual recoil fixes do not retune the mechanic.
10. As a designer, I want existing Slingshot tuning to remain valid, so that this bug fix does not require a broad balance pass.
11. As a designer, I want visible **Band** thickness included in clearance, so that the rendered line does not still clip the **Launch Target**.
12. As a designer, I want **Band Contact Padding** to keep its current meaning, so that contact tuning remains about visible clearance from the target.
13. As a designer, I want **Band Recoil Duration** to remain a feel control, so that it does not become a hidden detach safety timeout.
14. As a designer, I want no new rope physics dependency, so that the Slingshot remains deterministic and easy to tune.
15. As a designer, I want weak or canceled **Pull Release** behavior to remain unchanged, so that only accepted **Launch** recoil is affected.
16. As a developer, I want **Band Shape** to remain visual-only, so that launch math does not depend on rendered geometry.
17. As a developer, I want recoil progress separated from detach clearance, so that time interpolation cannot be mistaken for target clearance.
18. As a developer, I want the virtual recoil **Pull Point** to keep driving the **Pulled Side**, so that recoil direction remains tied to the shot.
19. As a developer, I want the moving **Launch Target Silhouette** to keep supplying contact geometry, so that the **Band** follows the target it can
   still visually touch.
20. As a developer, I want to avoid using the launched target midpoint as the solver query point, so that **Pulled Side** does not drift away from
   the release direction.
21. As a developer, I want simple two-span geometry blocked during unsafe recoil, so that the near-rest shortcut cannot pass through the target.
22. As a developer, I want rest/idle/default geometry blocked during unsafe recoil, so that final recoil progress cannot force visual clipping.
23. As a developer, I want live collider-aware solving preferred during recoil, so that the existing taut solver remains the source of safe shapes.
24. As a developer, I want solve failure before proven clearance to fail closed, so that temporary geometry failure does not produce an unsafe visual.
25. As a developer, I want the last valid collider-aware **Band Shape** retained on unsafe solve failure, so that the frame remains visually plausible.
26. As a developer, I want solve failure after proven clearance to allow normal detachment, so that the **Band** does not keep chasing unnecessarily.
27. As a developer, I want clearance measured in the **Pull Plane**, so that production checks align with the deterministic taut solver model.
28. As a developer, I want clearance checked against the current **Launch Target Silhouette**, so that moving target position is respected.
29. As a developer, I want visible **Band** radius included in the clearance margin, so that centerline-only checks cannot approve clipping.
30. As a developer, I want a small deep module for clearance if needed, so that geometry policy can be tested without Unity lifecycle noise.
31. As a developer, I want the Slingshot controller to stay responsible for lifecycle orchestration, so that recoil states remain explicit.
32. As a developer, I want the taut **Band Shape** provider to stay focused on shape solving, so that it does not gain launch lifecycle decisions.
33. As a developer, I want Unity `Collider` access to remain behind the existing silhouette adapter boundary, so that plain C# tests remain practical.
34. As a developer, I want MonoBehaviours to remain shallow adapters, so that they do not accumulate recoil policy.
35. As a developer, I want existing fixed **Band Shape** buffer sizing to remain valid, so that the LineRenderer contract does not churn.
36. As a developer, I want existing **Band Wrap** sample count behavior preserved, so that visual quality settings remain stable.
37. As a developer, I want existing **LaunchRequested** and launch-applied handoff semantics preserved, so that Gameplay Flow does not change.
38. As a developer, I want accepted **Pull Release** to keep the loaded **Band Shape** through launch handoff, so that the existing launch boundary
   remains stable.
39. As a developer, I want **Band Release Recoil** to start only after launch application, so that pre-launch cleanup is not conflated with recoil.
40. As a developer, I want **Band Release Recoil** to stop querying target geometry after safe detach, so that the **Band** does not chase the target
   during the **Run**.
41. As a developer, I want no unconditional physics synchronization added, so that the hot path does not gain cost without evidence.
42. As a tester, I want a regression that starts from a low valid **Pull**, so that the exact visible defect is covered.
43. As a tester, I want the regression to sample multiple recoil frames, so that a one-frame passing result cannot hide later clipping.
44. As a tester, I want the regression to inspect the rendered **Band Shape**, so that the assertion matches what the player sees.
45. As a tester, I want the regression to compare against the real Unity target `Collider`, so that PlayMode catches engine-boundary issues.
46. As a tester, I want rendered **Band** radius included in the PlayMode assertion, so that the test catches line-thickness clipping.
47. As a tester, I want focused EditMode tests only when a plain policy module is introduced, so that tests do not overfit private controller code.
48. As a tester, I want existing deep-pull recoil coverage preserved, so that the fix does not regress current live-provider behavior.
49. As a tester, I want weak-release tests preserved, so that invalid **Pull Release** still returns to capture idle.
50. As a tester, I want launch request tests preserved, so that final **Pull Point**, launch speed, and steering remain unchanged.
51. As a tester, I want compile to pass before running tests, so that test results are not polluted by script errors.
52. As a maintainer, I want this PRD to supersede only the old recoil detach wording, so that broader Slingshot PRDs remain useful.
53. As a maintainer, I want ADR-0008 respected, so that the project does not drift back toward rope physics for a visual bug.
54. As a maintainer, I want no public API, save format, or package dependency change unless implementation proves it necessary.
55. As a maintainer, I want unresolved tuning values recorded as assumptions, so that implementation can proceed without reopening design questions.
56. As a maintainer, I want the fix scoped to **Band Release Recoil**, so that unrelated run, camera, input, and launch features are not refactored.

## Implementation Decisions

- Use established glossary terms: **Slingshot**, **Band**, **Band Shape**, **Band Wrap**, **Band Contact Point**, **Pulled Side**, **Pull Point**,
  **Rest Point**, **Pull Plane**, **Launch**, **Launch Target**, **Launch Target Silhouette**, and **Band Release Recoil**.
- Respect ADR-0008: keep deterministic taut **Band Shape** solving in the **Pull Plane** and do not introduce runtime rope physics, joints, or
  Unity-physics-driven band simulation.
- Respect the existing architecture direction: gameplay and presentation rules stay in plain C# controllers or services; MonoBehaviours remain
  shallow Unity adapters.
- This PRD is a focused follow-up to the target-coupled Slingshot work. It supersedes only the older statement that **Band Release Recoil** detaches
  merely when the **Band** reaches rest.
- **Band Release Recoil** detach requires clearance of the rest/idle/default **Band Shape** against the current **Launch Target Silhouette** plus
  visible **Band** radius.
- Recoil progress may still drive the visual relaxation curve, but progress completion is not sufficient proof that detachment is safe.
- If recoil progress reaches the end before clearance is proven, the **Band** remains in a safe collider-aware visual state and retries clearance on
  later ticks.
- If clearance is proven before the visual recoil curve has completed, implementation may continue the visual recoil to its normal completion; it
  should not detach early in a way that visibly skips intended recoil.
- During **Band Release Recoil**, the virtual recoil **Pull Point** continues to interpolate from the final launch **Pull Point** toward the
  **Rest Point**.
- During **Band Release Recoil**, that virtual recoil **Pull Point** drives **Pulled Side** selection.
- During **Band Release Recoil**, the moving current **Launch Target Silhouette** supplies **Band Contact Points** and **Band Wrap** geometry.
- Do not use the launched target collider midpoint, bounds center, or Rigidbody position as the solver query point for **Pulled Side** during recoil.
- Simple two-span **Band Shape** is allowed for near-rest active/capture visuals, but it must not replace collider-aware recoil while target clearance
  is still unknown or blocked.
- The rest/idle/default **Band Shape** is the detach candidate. It may be shown after clearance, but it must not be used as an unsafe intermediate
  fallback before clearance.
- Live collider-aware **Band Shape** solving remains preferred during unsafe or unknown-clearance recoil.
- If live collider-aware solving succeeds during recoil, the resulting **Band Shape** becomes the last valid collider-aware recoil shape.
- If live collider-aware solving fails before clearance is proven, keep the last valid collider-aware recoil shape for that frame and retry on the
  next tick.
- Do not blend the last active **Band Shape** toward rest as a solve-failure fallback while clearance is unknown.
- Do not detach, switch simple, or show detached rest geometry while clearance is unknown.
- After clearance is proven and the **Band** detaches, the Slingshot should stop querying **Launch Target** contact/wrap geometry for recoil.
- Production clearance should use a **Pull Plane** geometry check against the current **Launch Target Silhouette** expanded by visible **Band** radius.
- The visible **Band** radius should come from a narrow runtime surface that reflects the rendered **Band** thickness; the exact implementation may
  be a view-exposed value or a config-owned value, but it should not couple the controller to LineRenderer internals.
- A pure clearance policy module is recommended if the production clearance check needs segment-versus-silhouette math, radius inflation, or multiple
  return states. Its stable interface should answer whether the detach candidate **Band Shape** is clear for the current silhouette and margin.
- The Slingshot controller should remain the owner of recoil lifecycle: pending launch handoff, active recoil, clearance-gated detach, and final idle
  presentation.
- The taut **Band Shape** provider should remain responsible for producing safe collider-aware **Band Shape** points from a query; it should not own
  launch or detach lifecycle.
- The **Launch Target** silhouette adapter should remain responsible for reading Unity `Collider` geometry from the current target pose.
- Existing launch request data and launch physics should not change.
- Existing held target behavior should not change: accepted **Pull Release** keeps the **Launch Target** at the final **Pull Point** through launch
  handoff, and launch application re-applies that final **Pull Point** before velocity change.
- Existing weak, canceled, forward-only, or invalid projection **Pull Release** behavior should not change.
- Existing **Band Shape** point count and ordered polyline rendering should remain stable.
- Existing scene composition should not require a new gameplay state or a new launch sequence step.
- No new ADR is required for this PRD because ADR-0008 already records the architectural decision to use deterministic taut solving instead of rope
  physics. This PRD refines a behavior within that decision.

## Testing Decisions

- Good tests should verify externally visible behavior: rendered **Band Shape** clearance, launch handoff, target movement, and detach lifecycle. They
  should not assert private helper structure unless a new pure policy module is introduced.
- Add the regression test first as a PlayMode test in the existing natural **Band Shape** scene coverage.
- The PlayMode regression should pull just above `MinimumPullDistance`, release, and sample the rendered **Band Shape** for multiple recoil frames.
- The PlayMode regression should assert every rendered **Band Shape** segment stays outside the real **Launch Target** collider by rendered **Band**
  radius plus a small margin.
- The PlayMode regression should use the same style as existing natural **Band Shape** scene tests: drive input through editor mouse events, read the
  LineRenderer world positions, and compare against the real target collider.
- The PlayMode regression should cover the low-impulse case because deep-pull recoil already has precedent for using the live **Band Shape** provider.
- The PlayMode regression should not depend on exact private recoil buffer names, private flags, or implementation-only state.
- The PlayMode regression should sample enough frames to cover the vulnerable period where the target is still near the rest/idle/default path.
- The PlayMode regression should fail against the current behavior before production changes, or be adjusted until it reproduces the observed
  low-impulse clearance issue.
- Preserve existing PlayMode tests that prove capture idle, rest press, tiny pull, shallow lateral pull, and active pull shapes stay outside the target
  collider.
- Preserve existing EditMode launch request tests that prove accepted **Pull Release** keeps the loaded **Band Shape** through launch handoff.
- Update or replace EditMode tests that currently encode "detach at rest" as the full completion rule.
- Add EditMode tests for the controller if the behavior can be expressed through public/fake dependencies: low valid release should keep using live
  collider-aware recoil instead of simple geometry while clearance is blocked.
- Add EditMode tests for fail-closed behavior if a fake provider can simulate solve failure before clearance: the view should receive the last valid
  collider-aware **Band Shape**, not a rest blend or detached idle.
- Add EditMode tests for safe detach when clearance is proven: once visual recoil is ready and the rest/idle/default shape is clear, the view may show
  detached idle and stop following the target.
- If a pure clearance policy module is introduced, test it directly with deterministic **Pull Plane** shapes, silhouette samples, visible **Band**
  radius, clear cases, blocked cases, and boundary/tolerance cases.
- If visible **Band** radius is exposed through a view or config surface, add a focused test that the production clearance policy uses that value.
- Do not use reflection to inspect private controller state.
- Do not hardcode asset paths, GUIDs, Resources paths, or package paths in tests.
- Compile must be clean before running tests.
- Run targeted EditMode Slingshot tests after controller or policy changes.
- Run the targeted natural **Band Shape** PlayMode tests after scene-level regression changes.
- Manual Unity smoke should verify shallow valid launch, deeper launch, left/right lateral launch, and post-launch detach timing from the player
  camera.

## Release and Compatibility

- Unity version assumption: the current Unity 6 project with Unity Test Framework, VContainer composition, New Input System editor mouse test support,
  Rigidbody launch target behavior, and URP rendering.
- No new third-party dependency is expected.
- No package version, package manifest, package lock, or install/sync behavior change is expected.
- No save format or player data migration is expected.
- No Gameplay State asset migration is expected.
- No scene migration is expected unless the chosen visible **Band** radius source requires a serialized value or view reference.
- Backward compatibility risk is limited to visual recoil timing and tests that assumed detachment happens as soon as recoil progress reaches rest.
- Existing launch power, launch direction, **Pull Offset**, **Pull Distance**, **MinimumPullDistance**, and Rigidbody velocity-change semantics should
  remain unchanged.
- The fix should be compatible with existing single explicit **Launch Target Silhouette** collider support. Compound or multi-collider support remains
  deferred.

## Out of Scope

- Runtime rope physics, joints, cloth, spline physics, or physically simulated elastic tension.
- Replacing the deterministic taut **Band Shape** solver.
- Using the launched target midpoint as the recoil solver query point.
- Retuning launch power, launch speed curve, lateral steering, or launch lift.
- Changing **Pull** capture, **Band Touch Target**, **Touch Indicator**, or **Pull Hint** behavior.
- Changing **Gameplay State**, **Gameplay Flow**, **Run Camera**, **Run End Flow**, or run control behavior.
- Adding haptics, audio, particles, camera shake, or a richer **Launch Sequence**.
- Supporting compound or multi-collider **Launch Target Silhouette** solving.
- Exact arbitrary mesh wrapping beyond the existing inflated convex **Launch Target Silhouette** model.
- New custom editor tooling.
- Package publishing, changelog updates, or remote issue tracker publishing.

## Further Notes

- Assumption: low valid **Pull Release** can be reproduced by pulling just above the authored `MinimumPullDistance` in the Gameplay Scene.
- Assumption: the current rendered **Band** radius should be included in both production clearance and the PlayMode regression oracle.
- Assumption: the existing fixed point count for **Band Shape** remains sufficient for recoil and detach-candidate clearance.
- Assumption: no unconditional physics transform synchronization is needed unless a PlayMode boundary test proves current collider samples are stale.
- Implementation detail still open: exact source of visible **Band** radius for production clearance.
- Implementation detail still open: exact clearance tolerance and PlayMode assertion margin.
- Implementation detail still open: exact number of recoil frames sampled by the PlayMode regression.
- No design question remains unresolved from the grill session: detach condition, clearance measurement, recoil solver query ownership, solve-failure
  fallback, and regression-test contract are settled.
