# Run Surface Stability Feel Review

Status: awaiting human in-editor approval.

## Review matrix

Run each scenario at representative low, nominal, and high run speed with surface diagnostics visible.

| Scenario | Automated invariant | Human acceptance question |
|---|---|---|
| Continuous slope | Stable normal updates continuously; steering slews at the authored rate | Does steering follow the bank smoothly without lag that feels detached? |
| Fast seam | Coherent discontinuity confirms once; no oscillation | Is the seam legible without a visible/control-frame snap that feels harsh? |
| Trough | Candidate clustering and confirmation stay deterministic | Does the body settle through the trough without flicker or prolonged wrong orientation? |
| Brief gap | Observed may be Missing while Stable remains supported | Is there any objectionable movement, steering, presentation, or reward discontinuity? |
| True walk-off | `SupportLost` occurs once after the seconds threshold | Does airborne control begin responsively and predictably? |
| Jump | Sustained absence starts airtime once | Does the transition read as a real jump rather than surface jitter? |
| Landing | `SupportAcquired` occurs once and ends airtime | Does landing regain orientation promptly without a double correction? |
| Alternating noisy normals | Incompatible candidates never falsely confirm | Are visible orientation and diagnostics free of oscillating confirmed seams? |

## Active authored values

Probe values in `GameplayPhysicsSceneCompositionMonoInstaller`:

| Field | Active value | Unit / meaning |
|---|---:|---|
| Probe distance | 0.08 | metres |
| Probe skin width | 0.02 | metres; code default for the existing scene |
| Surface mask | layer bit 256 | Run Surface selection |
| Minimum support-normal dot | 0.17 | normalized dot; code default for the existing scene |
| Footprint sample offset scale | 0.60 | collider-footprint fraction; code default for the existing scene |
| Footprint normal-cluster angle | 8 | degrees; code default for the existing scene |

Stability and steering values in `RunBodyMovementConfig`:

| Field | Active value | Unit / rationale |
|---|---:|---|
| Stable Support loss confirmation | 0.12 | seconds; preserves existing serialized steering grace pending feel review |
| Discontinuous-normal threshold | 60 | degrees; preserves existing serialized snap threshold |
| Discontinuous-normal confirmation | 0.60 | seconds; preserves existing serialized suspect confirmation |
| Candidate coherence | 1 | degrees; narrow code default prevents incompatible normals accumulating |
| Steering-normal slew | 120 | degrees/second; preserves existing serialized feel |
| Airborne steering-up retention | 0.12 | seconds; code default maintains short steering continuity without claiming grounding |

No value has been retuned from feel alone. The migration preserves serialized values and introduces explicit defaults only where the previous architecture had no equivalent field.

## Approval

Human owner must record one of:

- approved with the active values above; or
- severity-ranked follow-up findings, affected matrix rows, and requested tuning fields.

Architectural workarounds, consumer-local grace, and consumer-local normal filters are not acceptable tuning outcomes.
