# Harden Detach Lifecycle And Regression Suite

## Parent

[Slingshot Band Release Recoil Clearance PRD](../../prd/prd-slingshot-band-release-recoil-clearance.md)

## Type

AFK

## User stories covered

13, 15, 35-41, 48-56

## What to build

Harden the full detach lifecycle after the core clearance and failure paths exist. Keep weak, canceled, forward-only, and invalid Pull Release behavior unchanged. Preserve existing loaded handoff, LaunchRequested, LaunchApplied, deep-pull recoil, and post-detach query-stop semantics. Update or replace any stale tests that encode "recoil progress reached rest" as the complete detach rule.

This slice is the cleanup pass that makes the new rule coherent across the existing Slingshot regression suite without broad gameplay refactors.

## Acceptance criteria

- [ ] Weak, canceled, forward-only, and invalid Pull Releases still reset directly to capture idle/rest instead of entering Band Release Recoil.
- [ ] Existing launch request tests still prove final Pull Point, launch speed, steering, and loaded Band Shape handoff semantics.
- [ ] Existing deep-pull recoil coverage still proves live Band Shape provider usage during recoil.
- [ ] After safe detach, the Slingshot stops querying Launch Target contact/wrap geometry for recoil.
- [ ] Tests no longer treat recoil progress reaching rest as sufficient by itself for safe detachment.
- [ ] Existing fixed Band Shape buffer sizing and ordered polyline rendering remain stable.
- [ ] Scene composition does not add a new gameplay state or launch sequence step.
- [ ] No unrelated Run, camera, input, launch tuning, or Gameplay State behavior is refactored.

## Verification

- EditMode tests: targeted Slingshot controller and launch-request tests remain green; stale detach-at-rest assertions are updated to clearance-gated semantics.
- PlayMode tests: targeted Gameplay Scene natural Band Shape tests remain green, including the low-valid-pull recoil regression.
- Static checks: Rider reformat/problems on changed code files; Unity compile via Unity AI Agent Connector before tests.
- Manual Unity smoke check: basic launch flow still reaches Run normally after safe Band detach.
- Package version/changelog: no package/changelog change.

## Blocked by

- 02 - Use Clearance Gate In Low-Impulse Release Recoil
- 03 - Fail Closed On Recoil Solve Failure
