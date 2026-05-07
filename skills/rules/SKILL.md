---
name: rules
description: EvoTowers tower evolution flow rules and acceptance guidance. Use when designing, implementing, reviewing, or validating the complete tower evolution chain: place base tower, gain combat experience, meet evolution requirements, open evolution UI, choose one branch, pay resources, lock the branch, change tower visuals, change attack behavior, and connect evolved branches with commander archetypes.
---

# Rules

## Purpose

Use this skill to keep tower evolution implemented as a complete player-facing chain, not as a hidden stat upgrade. The required flow is:

```text
place base tower -> gain combat experience -> meet evolution requirements -> open evolution UI -> choose branch -> pay resources -> apply branch -> change combat behavior and build identity
```

For the full Chinese design explanation and example run, read `references/tower-evolution-flow.md`.

## Core Rule

Evolution must feel like raising a tower into a strategic branch. Never treat evolution as only a damage or level number change.

Every evolved tower must change at least three visible things:

- target selection or attack cadence
- combat effect such as area damage, DOT, slow, bounce, charge, or combo
- visual or feedback signal such as color, projectile, hit effect, status marker, or evolution pulse

## Required Player Flow

Implement and validate the player flow in this order:

1. Player places only base towers at the start: Archer, Flame, and Magic.
2. Each tower owns independent level and experience state.
3. Towers gain experience from hits, kills, assists, and optionally wave survival.
4. A tower becomes evolution-ready only after meeting level or experience requirements.
5. Player selects the tower and opens an evolution choice UI.
6. UI presents exactly two branch choices for the base tower.
7. Player chooses one branch and confirms.
8. System validates gold, level/experience, branch validity, and not-yet-evolved state.
9. System spends gold, applies branch data, locks the branch, updates visuals, and closes/refreshes UI.
10. The evolved tower uses its new behavior for the rest of the run and cannot switch branches.

## Experience Rules

Use these default experience awards unless the active task defines a different balance table:

- hit normal enemy: `+1`
- kill normal enemy: `+5`
- hit elite enemy: `+2`
- kill elite enemy: `+10`
- wave-end survival reward: `+3`, optional

Use these default thresholds:

- `Lv1`: starts at `0`
- `Lv2`: unlocks at `40` total experience
- `Lv3`: unlocks at `100` total experience and enables evolution

Show progress in a way the player can understand, such as `Lv2 58/100` or a progress bar plus level text.

## Evolution Conditions

Require all of these before applying a branch:

- tower has not evolved
- branch belongs to the tower's base type
- tower is at least required level, usually `Lv3`
- tower has required experience if the branch requires it
- player has enough gold
- branch configuration is valid

When any condition fails, keep the tower unchanged and show a clear reason such as insufficient gold or level requirement.

## Branch Choice Rules

Each base tower must have two branches with different purposes:

| Base tower | Branch A | Branch B | Required contrast |
| --- | --- | --- | --- |
| Archer | Sniper | Rapid Shot | high-damage priority target vs fast sustained clear |
| Flame | Blast Flame | Burning | area burst vs refreshable DOT |
| Magic | Frost | Chain Lightning | control slow vs bounce group damage |

Choosing a branch is permanent for that tower. Other towers of the same base type may choose different branches.

## UI Rules

The tower detail panel should show:

- tower name
- level
- experience and progress
- current branch or base state
- whether evolution is available
- evolution entry when eligible or near-eligible

The evolution choice UI should show:

- current base tower name, level, and experience
- two branch cards
- branch name
- gameplay tags
- stat preview
- special behavior description
- evolve cost
- confirm and back controls

On selection, highlight the chosen branch. Disable confirm or block confirmation when resources are insufficient.

## Branch Behavior Rules

Implement these behavior identities:

- Sniper: high single-hit damage, slower attack, longer range, prefers highest-health or marked enemies, bonus damage to elites, charge feedback.
- Rapid Shot: lower per-hit damage, much faster attack, combo attack-speed stacks while staying on the same target, reset on target switch.
- Blast Flame: stronger area explosion, larger radius, center-to-edge damage falloff, obvious explosion feedback.
- Burning: lower direct damage, applies Burning DOT, refreshes duration, caps stack count.
- Frost: lower damage, applies Slowed, refreshes slow duration, optionally splashes slow in a small radius.
- Chain Lightning: bounces to nearby enemies, never repeats a target in one attack, loses damage per bounce, shows lightning connection feedback.

## Commander Rules

Commander synergy should reinforce branch identity:

- Flame Warden / Pyromancer strengthens fire branches: fire branch damage, burn duration, and Blast Flame radius.
- Ranger Captain strengthens archer branches: archer branch range, Sniper elite damage, and Rapid Shot max combo stacks.

Do not make commander synergy only a generic global damage buff. It should make the matching branch strategy feel more coherent.

## System Flow

Follow this internal flow:

```text
tower gains experience
-> recalculate level
-> check evolution readiness
-> player opens branch UI
-> player selects branchId
-> validate gold, level/exp, branch, and evolved state
-> spend gold
-> apply branch config
-> replace or switch attack behavior
-> update visual/icon/effects
-> set hasEvolved true
-> refresh tower panel
```

Prefer configuration-driven branch data, but keep the first working implementation simple enough to test.

## Acceptance Checklist

Before calling tower evolution complete, verify:

- Base towers are placed before evolution.
- A tower visibly gains experience through combat.
- A tower reaches an evolution-ready state.
- Player can inspect tower level and experience.
- Player can open an evolution UI from the selected tower.
- UI offers two meaningful branches.
- Insufficient gold prevents evolution.
- Confirming a valid branch spends gold.
- The tower cannot switch branches after evolution.
- Evolved tower visuals or feedback change.
- Evolved tower target selection or attack behavior changes.
- Commander synergy affects matching branches.
- A run can produce a clear build identity such as Ranger Captain plus Sniper/Rapid Shot or Flame Warden plus Blast Flame/Burning.
