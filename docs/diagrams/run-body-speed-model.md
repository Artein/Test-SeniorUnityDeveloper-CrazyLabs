# Run Body Movement And Speed Model Diagram

This artifact gives a high-level overview of the planned **Run Body Movement Controller** and **Run Body Speed Model** split. It is intentionally separate from any PRD so future product or implementation documents can reference it without duplicating the diagram.

## Related Documents

- [ADR-0010: Use Explicit Run Body Speed Model With Rigidbody Contact Physics](../adr/adr-0010-use-explicit-run-body-speed-model-with-rigidbody-contact-physics.md)
- [PRD: Run Body Explicit Speed Ownership](../prd/prd-run-body-explicit-speed-ownership.md)
- [Speed Model Engineering Notes](./run-body-speed-model-engineering-notes.md)
- [Movement Controller Engineering Notes](./run-body-movement-controller-engineering-notes.md)
- [Speed Model Rationale](./run-body-speed-model-rationale.md)

```mermaid
flowchart TD
    subgraph Player["Player"]
        P1["Pull strength<br/>input"]
        P2["Steering touch<br/>input"]
        P3["On-screen feel<br/>observes speed, control, fairness"]
    end

    subgraph Designer["Designer"]
        D1["Launch tuning<br/>config"]
        D2["Run Body Movement Tuning<br/>RunBodyMovementConfig asset"]
        D3["Speed upgrade design<br/>PlayerMaxSpeed progression"]
        D4["Future optional surface profiles<br/>ice, snow, roof tile"]
        D5["Movement validity tuning<br/>surface-normal lift threshold"]
        D6["Inspector validation feedback<br/>invalid tuning shown before Play"]
    end

    subgraph Engineer["Engineer"]
        E1["Run Body Movement Controller<br/>fixed-step orchestration + tests"]
        E2["Run Body Speed Model<br/>plain C# speed policy + tests"]
        E3["Run Body Movement Target<br/>Rigidbody adapter/code boundary"]
        E4["Diagnostics<br/>speed, slope, contact graphs"]
        E5["Authoring validation rules<br/>pure validator + EditMode tests"]
    end

    subgraph Runtime["Runtime System"]
        R1["Launch Impulse"]
        R2["Current Rigidbody Velocity"]
        R3["Run Surface Context<br/>grounded, normal, downhill angle"]
        R4["Run Steering Input Controller<br/>gesture lifecycle + smoothing"]
        R5["Speed Decision<br/>surface-tangent accel, drag, recovery, soft envelope"]
        R6["Run Body Movement Target State<br/>velocity + optional facing"]
        R7["Unity Rigidbody Physics<br/>contacts, collisions, solver"]
        R8["Narrow config interfaces<br/>speed, validity, launch landing, steering"]
        R9["Run Gameplay Stat Resolver<br/>PlayerMaxSpeed from active run snapshot"]
        R10["Run Steering Evaluator<br/>pure direction-intent contribution"]
        R11["Run Body Speed Sanity Guard<br/>defensive velocity correction"]
        R12["Launch Landing Stabilization<br/>post-launch lift correction"]
        R13["Corrected Movement Velocity<br/>sanitized + stabilized"]
        R14["Validated Run Surface Gate<br/>HasValidGroundedRunSurface"]
        R15["Run Steering Input Metrics Resolver<br/>raw DPI to captured range/deadzone"]
        R16["RunSteeringInputState<br/>coherent per-step input snapshot"]
        R17["IRunSteeringInputSource<br/>AdvanceAndRead(fixedDeltaTime)"]
        R18["Steering Activation Gate<br/>Running + LaunchApplied"]
        R19["Gameplay State<br/>Running"]
        R20["Run Steering Frame Source/Resetter<br/>stable steering up"]
        R21["Fallback LaunchUpDirection<br/>movement-side launch fact"]
        R22["Low-Speed Assist Attempt<br/>bounded velocity budget"]
        R23["Movement Authoring Invariant Gate<br/>startup/run preparation"]
    end

    subgraph Future["Future Optional Extension"]
        F1["Terrain paint/material mapping<br/>profile selector"]
        F2["Run Surface Speed Profiles<br/>surface-specific speed knobs"]
    end

    P1 --> R1
    P2 --> R4
    R6 --> P3

    D1 --> R1
    D2 --> R8
    D3 --> R9
    D4 -. later authored as .-> F2
    D5 --> R14
    D2 --> D6
    D3 --> D6

    E1 --> E2
    R8 --> E1
    R8 --> E2
    R9 --> E1
    E2 --> R5
    R5 --> E1
    E1 --> R6
    R6 --> E3
    E4 -. observes .-> R2
    E4 -. observes .-> R3
    E4 -. observes .-> R13
    E4 -. observes .-> R14
    E4 -. observes .-> R5
    E4 -. observes .-> R6
    E4 -. observes .-> R22
    E5 --> D6
    E5 --> R23

    R1 --> R2
    R2 --> R11
    R11 --> R12
    R12 --> R13
    E1 -. arms, resets, invokes .-> R12
    R13 --> E1
    R3 --> E1
    R3 --> R12
    R13 --> R14
    R3 --> R14
    E1 -. computes .-> R14
    R14 --> E2
    R3 -. future input .-> F1
    R1 -. publishes LaunchApplied .-> R18
    R1 -. LaunchUpDirection .-> R21
    R21 --> E1
    R19 --> R18
    R18 -. enables/disables .-> R4
    E1 -. calls once per fixed pass .-> R17
    R4 -. implements .-> R17
    R17 --> R16
    R16 --> E1
    R4 -. raw DPI on gesture begin .-> R15
    R8 --> R15
    R15 --> R4
    E1 -. resets .-> R20
    R20 -. SteeringUp .-> E1
    E1 --> R10
    R10 --> E1
    R5 --> R22
    R13 --> R22
    E1 -. owns attempt state .-> R22
    R22 --> E1
    D2 --> R23
    D3 --> R23
    R23 -. validated values .-> E1
    R23 -. validated values .-> E2
    E3 --> R7
    R7 --> R2
    R7 --> R3
    F1 -. later selects .-> F2
    F2 -. later replaces or augments .-> E2
```


## Actor Responsibilities

- **Player:** supplies pull and steering input, then observes speed, control, obstacle readability, and fairness on screen.
- **Designer:** changes serialized launch, movement, validity, and upgrade configuration; receives fail-fast Inspector validation.
- **Engineer:** changes policy, orchestration, adapters, diagnostics, validation rules, and automated tests.
- **Runtime:** combines validated configuration with current Rigidbody, Run Surface, input, and active-run stat observations.
- **Unity physics:** retains gravity, contacts, collisions, separation, and external normal-velocity behavior.
