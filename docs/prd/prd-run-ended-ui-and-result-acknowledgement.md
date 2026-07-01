## Problem Statement

The current **Run Ended** phase is too transient to support the core upgrade loop. **Run End Flow** accepts a **Run Result**, transitions into
**Run Ended**, then automatically returns to **Run Preparation** after a very short delay. That gives the player no reliable moment to understand why
the run ended, how far they reached, how many coins they earned, or whether they improved over their earlier attempts in the current **Level Session**.

This is especially important for the Ladybug downhill half-tube game. The first run is not expected to reach the finish. The intended loop is: run,
collect coins, fail or finish, inspect progress, acknowledge the result, buy upgrades in **Run Preparation**, then try again. If **Run Ended** skips
straight back to **Run Preparation**, the loop loses its feedback step.

The end state also needs physical and presentation clarity. The **Launch Target** should stay at the accepted **Run Result** pose for the whole
**Run Ended** phase, while the character presentation plays **Victory** or **Defeat**. The result UI must be scene-authored like the existing
**Run Preparation UI**, not created as an arbitrary runtime hierarchy.

## Solution

Implement **Run Ended** as an acknowledged result phase instead of a timed pass-through.

When **Run End Flow** accepts a **Run Result**, the game transitions to **Run Ended**, locks the **Launch Target** at the accepted result pose, shows a
scene-authored **Run Ended UI**, and lets the existing character presentation layer play **Victory** for successful results or **Defeat** for failed
results. The UI appears when the **Run Result** is accepted; it does not wait for the terminal animation to finish.

The **Run Ended UI** presents the accepted current **Run Result**:

- coins collected during the run from the **Run Currency Snapshot**;
- whole-meter **Run Distance Display**, rounded down;
- improvement over the current **Level Session** **Best Run Distance** when the current run exceeds that best;
- success/failure title copy authored in the scene.

After a short **Run Ended Acknowledge Guard**, tapping the UI performs **Acknowledge Run Result**. **Run End Flow** then transitions from **Run Ended**
back to **Run Preparation** for both successful and failed results. The guard delays only acknowledgement input; it does not delay UI visibility or
terminal character presentation.

## Unity Surfaces

- Runtime assemblies and asmdefs:
  - `Game.Gameplay` keeps **Run End Flow** as the authority that accepts **Run Result** and controls the **RunEnded -> RunPreparation** transition.
  - `RunEndFlow` stops using the transient auto-return delay as the normal **Run Ended** exit path.
  - Add an acknowledgement boundary, for example `IRunResultAcknowledgeCommand`, so UI/presenter code requests acknowledgement without owning
    gameplay-state transitions.
  - Add scene-owned **Run Ended UI** view and presenter types following the existing `RunPreparationUIView` and `RunPreparationPresenter` pattern.
  - Add a session-scoped **Best Run Distance** service for the current **Level Session**. It is runtime-only and not save-data-backed.
  - Add a pure **Run Distance Display** formatter for whole-meter result presentation and best-run improvement text/state.
  - Add a **Run End Pose Lock** boundary around the **Launch Target** so physics motion is held during **Run Ended** and released/reset when returning
    to **Run Preparation**.
  - Reuse existing character presentation mode classification for `Victory` and `Defeat`.
- Editor assemblies, windows, inspectors, importers, or menu items:
  - No custom editor window, importer, or menu item is required.
  - Scene validation tests may inspect serialized references, but designers remain free to change layout and visual styling.
- Scenes, prefabs, ScriptableObjects, package manifests, or ProjectSettings:
  - `GameplayScene.unity` gains a serialized **Run Ended UI** hierarchy under the existing gameplay UI/canvas structure.
  - The **Run Ended UI** view has serialized references for root visibility, title, coins earned, reached distance, best-run improvement, and tap
    continue input area.
  - `GameplayLifetimeScope` gains a serialized scene-owned reference to the **Run Ended UI** view and registers it without taking ownership of the
    MonoBehaviour.
  - `RunEndConfig` may replace or repurpose the old run-ended auto-delay with a tunable **Run Ended Acknowledge Guard**, defaulting to `0.25s`.
  - No new Unity package dependency is expected.
  - No save-data schema change is expected.
- RPC/helper commands, hooks, or shell wrappers:
  - Continue using `.unity-ai-agent-connector/bin/uaiac compile` as the compile gate.
  - Run targeted EditMode and PlayMode tests through Unity AI Agent Connector after code or scene changes.
- Package versioning, changelog, and installation/sync behavior:
  - This is project gameplay/UI work, not a distributable package release.
  - No package manifest, Addressables schema, or installation behavior changes are required.

## User Stories

1. As a player, I want the game to pause on **Run Ended**, so that I can understand the result before returning to upgrades.
2. As a player, I want to see coins collected during this run, so that I know what I earned before spending upgrades.
3. As a player, I want to see how many meters I reached, so that run progress is understandable even when I fail.
4. As a player, I want to see when this run is better than my current **Level Session** best, so that small improvements feel meaningful.
5. As a player, I want failed runs to show progress instead of only failure, so that the upgrade loop feels fair.
6. As a player, I want successful runs to show a completion result, so that reaching the finish has a clear payoff.
7. As a player, I want to tap to continue from **Run Ended**, so that I control when I return to **Run Preparation**.
8. As a player, I want accidental immediate taps to be ignored briefly, so that I do not skip the result screen by mistake.
9. As a player, I want the character to stay where the run ended, so that the result screen matches the accepted run outcome.
10. As a player, I want the character to play a victory or defeat animation, so that the end state is readable without inspecting text.
11. As a designer, I want **Run Ended UI** serialized in the scene, so that I can tune layout, text objects, and visuals without runtime hierarchy code.
12. As a designer, I want success and failure title copy authored in the UI, so that result tone can be tuned without changing gameplay logic.
13. As a designer, I want meter and coin widgets wired through serialized references, so that the UI remains explicit in the hierarchy.
14. As a designer, I want **Best Run Distance** to be session-scoped, so that repeated attempts in one level session communicate improvement.
15. As a designer, I want all-time best distance deferred, so that this slice does not introduce persistence or meta-progression assumptions.
16. As a designer, I want the result UI to appear at the same time as terminal character presentation, so that the end moment feels immediate.
17. As a developer, I want **Run End Flow** to own **Acknowledge Run Result**, so that result acknowledgement and state transition stay in one place.
18. As a developer, I want presenters and views to consume accepted **Run Result** data, so that UI does not poll live physics or live run counters.
19. As a developer, I want **Run Currency Snapshot** to remain the source of earned-this-run coins, so that result UI does not display total balance.
20. As a developer, I want distance presentation to round down to whole meters, so that UI never overstates progress.
21. As a developer, I want **Best Run Distance** updated only from accepted **Run Result** values, so that rejected or live distances do not affect UI.
22. As a developer, I want the pose lock to hold the gameplay body while presentation animation can still play, so that physics and visuals have clear
    ownership.
23. As a developer, I want **Run Reward Committer** to keep granting rewards on accepted **Run Result**, so that the economy flow does not depend on UI
    acknowledgement.
24. As a developer, I want scene-owned UI registered through VContainer without owning the MonoBehaviour, so that composition remains consistent with
    existing view patterns.
25. As a tester, I want EditMode tests for acknowledgement gating, so that **Run Ended** no longer auto-exits.
26. As a tester, I want EditMode tests for run distance formatting, so that whole-meter rounding and improvement copy/state are deterministic.
27. As a tester, I want EditMode tests for session best distance, so that success and failure results can both improve the session best.
28. As a tester, I want PlayMode scene tests for serialized **Run Ended UI** references, so that missing UI wiring is caught before playtesting.
29. As a tester, I want PlayMode tests for tap acknowledgement after the guard, so that the player can always return to **Run Preparation**.
30. As a tester, I want tests to avoid exact UI layout assertions, so that designers can change the screen without editing C# tests.

## Implementation Decisions

- Use **Run Ended** for the gameplay state and **Run Ended UI** for the scene-authored result screen.
- Use **Acknowledge Run Result** for the player confirmation action in **Run Ended**.
- Do not call this action **Continue** in runtime interfaces, because **Continue** already belongs to **Run Preparation**.
- **Run End Flow** remains the gameplay owner for the accepted **Run Result** and the eventual **RunEnded -> RunPreparation** transition.
- Replace the normal `RunEnded` timed auto-return behavior with acknowledgement-driven exit.
- Keep a short **Run Ended Acknowledge Guard** before acknowledgement input is accepted. The default target is `0.25s`.
- The guard does not delay UI visibility, result data rendering, reward commit, or terminal character presentation.
- The **Run Ended UI** is scene-authored and serialized into `GameplayScene.unity`.
- Runtime must not create the **Run Ended UI** hierarchy.
- Runtime may toggle serialized UI roots and update serialized text/image references.
- Follow the scene-owned view pattern used by **Run Preparation UI** and ADR-0006.
- **Run Ended UI** presents the accepted current **Run Result**, not live run counters.
- Coins earned are read from `RunResult.RunCurrencySnapshot` / **Run Currency Snapshot**.
- Do not display total **Currency Balance** as the earned value on the result screen.
- Distance reached is read from `RunResult.DistanceTravelled`.
- **Run Distance Display** rounds down to whole meters.
- Improvement is shown only when the current displayed/run distance is greater than the previous current-session **Best Run Distance**.
- **Best Run Distance** means the greatest accepted **Run Result** distance earlier in the current **Level Session**.
- **Best Run Distance** may be improved by successful or failed runs.
- **Best Run Distance** is not all-time best distance, save-data progress, or app-session global progress.
- The first slice does not modify `PlayerEconomyState` or save schema for best-run data.
- **Run Reward Committer** can keep granting run rewards when **Run Result** is accepted; it does not need to wait for acknowledgement.
- The UI title may use designer-authored copy such as `LEVEL COMPLETE`, `RUN COMPLETE`, or `TRY AGAIN`.
- `Victory` and `Defeat` remain **Character Presentation Mode** terms, not required UI title copy.
- Existing character presentation classification should select `Victory` for successful results and `Defeat` for failed results.
- The **Run End Pose Lock** holds the **Launch Target** gameplay body at the accepted result pose for the entire **Run Ended** phase.
- The pose lock should not prevent the visual character child from playing terminal animation.
- The pose lock is released or superseded by existing reset behavior when returning to **Run Preparation**.
- Both success and failure acknowledge back to **Run Preparation** in this slice.
- No next-level, level-select, or post-victory progression flow is introduced.

## Testing Decisions

- Keep tests focused on technical contracts and observable behavior, not designer-owned layout polish.
- Add or update EditMode tests for **Run End Flow**:
  - accepting a **Run Result** transitions into **Run Ended**;
  - **Run Ended** does not auto-transition back to **Run Preparation** without acknowledgement;
  - acknowledgement before the guard does not transition;
  - acknowledgement after the guard transitions to **Run Preparation**;
  - acknowledgement is ignored outside **Run Ended**.
- Add EditMode tests for **Run Distance Display**:
  - distances round down to whole meters;
  - zero and sub-meter distances display as `0`;
  - improvement state appears only when current distance exceeds previous session best;
  - improvement is measured in meters and never rounded up.
- Add EditMode tests for session **Best Run Distance**:
  - best starts empty or zero for a new **Level Session**;
  - failed results can establish or improve best distance;
  - successful results can establish or improve best distance;
  - shorter accepted results do not reduce best distance.
- Add presenter/view-model tests for **Run Ended UI**:
  - success and failure select the expected title/state data;
  - coins earned come from **Run Currency Snapshot**;
  - total currency balance is not used for earned-this-run display;
  - stats remain visible for success and failure.
- Add scene/composition PlayMode tests:
  - `GameplayScene.unity` contains one serialized **Run Ended UI** view wired into gameplay composition;
  - serialized view references required for root, stats, and tap area are non-null;
  - entering **Run Ended** shows the UI root;
  - tapping after the guard returns to **Run Preparation**;
  - Play Mode does not create a duplicate result UI hierarchy at runtime.
- Add pose-lock tests at the lowest practical level:
  - pure interface/unit coverage if the lock is implemented as a plain service;
  - PlayMode coverage if Rigidbody state is required to prove the **Launch Target** stays held during **Run Ended**.
- Keep or update character presentation tests:
  - successful accepted result selects `Victory`;
  - failed accepted result selects `Defeat`;
  - terminal presentation remains active during **Run Ended**.
- Avoid automated assertions for exact title copy, font sizes, anchoring, widget positions, animation clip duration, or final UI art.
- Verification order after implementation:
  - reformat changed C# files with Rider tools when available;
  - `.unity-ai-agent-connector/bin/uaiac compile`;
  - targeted EditMode tests for **Run End Flow**, **Run Distance Display**, session best distance, and presenters;
  - targeted PlayMode tests for `GameplayScene` composition, **Run Ended UI**, pose lock, and character presentation regressions.

## Release and Compatibility

- This change is backward compatible with existing player save data.
- No new persistent best-distance field is introduced in this slice.
- Existing run reward persistence remains based on accepted **Run Result** and **Run Currency Snapshot**.
- Existing upgrade purchase flow in **Run Preparation** remains the next step after acknowledgement.
- Scene serialization changes are expected because **Run Ended UI** must be authored in `GameplayScene.unity`.
- Existing `RunEndConfig` serialized fields may need a small migration or renamed field if `RunEndedDelay` becomes **Run Ended Acknowledge Guard**.
- This does not require a Unity version upgrade, new package dependency, or Addressables schema change.

## Out of Scope

- Persisted all-time best distance.
- Cross-level or app-session best-distance tracking.
- Level completion routing to a next level, level select, or victory campaign screen.
- Redesigning **Run Preparation UI** or the upgrade system.
- Changing coin pickup collection rules or reward commit timing beyond presenting earned-this-run coins.
- Runtime-generated result UI hierarchy.
- New animation assets, unless the existing `Victory` or `Defeat` clips are missing or broken.
- Haptics, audio stingers, particles, analytics, or telemetry.
- Exact result-screen art direction, typography, or final copy polish.
- Terrain, obstacle, pickup, or level-layout changes.

## Further Notes

- The slice should respect ADR-0002 by keeping gameplay decisions in plain C# services/controllers and MonoBehaviours shallow.
- The slice should respect ADR-0003 by using direct C# events before introducing a broader event bus.
- The slice should respect ADR-0005 by composing runtime services through VContainer.
- The slice should respect ADR-0006 by registering scene-owned UI views without treating them as owned disposable services.
- A new ADR is not required for this slice unless the team decides to persist best-run data or introduce a broader post-run progression model.
- Default **Run Ended Acknowledge Guard** assumption: `0.25s`, designer-tunable through existing config patterns.
- Final UI copy and art can remain designer-owned as long as required serialized references are present.
