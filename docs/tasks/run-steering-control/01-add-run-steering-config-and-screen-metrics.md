## Parent

[Run Steering Control PRD](../../prd/prd-run-steering-control.md)

## What to build

Make the Run Steering Control's physical input policy authorable and testable. Gameplay configuration should expose Run Steering Range in centimeters, Run Steering Deadzone as a fraction of range, Run Steering Responsiveness under the corrected name, fallback DPI, and accepted raw-DPI bounds.

The screen abstraction should expose raw DPI as a Unity fact. Gameplay steering code should own validation and fallback policy so the screen adapter remains a thin raw-data wrapper.

This slice should also remove the old active sensitivity multiplier from the steering model and intentionally update the current steering configuration asset to the agreed baseline values.

## Acceptance criteria

- [ ] The screen abstraction exposes raw DPI alongside width and height.
- [ ] The Unity screen adapter returns Unity's raw DPI value without applying gameplay fallback policy.
- [ ] Player steering configuration exposes Run Steering Range in centimeters.
- [ ] Player steering configuration exposes Run Steering Deadzone as a fraction of Run Steering Range.
- [ ] Player steering configuration exposes fallback DPI.
- [ ] Player steering configuration exposes minimum and maximum accepted raw DPI.
- [ ] The initial authored values are `1.5cm` range, `0.15` deadzone fraction, fallback DPI `326`, minimum accepted DPI `1`, and maximum accepted DPI `1000`.
- [ ] `SteeringResponseRate` is renamed to Run Steering Responsiveness in code-facing configuration.
- [ ] The old steering sensitivity multiplier is removed from active steering configuration.
- [ ] No serialized migration shim or `[FormerlySerializedAs]` is added for the old steering fields.
- [ ] Existing movement speed and turn-rate configuration remains separate from input mapping configuration.

## Verification

- EditMode tests:
  - Raw DPI `96` is treated as an accepted value by gameplay validation.
  - Raw DPI `0` uses fallback DPI.
  - Negative DPI uses fallback DPI.
  - NaN DPI uses fallback DPI.
  - Infinite DPI uses fallback DPI.
  - Raw DPI greater than `1000` uses fallback DPI.
  - Centimeters convert to pixels using the validated DPI.
- PlayMode tests:
  - Not expected for this slice unless existing composition requires runtime validation.
- Static checks:
  - `git diff --check`.
  - Unity compile through Unity AI Agent Connector.
  - Source search confirms no active steering code still reads the old steering sensitivity multiplier.
  - Source search confirms the old response-rate name is not used as the active config API.
- Manual Unity smoke check:
  - Inspect the steering configuration asset and confirm the new authoring fields and baseline values are visible.
- Package version/changelog:
  - No package manifest or changelog update expected.

## Blocked by

None - can start immediately
