## Parent

[Run Steering Frame Stability PRD](../../prd/prd-run-steering-frame-stability.md)

## What to build

Run a human feel review on the Ladybug Rooftop Half-Tube after deterministic **Run Steering Frame** stability behavior lands.

This is the HITL gate for tuning and product judgment. The review should verify that high **Side Bank** traversal feels stable and recoverable, smooth-looking bank sections no longer produce visible steering or movement jitter, and cresting the **Course Lip** can still escape **Soft Containment**. If defaults need adjustment, tune only the steering-frame stability values from the existing steering config unless the review produces evidence of a different root cause.

This slice is complete when the team has an explicit pass/fail call for the side-bank feel and any accepted tuning changes are recorded with verification.

## Acceptance criteria

- [ ] Manual review covers center-line downhill sliding, moderate **Side Bank** riding, high **Side Bank** riding, and **Course Lip** cresting.
- [ ] Smooth-looking **Side Bank** traversal no longer flickers or jitters in a way that feels illogical to the player.
- [ ] Steering remains readable and recoverable on banked sides.
- [ ] Real **Course Lip** escape remains possible and is not blocked by hidden containment or forced recentering.
- [ ] Ordinary center-line downhill sliding feels unchanged enough that the stability fix did not over-dampen the whole run.
- [ ] Any tuning changes stay limited to **Run Steering Frame Stability** values unless a new evidence-backed issue is opened.
- [ ] If jitter persists, the next investigation is classified as steering-frame, collider geometry, camera, visual character alignment, or Rigidbody contact-resolution work.
- [ ] The final chosen tuning values are recorded in the issue result or follow-up implementation notes.

## Verification

- EditMode tests:
  - Existing targeted steering-frame stability tests should already pass before this review starts.
- PlayMode tests:
  - Optional. Add a deterministic half-tube regression fixture only if manual review shows a bug that can be captured reliably and cheaply.
- Static checks:
  - `git diff --check` after any tuning edits.
  - Unity compile through Unity AI Agent Connector if tuning assets or code are changed.
  - Run targeted tests again if tuning changes alter tested defaults.
- Manual Unity smoke check:
  - Launch into the Ladybug Rooftop Half-Tube and ride center-line, moderate side-bank, high side-bank, and course-lip paths.
  - Record whether the behavior is accepted, needs tuning, or needs a new diagnosis task.
- Package version/changelog:
  - No package manifest update expected.
  - If the project keeps gameplay release notes, describe the accepted result as stabilizing **Run Steering Frame** behavior on banked **Run Surface** traversal.

## Blocked by

- 02 - Slew Ordinary Grounded Run Surface Normal Changes
- 03 - Preserve Steering Frame Through Short Support Misses
- 04 - Confirm Or Reject Large Steering Normal Discontinuities
