---
name: task3
description: EvoTowers third-week tower branch evolution scope and validation rules. Use when planning, implementing, reviewing, or validating Week 3 systems for this Unity project: tower experience, tower levels, branch evolution UI, 3 base towers with 6 evolved branches, branch-specific attack behavior, commander synergies for Flame Warden and Ranger Captain, status tags, hit/evolution/status feedback, and configuration-driven branch data.
---

# Task3

## Purpose

Use this skill to move EvoTowers from basic tower placement into early buildcraft. Prioritize branch choices that visibly change combat behavior, not just stat totals.

For the full requirement text, branch numbers, UI copy expectations, and test cases, read `references/week3-requirements.md`.

## Scope Guardrails

Implement these Week 3 systems:

- Tower experience and level thresholds
- Evolution eligibility from level or experience plus gold cost
- Branch selection UI from the tower detail panel
- Three base towers with two branches each
- Branch-specific target selection, attack logic, and feedback
- Commander passive synergy for fire and archer branches
- Basic status/tag support for Burning, Slowed, and Marked
- Basic hit effects, evolution effects, and readable status feedback
- Configuration-driven branch data where practical

Keep these as optional unless the user explicitly expands scope:

- Advanced particles, animation polish, and audio polish
- More commanders or more tower branches
- Boss-specific branch rules
- Save/meta progression
- Balance editor tooling
- Complex stun/vulnerable status systems

## Core Experience Targets

Week 3 should prove four things:

- A tower gains experience through real combat and becomes eligible to evolve.
- Each base tower offers two mutually exclusive branches with different play patterns.
- Evolved towers look and behave differently in combat.
- Flame Warden supports fire builds while Ranger Captain supports archer point-kill builds.

If an implementation only changes hidden numbers without changing targeting, cadence, area, status, or feedback, treat it as incomplete.

## Required Towers

Support exactly these baseline branches before adding optional content:

| Base tower | Branch A | Branch B | Difference |
| --- | --- | --- | --- |
| Archer Tower | Sniper Tower | Rapid Shot Tower | high-damage single target vs high-frequency sustained damage |
| Flame Tower | Blast Flame Tower | Burning Tower | burst area damage vs refreshable damage over time |
| Magic Tower | Frost Tower | Chain Lightning Tower | slowing control vs bouncing group damage |

## Experience And Evolution

Track experience per tower instance. Award experience from:

- hit normal enemy: `+1`
- kill normal enemy: `+5`
- hit elite enemy: `+2`
- kill elite enemy: `+10`
- optional wave-end survival reward: `+3`

Use these default levels unless local balance data already exists:

- `Lv1`: `0` exp, initial
- `Lv2`: `40` exp, small stat increase
- `Lv3`: `100` exp, unlock evolution

Evolution requires:

- base tower reaches `Lv3` or required experience
- player can pay the branch evolve cost
- tower has not already evolved

After evolution, lock the branch permanently for that tower.

## Branch Behavior

Implement behavior and feedback changes for every branch:

- Sniper Tower: prefers highest-health or marked elite targets, uses a brief charge, deals high single-hit damage, gains elite bonus damage.
- Rapid Shot Tower: attacks quickly, deals lower per-hit damage, gains attack-speed stacks while staying on the same target, resets stacks on target change.
- Blast Flame Tower: creates a larger explosion, damages multiple enemies, uses center-to-edge falloff.
- Burning Tower: applies refreshable Burning damage over time, limits burn stacks, benefits from burn duration modifiers.
- Frost Tower: applies refreshable Slowed status, slightly splashes slow to nearby enemies.
- Chain Lightning Tower: bounces between nearby enemies, never hits the same enemy twice in one attack, reduces damage per bounce.

## Commander Synergy

Use these commander identities:

- Flame Warden: fire branch commander
- Ranger Captain: archer branch commander

Flame Warden passive baseline:

- fire branch tower damage `+10%`
- Burning duration `+1s`
- Blast Flame explosion radius `+10%`

Ranger Captain passive baseline:

- archer branch tower range `+10%`
- Sniper elite bonus damage gains another `+15%`
- Rapid Shot max combo stacks `+2`

Active skills are P1. Implement only after P0 systems are stable:

- Flame Command: fire branch attack speed boost, burn refresh, next Blast Flame bonus explosion.
- Focus Mark: mark highest-health enemy, archer branches prioritize it, marked target takes extra archer branch damage.

## Tags And Statuses

Add enough structure to support future extension without overbuilding.

Suggested tower tags:

- `Physical`
- `Magic`
- `Fire`
- `Ice`
- `Lightning`
- `Archer`
- `Area`
- `SingleTarget`
- `DamageOverTime`
- `Control`

Required statuses:

- `Burning`: refresh duration; cap stack count.
- `Slowed`: refresh duration; apply movement modifier.
- `Marked`: only one primary marked target at a time.

Reserve `Stunned` and `Vulnerable` only if the implementation naturally needs placeholders.

## Data Shape

Prefer branch data assets or serializable configs over hardcoded branch logic. Each branch should expose at least:

- id
- display name
- base tower id
- branch id
- icon or visual reference
- damage multiplier or final damage
- attack speed multiplier or final attack interval
- range multiplier or final range
- attack type
- target priority
- special effects
- evolve cost
- required level
- required experience
- tags

Keep the attack implementation extensible, but do not introduce a large framework before the six required branches work.

## UI Expectations

Extend the tower detail panel with:

- current level
- current experience and progress bar
- upgrade or evolution readiness
- current branch info after evolution
- evolution entry when eligible

The branch selection UI must show:

- current base tower name, level, and experience
- two branch cards
- branch name
- gameplay tags
- stat preview
- special effect description
- evolve cost
- confirm and back controls

Disable confirmation and show insufficient-gold feedback when the player cannot pay.

## Acceptance Checklist

Before calling Week 3 complete, verify:

- At least three base tower types can evolve.
- Each base tower has exactly two available branches.
- A tower gains experience from combat.
- A tower becomes evolution-ready at Lv3 or the configured experience threshold.
- The player can open branch selection from an eligible tower.
- Insufficient gold blocks evolution clearly.
- Choosing one branch permanently evolves that tower.
- Evolved tower stats, targeting, attack behavior, and feedback change.
- Sniper prioritizes high-health or marked important targets.
- Rapid Shot attacks much faster and uses combo stack behavior.
- Blast Flame damages multiple enemies in an area.
- Burning Tower applies sustained Burning damage.
- Frost Tower slows enemies.
- Chain Lightning bounces to multiple enemies.
- Flame Warden strengthens fire branches.
- Ranger Captain strengthens archer branches.
- A playable run demonstrates different strategies from different branch choices.
