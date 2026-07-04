## Parent

[Ladybug Character Presentation PRD](../../prd/prd-ladybug-character-presentation.md)

Supersession note: [Slide-Only Character Presentation](../slide-only-character-presentation/index.md) supersedes wording in this slice that treats flat grounded movement as normal visible **Run**. **Run** remains reserved compatibility.

## What to build

Add the pure Character Presentation Mode Classifier that turns lifecycle, motion, speed, and Run Surface Context facts into a Character Presentation Classification Result. This slice should make Slide the default active grounded locomotion mode for meaningful movement, keep Run as reserved compatibility, and make Airborne a debounced mode for sustained ungrounded movement.

This slice should be testable without scene assets, Animator, Ladybug prefab, or VContainer wiring.

## Acceptance criteria

- [ ] Character Presentation Mode values exist for Idle, PullAnticipation, LaunchPush, Slide, reserved Run compatibility, Airborne, Victory, and Defeat with the agreed stable integer values.
- [ ] Character Presentation Classification Input contains current mode, current mode elapsed seconds, ungrounded elapsed seconds, lifecycle flags, Run Surface Context, course-planar speed, course-forward speed, and linear velocity.
- [ ] Character Presentation Classification Result contains the selected mode and does not expose reason strings as required runtime API.
- [ ] Terminal Victory and Defeat outrank all locomotion and pre-launch decisions.
- [ ] Pre-launch maps to Idle without a launch-specific or held-specific mode.
- [ ] Active grounded meaningful movement maps to Slide, including downhill, flat, lateral, and backward coasting.
- [ ] Flat forward grounded movement does not expose visible Run.
- [ ] Reserved Run classifier compatibility, if reached, is normalized before view application.
- [ ] Airborne is selected only after the configured ungrounded delay.
- [ ] Short ungrounded gaps preserve visible Slide for current locomotion, including reserved Run compatibility.
- [ ] Airborne exits immediately once grounded.
- [ ] Slide mode hold uses the minimum locomotion mode duration.
- [ ] Optional diagnostics, if added, log transition reasons without changing the classifier result contract.

## Verification

- EditMode tests:
  - Terminal priority chooses Victory/Defeat over active locomotion facts.
  - Pre-launch chooses Idle.
  - Grounded meaningful movement chooses Slide.
  - Grounded flat forward chooses Slide, not visible Run.
  - Backward/lateral movement chooses Slide when movement is meaningful.
  - Airborne debounce preserves Slide during short gaps and chooses Airborne after the delay.
  - Airborne exits on grounded state.
  - Slide mode hold and minimum mode duration prevent chatter around thresholds.
  - Fallback cases choose Idle.
- PlayMode tests:
  - None expected; this slice should be pure EditMode-testable.
- Static checks:
  - `git diff --check`.
  - Unity compile through the connector before tests.
- Manual Unity smoke check:
  - None expected beyond reviewing test names and threshold defaults for designer readability.
- Package version/changelog:
  - No package manifest or changelog update expected.

## Blocked by

- 03 Add Surface And Course Speed Facts
