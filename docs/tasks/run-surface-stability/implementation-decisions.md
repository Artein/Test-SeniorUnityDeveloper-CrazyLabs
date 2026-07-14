# Run Surface Stability Implementation Decisions

Status: approved architecture; deterministic implementation complete; human feel review pending.

## Support semantics

- **Observed Support** is the selected current-fixed-tick result after probe geometry, validation, footprint sampling, normal clustering, and ranking. Its states are `Unavailable`, `Missing`, and `Supported`. It contains no cross-tick history.
- **Stable Support** is the sole gameplay grounding context. `RunSurfaceStabilityPolicy` derives it from Observed Support, owns missing-support retention and coherent normal confirmation, and publishes the shared transition.
- Movement-plane projection, grounded speed, steering-mode selection, launch/landing stabilization, and default gameplay airtime consume Stable Support or its shared transition.
- Airtime begins on `SupportLost`, ends on `SupportAcquired`, ignores a one-tick Observed miss held by Stable Support, and hard-resets without reward on `HardReset`.
- Character presentation consumes Observed Support through its existing presentation tracker. A brief Observed miss may affect presentation tracking without changing locomotion grounding; presentation owns no support grace or normal confirmation.
- The migration-only compatibility context meant Stable Support. It and the ambiguous legacy interface were removed after all consumers migrated.

## Ownership and ordering

1. `PhysicsRunSupportProbe` owns same-tick physics observation only.
2. `RunSurfaceStabilityPolicy` owns cross-tick Stable Support and transitions only.
3. `RunSteeringFramePolicy` consumes Stable Support and the shared transition; it owns ordinary-normal slew and bounded airborne steering-up memory only.
4. `RunSurfaceFramePipeline` evaluates those stages once per fixed tick and atomically publishes one immutable `RunSurfaceFrameSnapshot`.
5. Gameplay, presentation, airtime, and diagnostics read that snapshot; none may infer a competing transition.

Runtime state belongs to policy instances. No timer, candidate history, cached normal, or mutable snapshot state is stored in shared authoring assets.

## Authoring boundary

The initial migration preserves existing serialized owners:

- `GameplayPhysicsSceneCompositionMonoInstaller`: probe distance, skin width, surface layer mask, minimum support-normal dot, footprint sample offset scale, and footprint normal-cluster angle.
- `RunBodyMovementConfig`: support-loss duration, discontinuity threshold, discontinuity confirmation duration, candidate coherence angle, steering-normal slew, and airborne steering-up retention.

`RunSurfaceProbeConfig`, `RunSurfaceStabilityConfig`, and `RunSteeringFrameConfig` are immutable runtime copies. A dedicated `RunSurfaceTuning` asset is deferred until reuse or authoring pressure justifies a serialization migration.

Existing serialized steering fields retain their values through `FormerlySerializedAs` mappings:

| Previous field | Current meaning |
|---|---|
| `_runSteeringFrameSnapDegrees` | discontinuous-normal threshold, degrees |
| `_runSteeringFrameUngroundedGraceSeconds` | Stable Support loss-confirmation duration, seconds |
| `_runSteeringFrameSuspectNormalConfirmationSeconds` | coherent discontinuity confirmation duration, seconds |

## Precedence

This decision and the parent [Run Surface Probing and Stability PRD](../../prd/prd-run-surface-probing-and-stability.md) supersede the older steering-only plan wherever it assumes a raw `PhysicsRunSurfaceContextSource`, duplicates temporal filtering inside steering, or exposes ambiguous `RunSurfaceContext` ownership. The older documents remain historical rationale for steering feel.

The human owner approved this architecture in the planning conversation before implementation. Final values remain subject to issue 11's in-editor feel approval.
