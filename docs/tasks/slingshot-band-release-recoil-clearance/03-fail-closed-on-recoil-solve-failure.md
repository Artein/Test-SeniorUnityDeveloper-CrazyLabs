# Fail Closed On Recoil Solve Failure

## Parent

[Slingshot Band Release Recoil Clearance PRD](../../prd/prd-slingshot-band-release-recoil-clearance.md)

## Type

AFK

## User stories covered

21-26, 30-34, 47, 51, 54-56

## What to build

Complete the unsafe-recoil failure path. If live collider-aware Band Shape solving fails before rest/idle/default Band Shape clearance is proven, keep the last valid collider-aware recoil Band Shape for that frame and retry on the next tick. Do not blend toward rest, switch to simple geometry, detach, or show detached idle while clearance is unknown.

Once clearance has been proven and visual recoil is allowed to detach, solve failure should not keep the Band chasing the Launch Target unnecessarily.

## Acceptance criteria

- [ ] A successful collider-aware recoil solve becomes the last valid collider-aware recoil Band Shape.
- [ ] If the next collider-aware recoil solve fails before clearance is proven, the view keeps the last valid collider-aware recoil Band Shape.
- [ ] Pre-clearance solve failure does not blend the last active Band Shape toward rest.
- [ ] Pre-clearance solve failure does not switch to simple two-span geometry.
- [ ] Pre-clearance solve failure does not detach or show detached rest/idle/default geometry.
- [ ] The next recoil tick retries live collider-aware solving with current Launch Target Silhouette data and the current virtual recoil Pull Point.
- [ ] Solve failure after clearance is proven still allows normal detachment instead of tethering forever.
- [ ] Tests observe behavior through public/fake dependencies or a pure policy module; no reflection is used for private controller state.

## Verification

- EditMode tests: fake provider success-then-failure keeps last valid collider-aware shape; failure before clearance does not simple-switch, rest-blend, or detach; failure after proven clearance allows normal detach.
- PlayMode tests: low-valid-pull rendered clearance regression remains green after introducing failure-path behavior.
- Static checks: Rider reformat/problems on changed code files; Unity compile via Unity AI Agent Connector before tests.
- Manual Unity smoke check: if a diagnostic failure mode is easy to force locally, confirm the Band freezes in the last safe shape for the frame rather than clipping through the target.
- Package version/changelog: no package/changelog change.

## Blocked by

- 02 - Use Clearance Gate In Low-Impulse Release Recoil
