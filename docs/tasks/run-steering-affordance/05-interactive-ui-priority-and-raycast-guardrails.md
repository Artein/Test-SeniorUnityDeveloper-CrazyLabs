# Interactive UI Priority And Raycast Guardrails

Type: AFK

## Parent

`docs/prd/prd-run-steering-affordance.md`

## What to build

Add the guardrails that let future interactive Running UI claim new touches while ensuring the **Run Steering Affordance** itself never claims input.

New pointer presses that begin over interactive UI should be rejectable by Run Steering Control through a narrow, testable policy seam. Existing active steering gestures should remain captured until release or cancellation. The affordance's own CanvasGroup and Images must be excluded from interactive raycast behavior so it cannot block steering or future UI by accident.

## Acceptance criteria

- [ ] New Running steering gestures can be rejected when the pointer press begins over interactive UI.
- [ ] Existing active steering gesture movement, release, and cancellation remain captured even if the pointer later moves over UI.
- [ ] The affordance UI itself is non-interactive and does not count as interactive UI for pointer acceptance.
- [ ] CanvasGroup blocksRaycasts is false for the affordance root.
- [ ] CanvasGroup interactable is false for the affordance root.
- [ ] Every affordance Image has raycastTarget false.
- [ ] Authored mistakes are corrected where practical during validation or initialization.
- [ ] Run Steering Control still accepts ordinary non-UI Running touches.
- [ ] Pre-Launch Pull and existing UI behavior are not changed by this policy.

## Verification

- EditMode tests:
  - Pointer press marked as interactive UI does not begin a new Run Steering Control gesture.
  - Pointer press not over interactive UI begins a Run Steering Control gesture.
  - Active pointer move/release/cancel is honored even after the pointer acceptance policy would reject a new press.
  - Affordance view initialization sets CanvasGroup and Image interaction flags to non-interactive values.
- PlayMode tests:
  - EventSystem raycast smoke proves the affordance does not block or appear as an interactive hit target.
  - If a future or test-only interactive UI element exists, pointer-over-UI policy can distinguish it from the affordance visuals.
- Static checks:
  - Rider reformat and file problem inspection for changed C# files.
  - Unity connector compile before tests.
- Manual Unity smoke check:
  - Optional editor smoke: steering works when touching ordinary play area, and authored affordance visuals do not block pointer input.
- Package version/changelog:
  - Not required.

## Blocked by

- Knob-Only Serialized Affordance Tracer

