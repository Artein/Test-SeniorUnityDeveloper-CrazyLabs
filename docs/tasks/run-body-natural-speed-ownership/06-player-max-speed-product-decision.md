# Player Max Speed Product Decision

Type: HITL

## Parent

`docs/prd/prd-run-body-natural-speed-ownership.md`

## What to build

Decide the product meaning of any player-facing **Player Max Speed** upgrade, stat, copy, icon, or economy entry after **Run Steering Control** stops consuming it as a velocity cap.

This is intentionally not a blocker for the movement fix. The immediate implementation should stop hidden steering-owned speed caps from shaping **Run Body** motion. This HITL issue exists so the remaining player-facing upgrade promise is not left ambiguous.

Possible outcomes include removing the upgrade, renaming it, repurposing it into launch energy, repurposing it into surface interaction, repurposing it into steering responsiveness, or explicitly deferring it with a known compatibility risk.

## Acceptance criteria

- [ ] Inventory all player-facing uses of Player Max Speed terminology, upgrade data, UI copy, and stat bindings.
- [ ] Decide whether the upgrade is removed, renamed, repurposed, or deferred.
- [ ] If removed or renamed, define the migration/cleanup scope for data, UI, tests, and assets.
- [ ] If repurposed, define the new owner of the gameplay effect and how it is visible to the player.
- [ ] If deferred, document the risk that an exposed upgrade may no longer match runtime movement behavior.
- [ ] Create follow-up implementation issue(s) if the decision requires code, asset, UI, or economy changes.

## Verification

- EditMode tests:
  - Not required for the decision itself.
- PlayMode tests:
  - Not required for the decision itself.
- Static checks:
  - Search current project terminology and upgrade definitions to support the decision.
- Manual Unity smoke check:
  - Review upgrade UI/copy if the upgrade is currently exposed.
- Package version/changelog:
  - Not required for the decision itself. Required only for any follow-up implementation that changes player-facing upgrade behavior.

## Blocked by

None - can start immediately.
