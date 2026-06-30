# Add Held Target Pull Positioning And Launch Handoff

## Parent

[Slingshot Target-Coupled Band PRD](../../prd/prd-slingshot-target-coupled-band.md)

## Type

AFK

## User stories covered

1, 4-6, 18, 20-27, 38-40

## What to build

Make the Launch Target physically follow the interpreted Pull Point during Active Pull, and make valid Launch handoff start from that same pulled position. The Slingshot should position the already-held Launch Target after input projection/clamping and before any visual Band calculations. Weak, canceled, and invalid Pulls should return the target to the Rest Point. Valid Pull Release should keep the target at the final Pull Point, include that Pull Point in the launch request payload, and have the launcher re-apply it immediately before applying velocity.

This slice should not introduce Band Contact Points or Band Wrap yet. It establishes the gameplay positioning contract that later visual slices depend on.

## Acceptance criteria

- [x] A narrow held-positioning interface exists for positioning an already-held Launch Target.
- [x] The existing Launch Target adapter implements held positioning while remaining a shallow Unity adapter.
- [x] Held positioning fails fast when called before the target is held.
- [x] Held positioning uses immediate pose assignment suitable for same-frame visual follow-up work.
- [x] Held positioning preserves Launch Target rotation and keeps future orientation behavior behind an explicit Launch Target contract boundary.
- [x] Active Pull updates move the held Launch Target to the clamped Pull Point.
- [x] Active Pull target positioning happens after Pull projection/clamping and before later Band visual calculation hooks.
- [x] Forward-only Pull keeps the held Launch Target at Rest Point.
- [x] Weak Pull Release, canceled Pull, and invalid Pull projection return the held Launch Target to Rest Point.
- [x] Valid Pull Release keeps the held Launch Target at the final Pull Point until launch handoff.
- [x] Launch request payload includes the final clamped Pull Point as pure data.
- [x] Valid launch re-applies the request Pull Point before applying velocity.
- [x] Invalid launch request data skips both held positioning and target launch.
- [x] No unconditional physics transform synchronization is added in the hot path.

## Verification

- EditMode tests: controller moves held target to clamped Pull Point; forward-only/weak/canceled/invalid Pulls reset to Rest Point; valid release keeps final Pull Point; launch request carries Pull Point; launcher re-applies Pull Point before launch; invalid request skips positioning and launch.
- PlayMode tests: Launch Target adapter hold/position/launch behavior; held positioning preserves rotation and fails fast before hold; boundary test for same-frame pose visibility if pure tests cannot prove it.
- Static checks: Rider reformat/problems on changed files; Unity compile via Unity AI Agent Connector before tests.
- Manual Unity smoke check: pull the Slingshot in Play Mode and confirm the Launch Target follows the finger-interpreted Pull Point and launches from the pulled position.
- Package version/changelog: no package/changelog change.

## Blocked by

None - can start immediately
