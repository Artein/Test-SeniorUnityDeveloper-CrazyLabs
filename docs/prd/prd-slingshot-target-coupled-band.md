## Problem Statement

> Superseded behavior note: this PRD records the completed target-coupled Band milestone. The current Band Shape target is the natural taut follow-up
> in `docs/tasks/slingshot-natural-band-shape/` and ADR-0008, which replaces the closest-point/contact and Pull Plane arc-wrap approximation with an
> inflated Launch Target Silhouette, tangent Band Contact Points, and pulled-side contour solving.

The current Slingshot launch slice can interpret Pull input and deform the Band, but the visible interaction still risks feeling disconnected from the Launch Target. If the player pulls the Band backward, the target must look like it is being physically loaded by the Slingshot. If the target remains at its idle position while only the Band moves, the interaction reads as a UI drag rather than a slingshot.

The current three-point Band Shape also treats the interpreted Pull Point as the visible Band middle. That is no longer correct once the Pull Point becomes the held Launch Target position. A Band renderer point at the Pull Point can run through the Launch Target mesh, which breaks the physical read of the loaded Band. The Band must instead meet the Launch Target at Band Contact Points and draw a collider-aligned Band Wrap around the target.

On a valid Launch, the Launch Target should depart from the loaded Pull Point. The Band should not snap to idle before the launch handoff, and it should not detach instantly if that makes the shot feel like the target moves by itself. The post-shot visual should read as the Band pushing the Launch Target forward, then returning to its rest/idle/default shape.

## Solution

Add target-coupled Slingshot loading and release visuals on top of the existing touch-launch feature.

During Active Pull, the Slingshot computes the clamped Pull Point, immediately positions the held Launch Target at that Pull Point, then computes the visible Band Shape from the Launch Target collider's current pose. The Pull Point remains gameplay data: it is the held and launch position. The renderer uses Band Contact Points and Band Wrap samples, not the Pull Point as a visible middle point.

The Band Shape becomes an ordered world-space polyline. It starts at the left anchor, follows the left Band leg to the left Band Contact Point, includes collider-aligned Band Wrap samples around the Launch Target, follows to the right Band Contact Point, and ends at the right anchor. The first slice uses one explicitly assigned Launch Target Collider of any Unity Collider type and generic Collider surface queries. This is a best-effort presentation approximation, not full rope physics.

On weak Pull Release, canceled Pull, or invalid Pull projection, the held Launch Target and Band return to the Slingshot Rest Point. On valid Pull Release, the Launch Target stays at the final Pull Point through launch handoff. The launch request includes the final Pull Point, and the launcher re-applies that position immediately before applying launch velocity. After launch application, the Band enters Band Release Recoil: it follows the moving Launch Target collider contact/wrap points while visually returning to the rest/idle/default shape. When that rest shape is reached, the Band detaches and stops following the Launch Target.

## Unity Surfaces

- Runtime assemblies and asmdefs:
  - Existing Slingshot runtime assembly gains held target positioning, Band contact/wrap calculation, ordered Band Shape data, and Band Release Recoil behavior.
  - Existing Gameplay runtime assembly may need composition updates so the Launch Target adapter is registered under the new narrow interfaces.
  - Existing utility assemblies may remain unchanged unless small math helpers are needed.
- Editor assemblies, windows, inspectors, importers, or menu items:
  - No custom Editor assembly is required for the first slice.
  - Existing selected-object Slingshot gizmos should expand to show collider contact/wrap authoring-relevant data if useful.
- Scenes, prefabs, ScriptableObjects, package manifests, or ProjectSettings:
  - Gameplay Scene wiring must assign one Launch Target Collider for Band contact/wrap calculation.
  - The existing Slingshot config gains Band visual quality and padding/recoil tuning fields where needed.
  - The existing Slingshot view must render an ordered Band Shape polyline rather than assuming exactly three positions.
  - The existing Launch Target adapter must expose held positioning and Band contact/wrap provider interfaces while staying shallow.
  - No ProjectSettings change is expected.
  - No new package dependency is expected.
- RPC/helper commands, hooks, or shell wrappers:
  - Unity AI Agent Connector compile and targeted tests should verify the implementation.
  - No new helper commands are required.
- Package versioning, changelog, and installation/sync behavior:
  - This is project gameplay work, not a package release.
  - No package manifest change is expected.
  - Existing scene serialization and test asset changes must be reviewed carefully because the feature touches Gameplay Scene composition.

## User Stories

1. As a player, I want the Launch Target to move with my Pull, so that the Slingshot feels like it is loading the target directly.
2. As a player, I want the Band to meet the Launch Target surface, so that the Band does not appear to cut through the mesh.
3. As a player, I want the Band to wrap around the visible target shape, so that the loaded Slingshot feels physical and readable.
4. As a player, I want the Launch Target to launch from the pulled position, so that the shot starts where I released the loaded Slingshot.
5. As a player, I want weak or canceled Pulls to reset the target and Band to rest, so that accidental gestures do not leave the scene in a loaded state.
6. As a player, I want the Band to remain loaded through launch handoff, so that release does not visually snap before the shot starts.
7. As a player, I want the Band to recoil alongside the departing Launch Target, so that it feels like the Band pushed the target forward.
8. As a player, I want the Band to detach after it reaches rest, so that the target does not appear tethered forever.
9. As a player, I want the Band Wrap to look smooth enough, so that the Slingshot feels polished rather than angular.
10. As a designer, I want Band Wrap sample quality to be tunable, so that I can balance visual crispness with simplicity.
11. As a designer, I want Band contact padding to be tunable, so that the Band can sit just outside the target mesh.
12. As a designer, I want recoil timing/shape to be tunable, so that Band Release Recoil can match the intended game feel.
13. As a designer, I want the Launch Target Collider assigned explicitly, so that the Band wraps the intended target shape.
14. As a designer, I want authoring validation when the Collider is missing, so that broken composition is visible before playtesting.
15. As a designer, I want existing Pull, launch, and touch tuning to keep working, so that this visual change does not force retuning the whole mechanic.
16. As a developer, I want the Pull Point to remain pure gameplay data, so that launch math stays separate from Band rendering.
17. As a developer, I want Band Contact Points to be separate from the Pull Point, so that visuals can avoid the target mesh without changing launch logic.
18. As a developer, I want held positioning behind a narrow interface, so that SlingshotController does not depend on Rigidbody details.
19. As a developer, I want Band contact/wrap behind a narrow interface, so that collider querying stays outside Slingshot input interpretation.
20. As a developer, I want the Launch Target adapter to remain shallow, so that Unity-specific data is exposed without embedding Slingshot rules in the MonoBehaviour.
21. As a developer, I want SlingshotController to move the held target before calculating Band contacts, so that contact data reflects the current Pull Point.
22. As a developer, I want held positioning to be immediate, so that same-frame Band contact calculations are deterministic enough for the first slice.
23. As a developer, I want held positioning to fail fast before hold, so that lifecycle mistakes are caught instead of hidden.
24. As a developer, I want held positioning to preserve rotation for now, so that future orientation behavior is explicit instead of accidental.
25. As a developer, I want the launch request to carry the final Pull Point, so that the launcher does not depend on mutable scene state.
26. As a developer, I want the launcher to re-apply the final Pull Point before launch, so that release position and launch position cannot drift apart.
27. As a developer, I want invalid launch requests to skip positioning and launch, so that broken payloads cannot move the target unexpectedly.
28. As a developer, I want Band Shape to be an ordered polyline, so that LineRenderer and tests can handle more than three points.
29. As a developer, I want Band Wrap samples to be presentation-only, so that visual quality changes cannot alter launch power or direction.
30. As a developer, I want generic Collider support, so that the first slice is not hardcoded to the current capsule-shaped player.
31. As a developer, I want exactly one explicit Collider in the first slice, so that compound target solving is deferred until it is actually needed.
32. As a developer, I want generic Band Wrap to be a best-effort approximation, so that we do not overpromise exact rope tangent behavior for arbitrary colliders.
33. As a developer, I want Band Wrap samples generated from a Pull Plane arc around the Pull Point, so that the wrap reads as going around the target instead of through it.
34. As a developer, I want asymmetric wrap cases to prefer the backward/pulled side, so that the Band does not flip visually to the front of the target.
35. As a developer, I want Band samples projected to the Pull Plane/Band height after collider queries, so that tall colliders do not pull the Band vertically out of its visual plane.
36. As a developer, I want contact padding to offset away from the Collider toward the query origin, so that the Band is visually outside the target.
37. As a developer, I want deterministic fallback directions for degenerate padding cases, so that Band Shape never produces NaN or unstable points.
38. As a developer, I want no unconditional physics transform sync in the hot path, so that we do not add cost without evidence.
39. As a tester, I want EditMode tests for Pull-to-target positioning order, so that controller behavior is validated without PlayMode overhead.
40. As a tester, I want PlayMode boundary tests for Rigidbody and Collider pose visibility if needed, so that Unity physics behavior is verified where pure tests cannot cover it.
41. As a tester, I want smoke tests updated away from exactly-three-point Band assumptions, so that tests match the ordered-polyline model.
42. As a maintainer, I want this PRD to remain a follow-up to the broader touch-launch PRD, so that the original feature scope does not become ambiguous.
43. As a maintainer, I want rope physics, Burst, and compound colliders deferred, so that this feature remains implementable as a practical first slice.

## Implementation Decisions

- This PRD is a follow-up to the broader Slingshot touch-launch PRD and supersedes any remaining assumption that the Band renderer middle point is the Pull Point.
- Use the established domain terms: Slingshot, Band, Band Shape, Band Wrap, Band Contact Point, Pull, Active Pull, Pull Offset, Pull Point, Rest Point, Launch Target, Pull Plane, Launch Frame, Pull Release, Launch, and Band Release Recoil.
- Keep MonoBehaviours shallow. Unity adapters expose serialized references, validation, and Unity API calls; rules stay in plain C# controllers/services.
- Keep one SlingshotController for first-slice input interpretation and visual command coordination. Do not introduce a separate presenter until visual orchestration becomes meaningfully larger.
- Add a held-positioning interface for the Launch Target. The minimal contract positions an already-held Launch Target at a world-space Pull Point.
- Held positioning is valid only after the target has been held. Calling it before hold fails fast.
- Held positioning is immediate enough that subsequent same-frame contact queries see the assigned Collider at the new pose.
- Held positioning preserves target rotation in this slice. Add an explicit TODO at the Launch Target boundary for future orientation, facing, lean, or spin work.
- Add a Band contact/wrap provider interface for the Launch Target. It provides Band Contact Points and Band Wrap samples from the assigned Collider's current Transform.
- The first slice supports one explicitly assigned Unity Collider of any Collider type. It does not auto-discover children or choose among multiple colliders.
- The contact provider uses generic Collider surface queries as a best-effort approximation. Exact collider-specific tangent or silhouette solving is deferred.
- SlingshotController update order during Active Pull is: project input, clamp Pull Point, set held target position, compute Band contact/wrap from the current Collider pose, build Pull visual, command the view.
- Pull Point remains the gameplay held/launch position. It is not a renderer point when that would place the Band inside the Launch Target.
- Band Shape becomes one ordered world-space polyline. It includes anchors, contact points, and wrap samples in render order.
- SlingshotView receives already computed Band Shape data and applies every point to the Band LineRenderer. It does not query colliders or calculate contacts.
- Rest/idle/inactive Band Shape uses the fixed anchor-to-Rest-Point-to-anchor shape. It does not wrap the Launch Target Silhouette in this slice.
- Band Wrap sample origins are generated from an arc on the Pull Plane around the Pull Point, then projected to the assigned Collider and padded outward.
- If left/right contact directions are asymmetric, Band Wrap uses the backward/pulled side aligned with negative Launch Frame forward rather than blindly selecting the shortest arc.
- Rendered Band Contact Points and Band Wrap samples are constrained back onto the Pull Plane/Band height after generic Collider queries.
- Contact padding offsets from the closest point back toward the query origin. Contact-point query origins are the relevant Band anchors; wrap query origins are the arc sample origins.
- Degenerate padding direction fallback order is Pull Plane direction from Collider bounds center to closest point, then negative Launch Frame forward, then no padding.
- Band Wrap segment count is designer-tuned through config with inspector clamping and validation. A practical default is around twelve samples, with a small supported range such as two through twenty-four.
- Contact padding is non-negative and designer-tuned through config or adapter authoring, with validation.
- Band Wrap samples are presentation-only. Changing sample count or padding must not change Pull distance, Pull Offset, normalized power, launch direction, launch speed, or final Pull Point.
- SlingshotLaunchRequest includes the final clamped Pull Point as pure data.
- SlingshotLaunchController depends on both launch and held-positioning interfaces. For a valid launch request, it re-applies the final Pull Point before launching.
- Invalid launch requests, invalid Pull Point values, or invalid final velocities warn and skip held positioning and launch.
- Weak, canceled, or invalid Pulls reset the held Launch Target and Band to Rest Point.
- Valid Pull Release keeps the Launch Target at the final Pull Point and keeps the loaded Band Shape through launch request notification and accepted launch handoff.
- Band Release Recoil starts only after launch application. It is post-shot visual behavior, not pre-launch cleanup.
- During Band Release Recoil, the moving Launch Target Collider supplies live contact geometry, while a virtual recoil Pull Point interpolates from the final Pull Point toward the Rest Point to define how loaded the Band still is.
- Band Release Recoil completion is defined by reaching the rest/idle/default Band Shape. There is no separate max-duration safety guard in this slice.
- Once Band Release Recoil reaches rest, it detaches from the Launch Target and stops querying target contact/wrap.
- Recoil timing or curve may be designer-tuned, but that tuning drives the intended visual interpolation rather than acting as a failure fallback.
- Do not call Unity physics transform synchronization unconditionally after held positioning. Add an engine-boundary test first; only introduce narrow sync if the test proves it is required.
- Scene composition registers the same Launch Target adapter under the launch, held-positioning, and Band contact/wrap interfaces as needed.
- Existing PlayMode smoke coverage must stop asserting a fixed three-point Band and instead verify that the LineRenderer receives the complete ordered Band Shape for the current state.

## Testing Decisions

- Good tests verify externally visible contracts: target movement, launch position, Band polyline shape, contact/wrap provider use, and recoil lifecycle. They should not assert private helper implementations.
- Prefer EditMode tests for SlingshotController sequencing, launch request payload, launch controller validation, Band Shape construction rules, config validation, and generic math helpers.
- Use PlayMode tests only for Unity engine behavior: Rigidbody held positioning, Collider query visibility after movement, LineRenderer scene wiring, and Gameplay Scene smoke behavior.
- SlingshotController EditMode tests should cover Active Pull moving the held Launch Target to the clamped Pull Point.
- SlingshotController EditMode tests should verify that held positioning happens before Band contact/wrap calculation.
- SlingshotController EditMode tests should verify that the view receives a Band Shape built from contact/wrap provider data, not a three-point anchor/PullPoint/anchor shape.
- SlingshotController EditMode tests should verify that weak, canceled, and invalid Pulls return the target to Rest Point.
- SlingshotController EditMode tests should verify that valid Pull Release does not reset the target or Band before launch notification.
- Slingshot launch tests should verify that a valid request re-applies final Pull Point before launch.
- Slingshot launch tests should verify that invalid request data skips both held positioning and launch.
- Band Shape tests should verify ordered polyline structure, endpoints, contact points, wrap sample inclusion, and sample count behavior.
- Band Shape tests should verify that changing Band Wrap segment count changes only visual sample density, not launch request data.
- Band contact/wrap provider tests should verify generic Collider contact points with representative Collider types where practical.
- Band contact/wrap provider tests should verify single explicit Collider ownership and no child-collider auto-selection.
- Band contact/wrap provider tests should verify padding direction from closest point toward query origin.
- Band contact/wrap provider tests should verify degenerate padding fallback order without producing invalid vectors.
- Band contact/wrap provider tests should verify Pull Plane/Band height projection after Collider queries.
- Band contact/wrap provider tests should verify arc-based wrap sampling rather than linear interpolation through the target.
- Band contact/wrap provider tests should verify backward-side arc selection for asymmetric contact cases.
- Rigidbody/Launch Target PlayMode tests should verify hold, immediate held positioning, rotation preservation, fail-fast positioning before hold, launch from held position, and same-frame Collider query visibility if the engine boundary requires it.
- Band Release Recoil tests should verify that recoil starts after launch application, keeps querying moving contact/wrap while returning to rest, and detaches when rest is reached.
- Gameplay Scene smoke tests should verify the assigned Launch Target Collider, the registered narrow interfaces, and the Band LineRenderer's ordered polyline usage.
- Existing tests that assert exactly three LineRenderer positions or a middle point equal to the Pull Point must be updated because those assertions are stale under the ordered-polyline model.
- Unity compile must pass before test runs. Run targeted tests for changed Slingshot, Gameplay, and PlayMode boundary behavior after implementation.
- Manual Unity smoke testing should check pull feel, collider wrapping readability, target movement, release recoil, gizmo clarity, and mobile finger comfort.

## Release and Compatibility

- Unity version assumption: Unity 6 project using the New Input System, URP, UGUI, Rigidbody physics, and Unity Test Framework.
- This PRD assumes the existing Slingshot touch-launch, Gameplay State, Unity Input, VContainer composition, and Gameplay Scene wiring remain the baseline.
- No new third-party dependency is expected.
- No save format or remote package compatibility impact is expected.
- Scene serialization changes are expected because the Launch Target Collider and possibly new config fields need authoring.
- Backward compatibility risk is mainly test and scene-wiring churn: existing three-point Band assumptions must be migrated to ordered-polyline assumptions.
- Runtime behavior remains single-touch Slingshot input; multi-touch and runtime mouse input in player builds remain unchanged.
- Existing launch direction semantics remain unchanged: lateral Pull Offset steers opposite the lateral pull direction, while backward Pull distance drives launch power.

## Out of Scope

- Full rope physics, joints, cloth, spline physics, or physically simulated Band tension.
- Burst-optimized rope or collider solving.
- Exact arbitrary-collider silhouette, tangent, or mesh-topology wrapping.
- Compound or multi-collider Launch Target wrapping.
- Target rotation/facing/lean/spin during Active Pull or Launch.
- Haptics, audio, particles, camera transition, Feel package integration, or a designer-authored Launch Sequence.
- Retry placement beyond resetting weak/canceled Pulls to the current Rest Point.
- New Gameplay States or changes to Gameplay State transition policy.
- UI hit-test blocking policy or device DPI scaling.
- Custom inspector/editor tooling beyond existing gizmos.
- Remote issue creation or tracker publishing.

## Further Notes

- This PRD narrows the next implementation around the user-facing physical read: the player pulls the target, the Band wraps the target, the target launches from the loaded position, and the Band recoils with the target before detaching.
- Older pre-target-coupled implementation notes and tests that assert a three-point Band Shape should be treated as stale relative to this PRD.
- Assumption: designer-tuned recoil duration/curve is acceptable as the intended visual interpolation driver, but there should be no independent max-duration safety guard.
- Assumption: one explicit Collider is enough for the current player target. Compound target wrapping can be revisited after this slice is playable.
- Assumption: if same-frame Collider queries after immediate held positioning are already correct, no explicit physics transform synchronization should be added.
- Unresolved questions: none blocking. Exact padding values, wrap segment count, and recoil curve are tuning decisions for implementation and QA.
