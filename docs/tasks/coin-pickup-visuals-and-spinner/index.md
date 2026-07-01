# Coin Pickup Visuals And Spinner Implementation Issues

Parent PRD: [Coin Pickup Visuals And Spinner PRD](../../prd/prd-coin-pickup-visuals-and-spinner.md)

These local implementation issues are ordered by dependency. They are tracer-bullet slices for adding a generic **Spinner**, migrating **Coin Pickup Visuals** to project-owned mesh/material assets, protecting pickup visual contracts, running a human feel review, and removing legacy Ladybug visual dependencies from runtime coin prefabs.

## Issues

| ID | Title | Type | Status | Blocked by | User stories covered |
| --- | --- | --- | --- | --- | --- |
| 01 | [Add Generic Spinner Presentation Component](01-add-generic-spinner-presentation-component.md) | AFK | Done | None | 2, 3, 14, 15, 17-26, 36, 48 |
| 02 | [Migrate Coin Pickup Prefabs To Project-Owned Visuals](02-migrate-coin-pickup-prefabs-to-project-owned-visuals.md) | AFK | Done | 01 | 1, 4-16, 31-35, 38-45 |
| 03 | [Validate Coin Pickup Visual Contracts](03-validate-coin-pickup-visual-contracts.md) | AFK | Done | 02 | 10, 26-30, 34, 35, 37-41, 46-49 |
| 04 | [HITL Coin Visual Feel Review And Tuning](04-hitl-coin-visual-feel-review-and-tuning.md) | HITL | Done | 02 | 1-6, 12-16, 42-45 |
| 05 | [Remove Legacy Coin Visual Dependencies](05-remove-legacy-coin-visual-dependencies.md) | AFK | Done | 02, 04 | 11, 31-33, 46-50 |

## Notes

- Do not publish these remotely unless explicitly requested.
- Keep **Coin Pickup Visual** separate from **Coin Pickup** collection and reward behavior.
- Keep **Spinner** generic and reusable outside Pickups.
- Keep **Pickup** root authoring stable: trigger collider, layer, tag, and **Pickup Definition** remain root gameplay contracts.
- Keep **Big Coin Pickup** value in **Pickup Definition**; use visual scale only for presentation.
- Do not add runtime mesh/material patching.
- Run Unity compile before implementation tests on code or asset-changing slices.
