# Run Surface Stability Implementation Issues

Parent PRD: [Run Surface Probing and Stability](../../prd/prd-run-surface-probing-and-stability.md)

Related architecture decisions:

- [ADR-0002: Keep Gameplay Logic in Plain C# Controllers](../../adr/adr-0002-keep-gameplay-logic-in-plain-csharp-controllers.md)
- [ADR-0005: Use VContainer for Dependency Injection](../../adr/adr-0005-use-vcontainer-for-dependency-injection.md)
- [ADR-0010: Use an Explicit Run Body Speed Model with Rigidbody Contact Physics](../../adr/adr-0010-use-explicit-run-body-speed-model-with-rigidbody-contact-physics.md)

## Delivery order

| ID | Title | Type | Blocked by | User stories |
|---|---|---|---|---|
| 01 | [Approve Support Semantics and Authoring Boundary](01-approve-support-semantics-and-authoring-boundary.md) | HITL | None | 18, 21–22, 45, 55, 73–75 |
| 02 | [Capture End-to-End Surface Transition Baseline](02-capture-end-to-end-surface-transition-baseline.md) | AFK | None | 56, 59–67, 70–71 |
| 03 | [Publish Atomic Observed and Compatibility-Stable Snapshot](03-publish-atomic-observed-and-compatibility-stable-snapshot.md) | AFK | 01, 02 | 20, 23–25, 30–31, 37–40, 46, 48–54, 64, 68 |
| 04 | [Stabilize Brief Missing Support in Seconds](04-stabilize-brief-missing-support-in-seconds.md) | AFK | 03 | 1–4, 7–9, 12–13, 17–19, 24–32, 36–40, 44, 56–58, 61–62, 64 |
| 05 | [Confirm Coherent Surface-Normal Discontinuities](05-confirm-coherent-surface-normal-discontinuities.md) | AFK | 04 | 5–6, 14, 33–40, 42, 59–62, 65–66 |
| 06 | [Drive Run Steering Frame from Shared Transitions](06-drive-run-steering-frame-from-shared-transitions.md) | AFK | 05 | 4–5, 10, 15–16, 41–43, 63–64 |
| 07 | [Migrate Core Locomotion to Explicit Stable Support](07-migrate-core-locomotion-to-explicit-stable-support.md) | AFK | 05 | 3–4, 8–10, 27, 39, 43–44, 52–54, 64 |
| 08 | [Apply Approved Airtime Support Semantics](08-apply-approved-airtime-support-semantics.md) | AFK | 01, 04 | 3, 10, 45, 56, 61, 64 |
| 09 | [Apply Approved Character Presentation Support Semantics](09-apply-approved-character-presentation-support-semantics.md) | AFK | 01, 03 | 11, 20, 45, 66–67 |
| 10 | [Expose Observed, Stable, Transition, and Steering Diagnostics](10-expose-observed-stable-transition-and-steering-diagnostics.md) | AFK | 06 | 20, 37–40, 66, 68 |
| 11 | [Run Seam, Trough, and Brief-Airborne Feel Review](11-run-seam-trough-and-brief-airborne-feel-review.md) | HITL | 02, 05–10 | 1–22, 56–67, 71, 73–74 |
| 12 | [Retire Legacy Surface Context and Align Documentation](12-retire-legacy-surface-context-and-align-documentation.md) | AFK | 07–11 | 46–47, 55, 68–72, 75 |

## Delivery notes

- “Observed support” means selected current-tick support after same-tick spatial resolution. “Stable support” means cross-tick locomotion context after temporal stability policy.
- Preserve the Rigidbody contact model and single movement writer required by ADR-0010.
- Keep package dependencies and project settings unchanged.
- Implementation issues must pass the Unity compile gate before targeted tests.
- A dedicated `RunSurfaceTuning` asset migration is deferred. Issue 01 records the approved authoring boundary.
- These are local planning issues only; nothing is published remotely.
