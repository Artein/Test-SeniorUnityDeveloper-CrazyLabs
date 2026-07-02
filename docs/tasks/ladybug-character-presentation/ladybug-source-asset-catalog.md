# Ladybug Source Asset Catalog

Source root: `Assets/Plugins/Ladybug/`

## First-Slice Character Basis

- Model/avatar source: `Assets/Plugins/Ladybug/Content/Geometry/Characters/Animations/Ladybug/Doamneajuta/LadyBug@TPose.FBX`
- Compatibility/avatar reference asset: `Assets/Plugins/Ladybug/Content/Geometry/Characters/Animations/Ladybug/LadyBug_Avatar.FBX`

## First-Slice Animation Mapping

| Presentation mode | Primary clip | Backup/candidate clips |
| --- | --- | --- |
| Idle | `Content/Geometry/Characters/Animations/Ladybug/LadyBug@Ilde_Breathing_FacingForward.FBX` | `LadyBug@Ilde_TurningLeftRight_FacingForward.FBX`, `LadyBug@Ilde_TurningLeftRight.FBX` |
| Slide | `Content/Geometry/Characters/Animations/Ladybug/LadyBug@Slide.FBX` | `LadyBug@Slide2.FBX` |
| Run (reserved compatibility) | `Content/Geometry/Characters/Animations/Ladybug/LadyBug@RunLoop.FBX` | `LadyBug@RunLoop2.FBX` |
| Airborne | `Content/Geometry/Characters/Animations/Ladybug/LadyBug@JumpFall.FBX` | `LadyBug@Jump.FBX`, `LadyBug@Jump2.FBX`, `LadyBug@Jump2_Fall.FBX` |
| Victory | `Content/Geometry/Characters/Animations/Ladybug/LadyBug@Victory.FBX` | none selected |
| Defeat | `Content/Geometry/Characters/Animations/Ladybug/LadyBug@Death2.FBX` | `LadyBug@Death3.FBX`, `LadyBug@Death4.FBX`, `LadyBug@DeathBehind.FBX`, `LadyBug@DeathTurning.FBX` |

## Deferred Animation Content

- Reserved Run/slide transition candidates: `LadyBug@Run_to_Slide.FBX`, `LadyBug@Run_to_Slide2.FBX`, `LadyBug@Slide_to_Run.FBX`, `LadyBug@Slide_to_Run2.FBX`, `LadyBug@Slide2_to_Run.FBX`
- Slide variants: `LadyBug@Slide_CenterRotation.FBX`
- Launch/jump variants: `LadyBug@Jump2_Fall_toRun.FBX`, `LadyBug@JumpFall_toRun.FBX`, `LadyBug@JumpRoll.FBX`, `LadyBug@JumpRollFall.FBX`, `LadyBug@JumpRollFallToRun.FBX`, `LadyBug@JumpSalt.FBX`, `LadyBug@JumpSaltFall.FBX`, `LadyBug@JumpSaltFallToRun.FBX`
- Abilities/special moves: `LadyBug@Yoyo_Start.FBX`, `LadyBug@Yoyo_Loop.FBX`, `LadyBug@Yoyo_End.FBX`, `LadyBug@StandingYoyo.FBX`, `LadyBug@Shield_Start.FBX`, `LadyBug@Shield_Loop.FBX`, `LadyBug@Shield_End.FBX`, `LadyBug@Shield.FBX`, `LadyBug@Magnet_All.FBX`, `LadyBug@WallRunLoop.FBX`, `LadyBug@WallRunLoop Right.FBX`
- Lane/avoidance moves: `LadyBug@LineChange_Right.FBX`, `LadyBug@LineChange_Rotation.FBX`, `LadyBug@LineChange_Rotation Flipped.FBX`, `LadyBug@FootBreak.FBX`, `LadyBug@SVault.FBX`, `LadyBug@SVaultFall.FBX`, `LadyBug@SVaultFlipped.FBX`

## VFX Candidates

- Slide dust: `Content/Geometry/Levels/AllLevels/FX/ParticleEffects/Slide Dust.prefab`
- Dust texture: `Content/FX/Dust_Particle.TGA`
- Wallrun: `Content/FX/Wallrun/WallrunLeftFX.prefab`, `Content/FX/Wallrun/WallrunRight FX .prefab`

VFX is deferred until presentation-facing gameplay hooks are defined. The first slice only catalogs reusable candidates.

## Audio Candidates

- Slide: `Sound Manager/Sounds/Effects/CartoonSlide.mp3`
- Footsteps: `Sound Manager/Sounds/Effects/PlayerFootSteps.wav`, `Sound Manager/Sounds/Effects/SingleStep.wav`
- Airborne/jump: `Sound Manager/Sounds/Effects/PlayerJump.wav`, `Sound Manager/Sounds/Effects/woosh1.wav`, `Sound Manager/Sounds/Effects/woosh2.wav`, `Sound Manager/Sounds/Effects/woosh3.wav`, `Sound Manager/Sounds/Effects/WooshDuck.wav`
- Defeat: `Sound Manager/Sounds/Effects/PlayerDeath.wav`
- Ladybug vocals: `Sound Manager/Sounds/Vocals/LadybugOuches/PlayerOuchLadybug1.wav`, `PlayerOuchLadybug2.wav`, `PlayerOuchLadybug3.wav`, `PlayerOuchLadybug4.wav`

Audio is deferred for a later slice so the first implementation keeps the view shallow and does not introduce one-shot gameplay vocabulary.
