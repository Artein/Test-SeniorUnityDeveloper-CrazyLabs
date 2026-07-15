# PRD: Run Locomotion Attachment and Steering Support Separation

## Problem Statement

The Run Surface Frame Pipeline currently computes several different kinds of support information, but its published contract and downstream consumer mapping do not preserve their different meanings.

The physics probe produces current-tick Observed Support. The attachment policy derives a persistent attachment state. The stability policy may retain an earlier support sample through brief misses and may retain an earlier normal while a discontinuous candidate is being confirmed. The steering policy then derives a smooth Run Steering Frame. Despite those distinct stages, the snapshot publishes only the attachment transition and not the current attachment state, while Run Body movement and Run Air Time consume Stable Support as if it were current physical attachment.

That conflates two questions:

- May grounded locomotion forces and grounded speed policy act on the Run Body now?
- Which support orientation should steering and presentation use to remain readable through noisy samples?

Those questions have different safety properties. Grounded locomotion must fail closed when the body separates, lifts away, or exhausts a tightly bounded sensor-continuity allowance. Steering may retain or smooth an old orientation after physical detachment because a stable control frame does not itself assert contact.

The active Run Body Movement Config retains missing support for 0.12 seconds and retains an old normal for as long as 0.6 seconds while a discontinuity is confirmed. At 40 metres per second those time windows correspond to 4.8 metres and 24 metres of travel. The source-level default for one related attachment window is 0.04 seconds, so authored production behavior and source defaults also communicate materially different assumptions.

The result is speed-dependent physical behavior. The same walk-off, ramp departure, seam, or sharp transition can continue receiving grounded projection, speed-model effects, landing stabilization, and grounded airtime semantics over radically different world distances. A time-only filter that is visually harmless at 5 metres per second can become a multi-body-length physical attachment at 40 metres per second.

The current attachment policy is not yet safe to publish unchanged as the physical authority. It can preserve attachment through Missing observations while evaluating lift against the last normal; tangential walk-off can therefore produce zero or negative normal lift and retain attachment without a spatial exit condition. The current Support Distance value also has mixed meaning: cast hits describe a gap while overlap hits describe penetration. Those values cannot safely drive one separation policy without an explicit proximity contract.

The architecture already contains most of the correct seams. The defect is that the semantic boundary is incomplete: persistent attachment state is computed but discarded, retained steering support still carries grounded meaning, and consumers select the filtered signal instead of the physical signal.

The priority finding is valid. Final P1 versus P2 classification remains an evidence question: the architecture permits severe speed-dependent behavior, while release-blocking impact should be confirmed with deterministic player-facing reproductions.

## Solution

Publish one atomic Run Surface Frame Snapshot that contains four explicit, non-substitutable concepts:

1. **Observed Support** — current-tick spatial evidence selected from Unity physics queries.
2. **Run Locomotion Attachment** — the bounded gameplay estimate that alone authorizes grounded locomotion behavior.
3. **Steering Support Target** — a retained or filtered orientation target with no grounded semantics.
4. **Run Steering Frame** — the continuously usable control orientation derived from the steering target and lifecycle state.

The pipeline remains one fixed-step coordinator. It updates those concepts in dependency order and publishes the completed snapshot atomically before movement, airtime, and lost-momentum consumers run.

### Observed Support

Observed Support remains the output of same-tick validation, footprint sampling, normal clustering, and candidate selection. Its state is Supported, Missing, or Unavailable.

A Supported observation carries explicit geometric evidence:

- normalized support normal;
- support point or plane anchor;
- whether the body footprint overlaps the support volume or is separated from it;
- non-negative separation distance when separated;
- non-negative penetration depth when overlapping;
- the body position at which the observation was sampled;
- enough support identity for diagnostics and continuity preference without exposing Unity collider ownership to pure policy.

Separation and penetration are separate values. No policy compares or stores one overloaded Support Distance whose unit meaning changes by query type.

Missing means the Run Progress Frame and probe are valid but no acceptable current support was found. Unavailable means the observation cannot be trusted because required runtime context is invalid or inactive. Unavailable hard-resets temporal policy and cannot participate in grace.

### Run Locomotion Attachment

Run Locomotion Attachment is a small state machine represented by immutable values, not a hierarchy of State objects. It has Inactive, Detached, and Attached states.

Inactive is used outside an active launched run. Beginning a launched run explicitly enters Detached. A reset to Unknown or Attached is forbidden because launch flight must never receive grounded forces before landing evidence exists.

An attachment snapshot contains:

- current state;
- transition reason;
- evidence kind: Current Support, Bounded Continuity Bridge, or None;
- the physical support frame allowed for grounded projection;
- elapsed time since the last current support evidence;
- cumulative path distance since the last current support evidence;
- current separation and relative lift evidence when available;
- an explicit Allows Grounded Locomotion value derived only from attachment state.

Acquisition and reattachment require current Supported evidence, finite valid geometry, separation within the acquisition limit, and relative normal velocity that is not moving away faster than the acquisition threshold. Reattachment is stricter than continued attachment through separate acquisition and detachment thresholds, giving the policy asymmetric hysteresis without adding a reattachment delay.

While Attached, current Supported evidence updates the physical support frame immediately. A 60-degree or larger current normal change is not allowed to keep the previous normal authoritative for physics. The attachment decision uses current proximity and relative lift; steering may independently filter the orientation change.

Attachment detaches immediately when any of the following is true:

- current relative lift velocity exceeds the detachment threshold;
- current separation exceeds the detachment threshold;
- required observation context becomes Unavailable;
- current evidence is non-finite or invalid;
- a teleport or discontinuous body-position change invalidates accumulated continuity;
- Missing support reaches either its temporal or spatial continuity limit;
- the transition-sampling budget cannot establish evidence within its configured spatial guarantee.

A brief Missing observation may retain Attached only as a Bounded Continuity Bridge. That bridge requires all of the following:

- the previous state was Attached from current evidence;
- relative movement is not lifting away from the last physical support plane;
- predicted separation from the last physical support plane stays within its limit;
- elapsed missing time remains below the temporal limit;
- cumulative path distance since the last Supported observation remains below a configured fraction of the Run Body support radius;
- all evidence remains finite and the run remains active.

The bridge ends when the first bound is reached. The bounds combine with logical AND for retention, so exceeding either bound detaches. Path length is cumulative motion, not net displacement, preventing curved or oscillating motion from extending attachment. The first Missing sample includes travel since the last Supported sample.

The authored spatial limit is expressed in body radii so the rule scales with the support footprint. The policy may additionally apply a small absolute ceiling to prevent an unusually large body from authorizing a large gap. Exact tuning is selected from deterministic fixtures, not copied from the existing 0.12-second or 0.6-second filters.

Fixed-step sampling still creates an observation quantization limit. At 40 metres per second and a 0.02-second fixed step, one sample spans 0.8 metres. The implementation must therefore detach on the first Missing sample when that full displacement exhausts the spatial bridge. If product acceptance requires a strict sub-radius edge-detection bound rather than a one-sample bound, the probe must add transition-local segment sampling or an equivalent swept observation. Silent multi-metre grace is not an acceptable fallback when a sampling budget is exceeded.

### Steering Support Target

Steering Support Target replaces the physical-sounding Stable Support contract.

It may retain the last usable normal through short observation misses, confirm coherent normal discontinuities, reject one-tick spikes, and provide transition metadata for the steering frame. It does not expose Is Grounded, Allows Grounded Locomotion, or any equivalent boolean. Its support values cannot be passed to grounded movement or physical airtime APIs by type.

Steering retention and normal confirmation have their own configuration. They may be more permissive than attachment because they affect control continuity rather than contact truth. Time-based steering response remains valid where it represents player-facing response time, but long holds must also be reviewed at 5, 20, and 40 metres per second. A spatial cap is required for a retained orientation that would otherwise remain tied to a departed piece of geometry over many body lengths.

Current Supported normals may be filtered for steering while the physical attachment frame updates immediately. Detached bodies may retain a Run Steering Frame for air steering or presentation, but that retention cannot restore attachment or grounded forces.

### Consumer ownership

Run Body Movement consumes Run Locomotion Attachment for:

- whether grounded speed-model effects may run;
- whether tangent projection may use a support plane;
- whether landing stabilization may act;
- whether any grounded correction or force may be applied;
- which current or tightly bridged physical support normal is used.

Run Steering Control consumes the Run Steering Frame for heading and facing. It receives attachment separately only when choosing grounded versus air steering mode. It never infers attachment from the availability of a steering frame.

Run Air Time starts physical airtime when attachment leaves Attached and ends it when attachment returns to Attached. It does not inspect Steering Support Target.

If tiny, real detachments should not qualify for rewards, a separate Run Air Time Qualification Policy owns that product rule. It starts a provisional episode on physical detachment, discards it when duration and travel stay below qualification thresholds, and commits the complete episode from the original detach moment once it qualifies. It never changes attachment, movement, or physical airtime truth.

Presentation and diagnostics may consume Observed Support, Steering Support Target, Run Steering Frame, and attachment side by side. They must label each signal accurately and cannot call a retained steering target raw or grounded.

### Configuration ownership

Run Body Movement Config remains the initial authoring asset to avoid a new asset migration. It exposes narrow runtime interfaces for:

- attachment acquisition and detachment proximity;
- attachment lift thresholds;
- maximum continuity time;
- maximum continuity distance in body radii and any absolute ceiling;
- steering-target missing-observation retention;
- steering discontinuity angle, coherence, confirmation, and spatial cap;
- steering-frame slew and airborne retention;
- optional airtime qualification thresholds.

Current serialized values are preserved only when their meaning remains the same. Existing values whose old meaning combined physical and steering authority require an explicit migration and human tuning decision. A field is not silently reinterpreted.

### Migration approach

The migration proceeds as a sequence of behaviorally inspectable slices:

1. Characterize current movement and airtime results for the speed and geometry matrix.
2. Enrich Observed Support so proximity evidence is unambiguous.
3. Harden the attachment policy with explicit lifecycle, separation, lift, time, and path-distance rules.
4. Publish attachment state and physical support in the atomic snapshot.
5. Introduce a narrow read-only attachment source for physical consumers.
6. Rename or replace Stable Support with Steering Support Target and remove grounded semantics from its type.
7. Move grounded movement, speed, landing, and physical airtime consumers to attachment.
8. Keep steering and presentation on steering-oriented signals.
9. Add reward qualification only if product confirms that tiny physical airtime should not count.
10. Remove compatibility members after repository search and composition tests prove no physical consumer depends on retained steering support.
11. Retune production values from deterministic fixtures at 5, 20, and 40 metres per second.

The solution preserves ADR-0010: gameplay owns intentional grounded tangent-speed outcomes while Rigidbody physics continues to own gravity, contacts, collision response, separation, and externally produced normal velocity. It does not create a second movement writer or replace Unity physics.

## Unity Surfaces

Runtime assemblies and deep modules:

- Game.Gameplay remains the owning runtime assembly.
- The Run Surface Frame Pipeline remains the atomic fixed-step coordinator and facade.
- Physics Run Support Probe remains the shallow Unity physics adapter.
- Run Support Observation selection owns current-tick candidate validation and spatial evidence.
- Run Locomotion Attachment Policy becomes the sole authority for grounded locomotion permission.
- Run Steering Support Target Policy owns missing-sample retention and discontinuous-normal confirmation for control and presentation.
- Run Steering Frame Policy owns angular slew, snap, fallback, and airborne frame memory.
- Run Body Movement Controller consumes attachment and steering through separate narrow dependencies.
- Run Air Time Tracker consumes physical attachment transitions.
- A Run Air Time Qualification Policy is added only if reward qualification is accepted.
- Run Fixed Step Pipeline preserves the order Progress, Surface Frame, Movement, Air Time, and Lost Momentum.
- No new runtime asmdef is required.

Composition and lifecycle:

- VContainer registers one shared Run Surface Frame Pipeline instance through narrow read, reset, and fixed-step interfaces.
- Attachment lifecycle is explicit: inactive before a run, detached when launch begins, eligible to attach only from current evidence, and inactive again when the run ends.
- Movement, airtime, steering, diagnostics, and presentation resolve the same published snapshot for a fixed step.
- No static service, scene lookup, or second stateful implementation is introduced.

Scenes, prefabs, and serialized assets:

- Gameplay Scene continues to provide the Run Body, support collider, Run Surface mask, probe distance, and scene physics composition.
- Run Body Movement Config remains the first migration's authoring asset.
- The production asset requires intentional review because the current 0.12-second and 0.6-second values cannot remain physical defaults.
- Serialized fields are renamed with safe Unity migration metadata only where semantics remain equivalent.
- A semantic change gets a new field and an explicit asset value rather than a misleading rename.
- No Run Body prefab hierarchy, collider topology, Rigidbody mode, physics material, course mesh, hidden wall, or containment geometry change is required.
- No new ScriptableObject type is required for the first slice.
- Any production serialized-asset change is an implementation approval gate under repository rules.

Editor and developer tooling:

- No custom inspector, Editor window, importer, menu item, helper command, or RPC endpoint is required.
- Existing validation reports non-finite values, invalid thresholds, reversed acquisition and detachment bands, non-positive radius factors, and inconsistent limits.
- Diagnostics should expose observation state, proximity relation, attachment state and evidence, time and path budgets, attachment transition, steering target state, steering transition, steering frame, and consumer mode.
- Diagnostic screenshots may help visual review but are not committed test oracles.

Tests and project metadata:

- Existing Game.Gameplay EditMode and PlayMode test assemblies own the new coverage.
- Pure policies remain plain C# and are tested in EditMode without MonoBehaviour lifecycle dependence.
- Unity query behavior, scene composition, Rigidbody behavior, and deterministic geometry scenarios are tested in PlayMode.
- The existing Unity AI Agent connector is used for compile and targeted tests during implementation.
- Unity remains 6000.3.18f1.
- No package manifest, package lock, Addressables schema, ProjectSettings, build target, or Unity version change is required.
- No package version bump, installation flow, sync behavior, or package changelog entry is required.

## User Stories

1. As a player, I want walking off the same edge to detach consistently at every reachable speed, so that speed does not create invisible ground.
2. As a player, I want ramp departure to become airborne when the body lifts away, so that jump arcs remain physical.
3. As a player, I want a sharp support transition to use the surface actually beneath me, so that an old plane cannot redirect movement for metres.
4. As a player, I want brief probe noise not to make movement chatter, so that small seams remain playable.
5. As a player, I want genuine support loss not to be hidden by steering smoothing, so that airborne behavior starts honestly.
6. As a player, I want steering to remain readable for a moment after detachment, so that control does not snap because physical support ended.
7. As a player, I want grounded acceleration to stop when attachment ends, so that the game does not accelerate me in midair.
8. As a player, I want grounded slowdown to stop when attachment ends, so that air velocity remains Rigidbody-driven.
9. As a player, I want landing stabilization to act only on a real landing, so that old support cannot pull me toward a departed ramp.
10. As a player, I want physical airtime to start at detachment, so that jump timing matches motion.
11. As a player, I want tiny seam noise not to award unintended rewards, so that reward outcomes remain fair.
12. As a player, I want a qualifying jump to count from its true takeoff moment, so that reward qualification does not erase the beginning of a jump.
13. As a player, I want launch flight to start detached, so that the launch path is never treated as grounded before landing.
14. As a player, I want landing to attach promptly when valid support is reached, so that controls do not feel delayed.
15. As a player, I want repeated runs over the same geometry to produce repeatable attachment transitions, so that the course is learnable.
16. As a player, I want movement at 5, 20, and 40 metres per second to preserve the same physical meaning, so that upgrades do not alter contact rules.
17. As a player, I want one noisy normal sample not to rotate steering abruptly, so that edge contact remains comfortable.
18. As a player, I want a real bank transition to become steerable promptly, so that filtering does not make intentional geometry sluggish.
19. As a player, I want air steering orientation to remain stable without faking ground, so that control continuity and physics honesty coexist.
20. As a player, I want no hidden wall or magnetic ground force, so that leaving the Run Surface remains possible.
21. As a designer, I want physical attachment and steering support tuned independently, so that control feel cannot accidentally change grounded physics.
22. As a designer, I want attachment grace expressed in time and body-relative distance, so that its meaning survives speed changes.
23. As a designer, I want the effective distance allowance visible in metres, so that body-radius tuning is understandable in the scene.
24. As a designer, I want acquisition and detachment separation thresholds separated, so that landing can be stable without sticky departure.
25. As a designer, I want lift velocity thresholds explicit, so that ramp takeoff can be tuned independently from probe gaps.
26. As a designer, I want steering-target retention to have its own limits, so that readable control does not authorize grounded effects.
27. As a designer, I want normal confirmation separate from attachment, so that contact truth does not wait for visual smoothing.
28. As a designer, I want current physical normals accepted immediately when attachment remains valid, so that physical movement follows actual geometry.
29. As a designer, I want long steering holds reviewed by world distance at high speed, so that 0.6 seconds cannot silently mean 24 metres.
30. As a designer, I want production values derived from deterministic fixtures, so that source defaults do not become design by accident.
31. As a designer, I want zero-duration and zero-distance limits to disable continuity explicitly, so that strict attachment is a supported configuration.
32. As a designer, I want invalid cross-field combinations rejected before play, so that inverted hysteresis cannot ship.
33. As a designer, I want reward airtime qualification separate from physical airtime, so that reward feel does not alter locomotion.
34. As a designer, I want no new tuning asset in the first migration, so that the semantic fix stays focused.
35. As a designer, I want any later dedicated Run Surface tuning asset treated as a separate decision, so that serialization scope remains visible.
36. As a designer, I want the course geometry to remain the source of support, so that stability does not become invisible containment.
37. As a gameplay engineer, I want Observed Support to represent current-tick evidence only, so that temporal state has one owner.
38. As a gameplay engineer, I want Supported, Missing, and Unavailable observations, so that contact loss differs from invalid runtime context.
39. As a gameplay engineer, I want separation and penetration represented separately, so that one distance value never changes meaning.
40. As a gameplay engineer, I want a support point or plane anchor with the normal, so that predicted separation has a geometric basis.
41. As a gameplay engineer, I want observation position recorded, so that travel since last evidence is deterministic.
42. As a gameplay engineer, I want Unity collider details hidden behind the probe adapter, so that pure policies remain engine-independent.
43. As a gameplay engineer, I want attachment state published in every snapshot, so that consumers do not reconstruct it from transitions.
44. As a gameplay engineer, I want attachment transitions and current state both available, so that event and level semantics are not conflated.
45. As a gameplay engineer, I want Inactive, Detached, and Attached lifecycle states, so that pre-run and airborne states are explicit.
46. As a gameplay engineer, I want launch to enter Detached, so that lifecycle reset cannot accidentally authorize grounding.
47. As a gameplay engineer, I want only attachment to expose Allows Grounded Locomotion, so that retained steering support cannot be misused.
48. As a gameplay engineer, I want attachment evidence classified as current, bridged, or none, so that retained physical authority is diagnosable.
49. As a gameplay engineer, I want a narrow read-only attachment source, so that movement and airtime depend on the smallest correct contract.
50. As a gameplay engineer, I want attachment policy in plain C#, so that speed and geometry sequences are deterministic in EditMode.
51. As a gameplay engineer, I want an enum-and-snapshot state machine rather than a State class hierarchy, so that a small transition domain stays simple.
52. As a gameplay engineer, I want asymmetric acquisition and detachment conditions, so that attachment resists chatter without becoming sticky.
53. As a gameplay engineer, I want current support to update the physical plane immediately, so that a steering confirmation timer never delays physics.
54. As a gameplay engineer, I want positive relative lift to detach immediately, so that ramp departure cannot consume an old normal.
55. As a gameplay engineer, I want excessive separation to detach immediately, so that a nearby ray hit cannot imply contact indefinitely.
56. As a gameplay engineer, I want Unavailable to fail closed, so that invalid progress or lifecycle data cannot retain attachment.
57. As a gameplay engineer, I want non-finite evidence to fail closed, so that malformed vectors cannot authorize grounded forces.
58. As a gameplay engineer, I want Missing continuity bounded by both time and path distance, so that speed cannot scale physical grace without limit.
59. As a gameplay engineer, I want retention to require every continuity condition, so that exceeding one bound always detaches.
60. As a gameplay engineer, I want cumulative path length rather than net displacement, so that curved motion cannot evade the spatial limit.
61. As a gameplay engineer, I want the first missing step to include travel from the last supported sample, so that high-speed walk-off detaches on its first observable tick.
62. As a gameplay engineer, I want teleports to invalidate continuity, so that scene repositioning cannot bridge unrelated surfaces.
63. As a gameplay engineer, I want support-radius normalization, so that the spatial rule scales with the body footprint.
64. As a gameplay engineer, I want an absolute spatial ceiling available, so that a large body cannot authorize an implausibly large gap.
65. As a gameplay engineer, I want sampled quantization documented, so that a test does not claim precision the fixed step cannot provide.
66. As a gameplay engineer, I want transition-local segment sampling available when strict spatial precision is required, so that 40-metre-per-second cases need not accept a full-tick blind interval.
67. As a gameplay engineer, I want sampling-budget exhaustion to fail closed, so that performance protection cannot extend grounded authority.
68. As a gameplay engineer, I want Steering Support Target to carry orientation without Is Grounded, so that type shape reinforces ownership.
69. As a gameplay engineer, I want steering discontinuity confirmation to affect steering only, so that old normals cannot remain physical.
70. As a gameplay engineer, I want the Run Steering Frame to remain usable while detached, so that air steering and presentation can stay stable.
71. As a gameplay engineer, I want detached steering retention unable to modify attachment, so that dependency direction stays one-way.
72. As a gameplay engineer, I want movement to receive attachment and steering as separate inputs, so that heading and grounded permission cannot be confused.
73. As a gameplay engineer, I want the speed model gated by attachment, so that ADR-0010 grounded effects remain physically scoped.
74. As a gameplay engineer, I want tangent projection to use the attachment support frame, so that retained steering orientation cannot redefine the physical plane.
75. As a gameplay engineer, I want final facing to use the steering frame, so that visual response can remain smooth.
76. As a gameplay engineer, I want landing stabilization gated by attachment transition, so that stabilization runs exactly on physical reattachment.
77. As a gameplay engineer, I want physical airtime driven by attachment transitions, so that it records real locomotion state.
78. As a gameplay engineer, I want reward qualification downstream of physical airtime, so that reward rules cannot affect movement.
79. As a gameplay engineer, I want provisional reward airtime to commit from takeoff when qualified, so that thresholds classify an episode instead of truncating it.
80. As a gameplay engineer, I want one immutable surface-frame snapshot per fixed step, so that all consumers observe coherent state.
81. As a gameplay engineer, I want the existing fixed-step order preserved, so that attachment is ready before movement and airtime.
82. As a gameplay engineer, I want one VContainer singleton exposed through narrow interfaces, so that attachment state cannot split across registrations.
83. As a gameplay engineer, I want mutable policy state runtime-owned, so that ScriptableObjects remain authored data.
84. As a gameplay engineer, I want configuration consumed through narrow interfaces, so that policies do not depend on a broad concrete asset.
85. As a gameplay engineer, I want current serialized fields preserved only when semantics match, so that migrations do not hide behavior changes.
86. As a gameplay engineer, I want new fields for new semantics, so that an old label cannot silently acquire physical authority.
87. As a gameplay engineer, I want the fixed-tick path allocation-free, so that mobile garbage collection is unaffected.
88. As a gameplay engineer, I want Rigidbody gravity, contacts, collision response, and separation preserved, so that this remains a locomotion-policy correction rather than a physics rewrite.
89. As a gameplay engineer, I want exactly one movement writer preserved, so that attachment migration cannot introduce competing velocity authority.
90. As a gameplay engineer, I want support-relative velocity isolated behind an extensible contract, so that moving surfaces can be added later without redefining attachment.
91. As a QA engineer, I want pure tests for launch beginning Detached, so that the lifecycle safety invariant is locked.
92. As a QA engineer, I want pure tests for current support acquisition, so that valid landings attach promptly.
93. As a QA engineer, I want pure tests for stricter reattachment than continued attachment, so that asymmetric hysteresis is covered.
94. As a QA engineer, I want pure tests for lift-velocity detachment, so that ramp departure is covered without Unity scene noise.
95. As a QA engineer, I want pure tests for separation detachment, so that geometric departure is covered.
96. As a QA engineer, I want pure tests for brief Missing continuity, so that seam tolerance is intentional.
97. As a QA engineer, I want pure tests for exact temporal-bound expiry, so that threshold semantics are unambiguous.
98. As a QA engineer, I want pure tests for exact spatial-bound expiry, so that speed-dependent grace cannot regress.
99. As a QA engineer, I want pure tests where time passes slowly but distance expires quickly, so that the spatial bound is independently effective.
100. As a QA engineer, I want pure tests where distance stays small but time expires, so that the temporal bound is independently effective.
101. As a QA engineer, I want pure tests for cumulative curved travel, so that net displacement cannot extend attachment.
102. As a QA engineer, I want pure tests for Unavailable and non-finite evidence, so that fail-closed behavior is covered.
103. As a QA engineer, I want pure tests for teleport invalidation, so that lifecycle discontinuities cannot bridge support.
104. As a QA engineer, I want pure tests proving a 60-degree-plus current normal does not preserve the old physical normal, so that physical and steering frames remain separate.
105. As a QA engineer, I want pure tests proving steering can retain the old orientation after attachment detaches, so that control continuity remains supported.
106. As a QA engineer, I want tests proving Steering Support Target exposes no grounded authority, so that accidental consumer coupling is caught by compilation and contracts.
107. As a QA engineer, I want movement tests proving no grounded speed effect while Detached, so that the main player-facing defect is locked.
108. As a QA engineer, I want movement tests proving projection uses attachment support rather than steering support, so that normal confirmation cannot delay physics.
109. As a QA engineer, I want airtime tests proving detachment starts an episode immediately, so that physical timing is honest.
110. As a QA engineer, I want reward tests proving tiny episodes can be discarded without changing physical airtime, so that product qualification remains downstream.
111. As a QA engineer, I want reward tests proving qualified episodes retain their full duration, so that classification does not truncate takeoff.
112. As a QA engineer, I want deterministic walk-off tests at 5, 20, and 40 metres per second, so that edge behavior is compared in world distance.
113. As a QA engineer, I want deterministic ramp-departure tests at 5, 20, and 40 metres per second, so that lift behavior is compared across speed.
114. As a QA engineer, I want deterministic sharp-transition tests above 60 degrees at 5, 20, and 40 metres per second, so that old-normal retention cannot remain physical.
115. As a QA engineer, I want deterministic seam and short-gap tests at 5, 20, and 40 metres per second, so that noise tolerance remains bounded.
116. As a QA engineer, I want landing tests with inward, tangent, and outward relative velocity, so that reattachment conditions are explicit.
117. As a QA engineer, I want tests at 0.02-second and 0.01-second fixed steps, so that time quantization stays documented and bounded.
118. As a QA engineer, I want per-tick observation, attachment, steering target, steering frame, movement mode, and airtime assertions, so that ownership failures are localizable.
119. As a QA engineer, I want scene composition tests for shared identity and fixed-step order, so that VContainer wiring cannot silently regress.
120. As a QA engineer, I want tests for production serialized values and validation, so that the asset migration is intentional.
121. As a QA engineer, I want condition-based PlayMode completion with timeouts, so that coverage does not depend on arbitrary waits.
122. As a QA engineer, I want visual captures used only for diagnosis, so that deterministic assertions remain the committed oracle.
123. As a maintainer, I want Stable Support renamed or removed, so that its name cannot continue implying physical authority.
124. As a maintainer, I want old compatibility members deleted after migration, so that two semantic models do not coexist indefinitely.
125. As a maintainer, I want the earlier surface-stability PRD explicitly superseded where it maps physical consumers to retained support, so that documentation matches the new invariant.
126. As a maintainer, I want the steering-frame PRD retained as historical rationale, so that its control-feel decisions remain discoverable.
127. As a maintainer, I want ADR-0010 preserved, so that attachment work cannot expand into custom physics.
128. As a maintainer, I want no package or Unity-version change, so that delivery risk stays within gameplay code and assets.
129. As a reviewer, I want architecture severity separated from release severity, so that priority is based on evidence.
130. As a reviewer, I want unresolved product and serialization decisions listed explicitly, so that implementation does not bury approval gates.

## Implementation Decisions

- The core invariant is: only Run Locomotion Attachment may authorize grounded locomotion.
- Retained support orientation is a control signal and must not carry grounded authority.
- Observed Support remains current-tick evidence after same-tick spatial selection and before temporal policy.
- Observed Support uses Supported, Missing, and Unavailable states.
- A Supported observation keeps separation and penetration as distinct metrics.
- A Supported observation includes a support point or plane anchor, normal, sample position, and diagnostic continuity identity.
- Unity query types and colliders remain behind the physics probe adapter.
- Run Locomotion Attachment uses Inactive, Detached, and Attached values plus explicit transition metadata.
- Beginning a launched run enters Detached.
- Ending or invalidating a run enters Inactive and clears all continuity state.
- No attachment state authorizes grounded locomotion except Attached.
- Attachment publishes Current Support, Bounded Continuity Bridge, or None as its evidence kind.
- Acquisition requires current Supported evidence, valid geometry, acquisition-range proximity, and acceptable relative lift.
- Acquisition proximity is no looser than continued-attachment proximity.
- Current Supported evidence updates the physical support frame immediately.
- Physical support does not wait for steering discontinuity confirmation.
- Positive relative lift above threshold detaches immediately.
- Separation above threshold detaches immediately.
- Unavailable, non-finite, teleport, or exhausted sampling evidence detaches immediately.
- Missing continuity requires time, path distance, predicted separation, lift, lifecycle, and validity conditions simultaneously.
- The first Missing observation includes the full body travel since the previous Supported observation.
- Path distance is cumulative fixed-step path length, not net displacement.
- The authored spatial allowance is a fraction of support radius with an optional small absolute ceiling.
- Temporal and spatial boundaries use greater-than-or-equal expiry semantics.
- Zero allowance disables the corresponding bridge immediately.
- Retention ends when either the temporal or spatial allowance expires.
- The policy reports remaining temporal and spatial budgets for diagnostics.
- Fixed-step quantization is part of the acceptance contract.
- If a strict sub-radius edge bound is accepted, transition-local segment sampling is added behind the observation boundary.
- Transition-local sampling uses bounded work and fails closed when its budget cannot cover the travelled segment.
- Run Locomotion Attachment is an enum-and-snapshot policy, not a polymorphic State hierarchy.
- Run Steering Support Target replaces Stable Support as the filtered control-oriented concept.
- Steering Support Target contains no Is Grounded or Allows Grounded Locomotion member.
- Steering Support Target may retain missing support orientation and confirm coherent discontinuities.
- Steering retention has its own time and spatial limits.
- Steering discontinuity decisions never delay the physical support frame.
- Run Steering Frame consumes Steering Support Target and lifecycle state.
- Run Steering Frame may remain valid while attachment is Detached.
- Steering frame validity never changes attachment state.
- Movement receives attachment and steering through separate narrow interfaces.
- Grounded speed policy, grounded projection, and grounded correction are gated by attachment.
- Tangent and normal decomposition use the attachment support frame.
- Heading and facing use steering intent and Run Steering Frame, then are projected onto the physical plane only while attached.
- Air steering uses the retained Run Steering Frame without grounded speed effects.
- Landing stabilization consumes a physical attachment acquisition or reattachment transition.
- Run Air Time consumes physical attachment state and transitions.
- Reward airtime qualification, if accepted, consumes physical airtime episodes and cannot feed back into locomotion.
- A qualifying reward episode commits from its original detachment.
- The Run Surface Frame Pipeline publishes one immutable, coherent snapshot per fixed step.
- The previous complete snapshot remains visible until the next update is complete.
- The fixed-step order remains Progress, Surface Frame, Movement, Air Time, and Lost Momentum.
- One pipeline singleton is registered through narrow interfaces in VContainer.
- Runtime services depend on narrow configuration contracts rather than the concrete asset.
- ScriptableObject fields remain raw authored data; policies own mutable state and cross-field resolution.
- Fields retain serialized names only where semantics remain equivalent.
- New semantics receive new fields and explicit production values.
- Existing 0.12-second and 0.6-second values are not reused as physical attachment values without human approval.
- Validation rejects non-finite values, negative limits, invalid radius factors, and inverted acquisition-detachment bands.
- The fixed-step path remains allocation-free.
- Rigidbody remains authoritative for gravity, contacts, collision response, separation, and external normal velocity.
- The Run Body Movement Controller remains the single movement writer.
- Current surfaces are assumed static for relative-lift calculation; future support velocity enters through an observation contract.
- The first slice does not introduce moving-platform compensation.
- Characterization precedes behavior change.
- Observation enrichment precedes attachment authority migration.
- Attachment state is published before physical consumers migrate.
- Stable Support compatibility is removed only after all consumers are classified.
- Compiler-visible type separation is preferred over comments to prevent steering support from reaching physical APIs.
- Diagnostics show all four concepts together with precise labels.
- The previous Run Surface Probing and Stability decision that physical consumers use Stable Locomotion Support is superseded by this PRD.
- The previous Run Steering Frame Stability rationale for steering-only smoothing remains valid.

## Testing Decisions

Tests assert externally visible policy, consumer, and Rigidbody-boundary outcomes. They do not lock down private counters, implementation class layout, or incidental helper calls.

EditMode observation-contract tests cover:

- Supported, Missing, and Unavailable construction and validation.
- Separate gap and penetration semantics.
- Finite normalized normals and finite support anchors.
- Deterministic sample-position and path-distance inputs.
- Unity-free conversion from probe result to policy observation.

EditMode attachment-policy tests cover:

- Inactive initial state.
- Begin-launched-run transition to Detached.
- No grounded authority before Supported evidence.
- Immediate valid acquisition.
- Reattachment with inward or tangent relative velocity.
- Reattachment rejection while lifting away.
- Acquisition and detachment separation bands.
- Immediate lift detachment.
- Immediate separation detachment.
- Current support normal replacement without steering delay.
- Brief Missing Bounded Continuity Bridge.
- Exact temporal expiry.
- Exact spatial expiry.
- Time-only expiry with low path distance.
- Distance-only expiry with short elapsed time.
- First-Missing travel accounting.
- Cumulative curved and reversing path length.
- Predicted separation failure.
- Unavailable hard reset.
- Non-finite evidence failure.
- Teleport invalidation.
- Zero time and zero distance configuration.
- No repeated transition after stable state.
- Determinism at 0.02-second and 0.01-second steps.
- Sample-budget failure closed when strict spatial sampling is enabled.

EditMode steering tests cover:

- Steering target acquisition from current support.
- Brief missing-observation retention.
- Time and spatial retention expiry.
- One discontinuous spike rejection.
- Coherent discontinuity confirmation.
- Incoherent candidate replacement.
- Confirmed transition to the new target.
- Ordinary angular slew.
- Confirmed snap where configured.
- Airborne steering-frame retention.
- Lifecycle reset.
- Explicit proof that steering validity does not modify attachment.
- Explicit proof that old steering normal never becomes the physical support normal.

EditMode movement tests cover:

- Detached movement receives no grounded acceleration, slowdown, low-speed assist, soft-envelope resistance, or grounded correction.
- Attached movement receives the existing grounded speed policy.
- Physical projection uses attachment support.
- Steering heading uses Run Steering Frame.
- A discontinuous current physical normal is used immediately while steering is still filtering.
- Lift detachment prevents same-tick grounded correction.
- Exactly one final Rigidbody target-state write remains.
- Corrected surface-normal velocity ownership from ADR-0010 remains intact.
- Launch flight and detached flight preserve Rigidbody behavior.

EditMode airtime tests cover:

- Physical airtime begins on attachment loss.
- Physical airtime does not begin while a valid bounded attachment bridge remains.
- Physical airtime ends on attachment acquisition.
- Steering retention cannot suppress physical airtime.
- Unavailable lifecycle reset has documented episode behavior.
- Optional reward qualification discards an unqualified short episode without changing physical duration.
- Optional reward qualification commits a qualifying episode from its original detach moment.
- Repeated provisional episodes cannot leak state across runs.

Composition and serialization tests cover:

- One shared pipeline identity through all interfaces.
- Fixed-step pipeline order before movement and airtime.
- Explicit launch and run-end lifecycle wiring.
- Movement resolves both attachment and steering sources.
- Airtime resolves attachment rather than steering support.
- Production config values copy into the intended narrow runtime contracts.
- Invalid threshold combinations produce actionable validation.
- Legacy Stable Support compatibility is absent from physical consumers after migration.
- No second movement writer is registered.

Deterministic PlayMode fixtures cover each of these geometries:

- flat continuous support;
- straight walk-off edge;
- upward ramp departure;
- short seam;
- short unsupported gap;
- sharp normal transition greater than 60 degrees;
- landing plane;
- unavailable progress/reset case.

Each traversal fixture runs at 5, 20, and 40 metres per second where physically applicable. The canonical fixed timestep is 0.02 seconds, with focused 0.01-second variants for quantization verification.

For every speed and geometry case, capture per fixed step:

- body position and velocity;
- Observed Support state and proximity;
- attachment state, evidence, support frame, time budget, path budget, and transition;
- Steering Support Target and transition;
- Run Steering Frame;
- movement mode and whether grounded effects ran;
- physical airtime state;
- qualified reward state when enabled.

Acceptance assertions include:

- a walk-off receives no grounded effect after the first sample that exceeds the attachment spatial allowance;
- total retained physical distance is bounded by the configured body-relative allowance plus the documented observation quantization;
- positive ramp lift detaches without waiting for missing-support or normal-confirmation timers;
- a greater-than-60-degree current support change never leaves the old normal authoritative for physical projection;
- steering may remain smooth across that same transition;
- a short seam may use a bounded attachment bridge but cannot extend beyond either bound;
- the same fixture has the same state-transition meaning at all three speeds;
- no authored 0.12-second or 0.6-second steering window can authorize grounded forces;
- landing requires current supported evidence and acceptable relative lift;
- launch begins Detached and first landing acquires once;
- snapshots remain coherent within a fixed step;
- the Rigidbody still owns gravity and collision response.

Strict spatial-precision acceptance is conditional on the product choice recorded in Further Notes:

- With sampled acceptance, maximum edge uncertainty is one fixed-step travel interval and is reported explicitly.
- With strict sub-radius acceptance, transition-local segment sampling must prove unsupported-distance error stays within its configured fraction of body radius at 40 metres per second and must prove its query budget remains bounded.
- The implementation may not claim strict sub-radius accuracy while relying only on one current-position sample.

Verification order during implementation:

1. Add or update focused tests and confirm their failures match the old semantic conflation.
2. Reformat changed C# files and resolve Rider-reported problems.
3. Compile through the Unity AI Agent connector.
4. Run focused EditMode tests only after compilation is clean.
5. Run focused PlayMode tests only after focused EditMode tests pass.
6. Run neighboring movement, surface-frame, steering, airtime, composition, launch, and run-end regressions.
7. Run the full relevant test assemblies when targeted coverage is green.
8. Run diff and serialization hygiene checks.
9. Perform a human feel pass on edge, ramp, seam, and bank fixtures at all three speeds.

The planned work does not touch Jobs or Burst. If implementation scope changes to include them, Burst compile guard coverage becomes mandatory.

## Release and Compatibility

- Target Unity 6000.3.18f1 and the repository's existing dependency set.
- Preserve the Game.Gameplay assembly and current EditMode and PlayMode test assemblies.
- Preserve ADR-0010's hybrid authority: gameplay owns intentional attached tangent-speed outcomes; Rigidbody owns physical contact solving and airborne dynamics.
- Preserve one movement writer and the current fixed-step composition order.
- No Unity upgrade, package dependency, package-manifest, package-lock, Addressables, ProjectSettings, save-format, or build-target change is required.
- This is a project-level gameplay change, not a distributable package release.
- No package version bump, installation step, sync behavior, or package changelog entry is required.
- Internal gameplay contracts, test fakes, and composition registrations will change.
- Avoid a new public API; keep new contracts internal where current assembly boundaries permit.
- Any required public API change is a separate implementation approval gate.
- Existing serialized values are preserved only for unchanged steering meanings.
- New physical attachment fields require explicit production values and serialized-asset approval.
- Use safe rename metadata only for genuinely equivalent serialized fields.
- Do not reinterpret the 0.6-second discontinuity value as physical attachment grace.
- Retain compatibility adapters only long enough to migrate and verify consumers.
- Remove Stable Support grounded semantics in the same feature delivery so two authorities do not remain available.
- Update the earlier surface-stability documentation to mark its physical-consumer mapping superseded.
- Preserve the earlier steering PRD as historical control-feel rationale.
- Rollout requires deterministic fixture results and a human feel pass before production tuning is accepted.
- A feature flag is not required because parallel attachment authorities would increase risk; migration should be atomic at the consumer boundary.
- If behavior cannot be made deterministic without scene or asset expansion, stop and request approval rather than adding hidden geometry or silent defaults.
- Finding severity remains separate from solution validity. Promote to P1 only when deterministic evidence shows release-blocking player impact.

## Out of Scope

- Replacing Rigidbody with CharacterController, kinematic movement, DOTS Physics, ECS, Jobs, Burst, or a custom solver.
- Taking ownership of gravity, collision impulses, penetration resolution, arbitrary contact response, or continuous collision detection.
- Adding magnetic grounding, downforce, hidden walls, forced recentering, or artificial course containment.
- Redesigning Run Surface meshes, collider topology, physics materials, layers, Course Lip geometry, or obstacle layouts.
- Changing the explicit Run Body Speed Model, PlayerMaxSpeed meaning, steering input model, launch impulse, gravity, or one-writer invariant.
- Adding per-surface attachment profiles, material inference, terrain volumes, or course-section overrides.
- Supporting moving platforms or moving Run Surfaces in the first slice.
- Building a general contact-manifold abstraction for every collider interaction.
- Introducing a new ScriptableObject, custom inspector, Editor window, importer, debug framework, helper command, or RPC endpoint.
- Changing camera, animation, VFX, audio, haptics, UI, economy, pickups, run-end priority, or reward presentation.
- Selecting exact production attachment and steering values inside this PRD.
- Enabling reward airtime qualification without product confirmation.
- Treating visual steering stability as physical contact.
- Preserving legacy Stable Support APIs indefinitely.
- Reclassifying the finding as P1 without deterministic release-impact evidence.
- Publishing this PRD or creating remote issues.

## Further Notes

### Assumptions

- The Run Body uses one support radius of approximately 0.35 metres and the active support probe distance is approximately 0.08 metres.
- Speeds of 5, 20, and 40 metres per second are representative and 40 metres per second is reachable enough to warrant regression coverage.
- The canonical fixed timestep remains 0.02 seconds.
- Run Surfaces are static in the first implementation, so relative lift is body velocity projected onto the relevant support normal.
- Current-tick Observed Support remains the source of physical evidence.
- The existing 0.12-second missing-support and 0.6-second discontinuity values were authored for stability feel and are not accepted physical-attachment requirements.
- Steering may retain orientation while detached as long as it cannot authorize grounded behavior.
- Physical airtime and reward qualification are separate domain concepts even if the current product exposes only one tracker.
- Initial authoring remains in Run Body Movement Config.
- No new asmdef, package, scene hierarchy, or prefab is required.
- The previous surface-probing PRD is authoritative for the atomic pipeline and observation terminology except where this PRD supersedes Stable Support as physical authority.
- ADR-0010 remains fully authoritative.

### Unresolved Questions

1. Should a tiny real detachment count as physical airtime but be excluded from reward qualification, or should all physical airtime remain reward-eligible?
2. What is the largest intentional seam or trough that attachment continuity must bridge in metres and as a fraction of the 0.35-metre support radius?
3. Is one-fixed-sample edge uncertainty acceptable at 40 metres per second, or is strict sub-radius transition-local segment sampling required?
4. What exact acquisition separation, detachment separation, relative-lift, continuity-time, and body-radius distance values pass the accepted fixtures?
5. What steering-target retention and discontinuity-confirmation values preserve feel without carrying an old orientation over excessive world distance?
6. Should steering-target retention use both time and distance in the first delivery, or may pure time-based airborne frame memory remain when attachment is already Detached?
7. Does the current overlap probe provide a trustworthy plane anchor, or must the observation adapter derive one separately from penetration data?
8. Should attachment continuity require the same support identity, a geometrically coherent support plane, or merely valid nearby Run Surface evidence across seams?
9. Does any planned moving Run Surface require relative support velocity now, or can static-surface scope remain explicit?
10. Which internal interfaces may change atomically, and does any existing public or serialized API require a compatibility period?
11. Will the production serialized-asset changes be approved in the same implementation task, or should code land with neutral defaults pending a separate tuning change?
12. What deterministic reproduction threshold promotes the finding from architectural P2 to release-blocking P1?
