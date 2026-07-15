# Run Locomotion Attachment Implementation Issues

Parent PRD: [Run Locomotion Attachment and Steering Support Separation](../../prd/prd-run-locomotion-attachment-and-steering-support.md)

Related architecture decisions:

- [ADR-0002: Keep Gameplay Logic in Plain C# Controllers](../../adr/adr-0002-keep-gameplay-logic-in-plain-csharp-controllers.md)
- [ADR-0005: Use VContainer for Dependency Injection](../../adr/adr-0005-use-vcontainer-for-dependency-injection.md)
- [ADR-0010: Use Explicit Run Body Speed Model With Rigidbody Contact Physics](../../adr/adr-0010-use-explicit-run-body-speed-model-with-rigidbody-contact-physics.md)

## Delivery order

| ID | Title | Type | Blocked by | User stories |
|---|---|---|---|---|
| 01 | [Capture Speed-Scaled Grounding Baseline](01-capture-speed-scaled-grounding-baseline.md) | AFK | None | 1–20, 29–30, 65–67, 91–122, 129 |
| 02 | [Approve Attachment, Precision, Airtime, and Authoring Semantics](02-approve-attachment-precision-airtime-and-authoring-semantics.md) | HITL | 01 | 21–36, 48, 65–67, 78–79, 85–86, 90, 123–130 |
| 03 | [Publish Unambiguous Support Proximity Evidence](03-publish-unambiguous-support-proximity-evidence.md) | AFK | 02 | 37–42, 80, 87, 102, 118 |
| 04 | [Publish Fail-Closed Launch-to-Landing Attachment](04-publish-fail-closed-launch-to-landing-attachment.md) | AFK | 03 | 13–15, 24–25, 43–57, 80–83, 91–95, 102–104, 116, 119 |
| 05 | [Bound Missing-Support Attachment by Time and Body Radius](05-bound-missing-support-attachment-by-time-and-body-radius.md) | AFK | 04 | 1, 4, 15–16, 22–23, 31–32, 48, 52, 58–65, 96–103, 112, 115, 117–118 |
| 06 | [Enforce Approved High-Speed Edge Precision](06-enforce-approved-high-speed-edge-precision.md) | AFK | 02, 05 | 1–3, 16, 22–23, 29, 61, 65–67, 87, 112–118, 129 |
| 07 | [Migrate Grounded Movement and Physical Airtime Atomically](07-migrate-grounded-movement-and-physical-airtime-atomically.md) | AFK | 06 | 1–16, 47, 49, 53–59, 72–77, 88–89, 107–109, 116–119, 127 |
| 08 | [Retype Filtered Support as Steering-Only and Bound Its Retention](08-retype-filtered-support-as-steering-only-and-bound-its-retention.md) | AFK | 07 | 5–6, 17–19, 21, 26–29, 36, 53, 68–75, 80–82, 104–106, 123–127 |
| 09 | [Apply Approved Airtime Reward Semantics](09-apply-approved-airtime-reward-semantics.md) | AFK | 02, 07 | 10–12, 33, 77–79, 109–111 |
| 10 | [Tune and Approve Production Edge, Ramp, and Seam Behavior](10-tune-and-approve-production-edge-ramp-and-seam-behavior.md) | HITL | 08, 09 | 1–36, 65–67, 91–122, 128–130 |
| 11 | [Retire Stable Support Compatibility and Align Documentation](11-retire-stable-support-compatibility-and-align-documentation.md) | AFK | 10 | 68, 84–86, 123–128 |

## Delivery notes

- Issue 02 is the authority for product semantics, spatial-precision acceptance, serialized authoring, compatibility, and severity decisions.
- Issue 06 implements exactly the spatial-precision mode approved in issue 02. It must not claim sub-radius precision when only fixed-step sampling is used.
- Issue 07 moves every physical grounded consumer in one slice so retained steering support and locomotion attachment never coexist as competing physical authorities.
- Issue 09 always resolves the approved reward behavior. It may implement a qualifier or verify direct physical-airtime eligibility.
- Issues 03–09 include their relevant configuration, composition, diagnostics, tests, and documentation instead of deferring those layers to horizontal follow-up work.
- Issue 10 is the required serialized-tuning and human-feel gate before compatibility removal.
- Preserve ADR-0010's Rigidbody contact authority and single movement writer throughout.
- Every code-changing issue follows the project compile gate, Rider cleanup and inspection, focused EditMode tests, then focused PlayMode tests.
- No package dependency, Unity version, ProjectSettings, Addressables, save format, package version, installation, or sync change is planned.
- These are local implementation issues only; nothing is published remotely.
