# Context Map

This repository uses multiple glossary contexts. Each context defines project language for one bounded area and links related terms across areas.

## Contexts

- [Gameplay](./Assets/Game/Gameplay/CONTEXT.md) - shared run flow, gameplay states, run results, contacts, and cameras.
- [Slingshot](./Assets/Game/Gameplay/Slingshot/CONTEXT.md) - pull, band, launch-target interaction, and launch handoff language.
- [Economy](./Assets/Game/Gameplay/Economy/CONTEXT.md) - currency balances, grants, run earnings, and reward amounts.
- [Upgrades](./Assets/Game/Gameplay/Upgrades/CONTEXT.md) - upgrade ownership, progressions, modifiers, and stat resolution language.
- [Pickups](./Assets/Game/Gameplay/Pickups/CONTEXT.md) - collectible objects, pickup authoring, collection, and level pickup state.
- [Character Presentation](./Assets/Game/Gameplay/CharacterPresentation/CONTEXT.md) - character appearance, presentation modes, classification, and view-facing frames.
- [Level](./Assets/Game/Level/CONTEXT.md) - run courses, sections, pacing beats, progression bands, and containment language.

## Relationships

- **Gameplay -> Slingshot**: Gameplay owns run state and launch acceptance; Slingshot owns pull interpretation and band presentation.
- **Gameplay -> Economy**: Run results expose earned run currency; Economy owns currency meanings and balances.
- **Gameplay -> Upgrades**: Run preparation accepts upgrades; Upgrades resolve stat changes used by gameplay.
- **Gameplay -> Pickups**: Pickups report collection events; Gameplay and Economy decide accepted run rewards.
- **Gameplay -> Character Presentation**: Gameplay provides lifecycle facts; Character Presentation decides how the character appears.
- **Gameplay -> Level**: A run traverses one run course; Level language describes authored path structure and pacing intent.
