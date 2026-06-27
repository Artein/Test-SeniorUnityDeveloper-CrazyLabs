---
id: ADR-0008
number: 8
title: "Use Deterministic Taut Band Shape Solver Instead Of Rope Physics"
status: approved
date: 2026-06-26
deciders: []
tags: ["slingshot", "band-shape", "physics", "testability"]
components: ["Gameplay", "Slingshot"]
supersedes: []
superseded_by: []
code_refs: ["Assets/Game/Gameplay/Slingshot/"]
test_refs: ["Assets/Game/Gameplay/Slingshot/Tests/"]
issue_refs: []
summary: "Represent natural slingshot Band Shape as a deterministic taut visual path around the Launch Target Silhouette instead of runtime rope physics."
---

# ADR-0008: Use Deterministic Taut Band Shape Solver Instead Of Rope Physics

## Summary

Represent natural slingshot **Band Shape** as a deterministic taut visual path around the held **Launch Target Silhouette**. Unity collider sampling
stays at the adapter boundary, while convex hull, tangent contact, and pulled-side wrap selection stay in plain C# for testability.

## Context

The slingshot **Band** must look like natural rubber under tension: fixed anchors, tangent **Band Contact Points**, and an ordered **Band Wrap** around
the **Pulled Side** of the held **Launch Target**. The project already treats **Band Shape** as visual deformation rather than gameplay force math, and
ADRs require gameplay/presentation logic to stay outside shallow MonoBehaviour adapters where practical.

## Decision

We will compute **Band Shape** with a deterministic taut path solver. The solver will work from an inflated convex **Launch Target Silhouette** in the
**Pull Plane**, find tangent contacts from the anchors, choose the **Pulled Side** contour, and emit ordered points for rendering.

Unity-specific collider access will remain in an adapter that samples the assigned `Collider`. The hull, tangent, and path selection logic will remain
plain C# and should be covered by focused EditMode tests. Burst or job-based optimization may be added later if profiling proves the plain C# solver is
too expensive.

Concrete first implementation decisions:

- Solver-local 2D coordinates use Rest Point as origin, Launch Frame right as X, and backward `-LaunchFrameForward` as Y.
- `ILaunchTargetSilhouetteSource` receives only Pull Plane/basis/sample-count data and writes raw world samples into caller-owned buffers.
- `SlingshotBandShapeProvider` owns reusable world and `float2` scratch buffers sized from immutable `ISlingshotConfig`; callers own the output
  Band Shape buffer.
- Raw Collider sampling is radial around the current Collider bounds center and uses `Collider.ClosestPoint` from outside origins projected onto the
  Pull Plane/Band height. The first slice supports exactly one explicitly assigned Collider and no child/compound auto-discovery.
- No unconditional `Physics.SyncTransforms` is added; add it only if a PlayMode boundary test proves same-frame Collider sampling is stale after
  held-target movement.
- `BandContactPadding` inflates the convex silhouette by offsetting hull edges outward in Pull Plane space.
- `BandSilhouetteSampleCount` controls raw Collider sampling density. `BandWrapSampleCount` controls visual contour samples, must be odd, and produces
  exactly `BandWrapSampleCount + 4` output points.
- The Pulled-Side Center comes from the actual Pull vector including lateral Pull Offset, with backward fallback only for near-zero Pull.
- Runtime geometry failures return false on the hot path; invalid config, missing references, invalid queries, and too-small buffers fail fast.

## Alternatives considered

- **Runtime rope physics or joints:** Could produce emergent motion, but adds solver instability, harder tests, and coupling to Unity physics for a
  visual problem.
- **Closest-point collider contacts:** Simple and already close to the current implementation, but nearest points are not necessarily tangent points
  and produce unnatural kinks on off-center pulls.
- **Mesh-authored wrap paths:** Gives precise art control, but makes collider support asset-specific and harder to maintain across target shapes.

## Consequences

- Positive: **Band Shape** behavior is deterministic and testable without Unity physics simulation.
- Positive: MonoBehaviours and scene adapters stay shallow while Unity `Collider` details remain isolated.
- Positive: Off-center pulls can choose tangent contacts and the **Pulled Side** wrap instead of flipping to nearest or shortest paths.
- Negative: The implementation must maintain custom 2D geometry code for silhouette sampling, convex hulls, tangent selection, and contour ordering.
- Neutral: The solver remains a visual system; launch force still comes from **Pull** distance and **Pull Offset** rules.
- Follow-up: Add separate config for silhouette sample count so geometry accuracy is not coupled to visual **Band Wrap** point count.

## Validation

- Code paths: `Assets/Game/Gameplay/Slingshot/`
- Tests or checks: implementation should add EditMode tests for hull/tangent/pulled-side selection and PlayMode adapter tests for Unity collider
  sampling.
- Review trigger: Revisit before introducing runtime rope physics, Unity joints, mesh-authored wrap paths, or Burst/job constraints into first-slice
  **Band Shape** behavior.

## Supersession

- Supersedes: None
- Superseded by: None
