---
name: task2
description: EvoTowers second-week build-and-upgrade prototype scope and validation rules. Use when planning, implementing, reviewing, or validating Week 2 systems for this Unity project: commander selection, battle state flow, wave-end pause, upgrade draft three-choice flow, upgrade pool, tower stat scaling, commander passive framework, and the first two commander archetypes.
---

# Task2

## Purpose

Use this skill to turn the Week 1 tower-defense skeleton into a build-focused prototype. Favor clear growth choices, stable wave transitions, and obvious commander identity over adding more raw content.

For the full requirement text and recommended upgrade list, read `references/week2-requirements.md`.

## Scope Guardrails

Implement only these Week 2 systems:

- Wave-end pause
- Upgrade draft three-choice flow
- Upgrade pool and upgrade roll rules
- Tower stat upgrades
- Commander selection
- Commander passive framework
- Two commanders with distinct baseline synergies
- Return from upgrade state into the next wave

Do not implement these unless the user explicitly expands scope:

- Tower branch evolution
- Bosses
- Risk-reward systems
- Meta progression
- Save system
- Multi-map support
- Complex cutscene or reward animation work

## Core Experience Targets

Week 2 should prove three things:

- Players look forward to the next upgrade choice after each wave.
- Upgrades visibly change combat, not just hidden numbers.
- The same starter towers feel different under different commanders.

If a change does not improve one of those goals, treat it as secondary.

## Required Flow

The standard in-match flow should be:

1. Choose a commander.
2. Enter the battle map.
3. Place initial towers.
4. Start the wave.
5. Wait until all enemies are killed or leaked and the wave has fully spawned.
6. Pause battle and enter upgrade state.
7. Show three upgrade cards.
8. Force the player to choose one.
9. Apply the upgrade immediately.
10. Return to prepare state.
11. Start the next wave.

## Game State

Use a small explicit state model. At minimum support:

- `Prepare`
- `Battle`
- `Upgrade`
- `GameOver`
- `Victory` as a reserved or active state

Week 2 must clearly support these transitions:

- `Battle -> Upgrade`
- `Upgrade -> Prepare`

Do not let upgrade UI trigger twice for one wave, and do not enter upgrade state before a wave is truly finished.

## Upgrade System

Build the upgrade draft from a shared data pool rather than hardcoding UI choices.

Each upgrade item should at least contain:

- `id`
- `name`
- `description`
- `rarity` as a reserved or active field
- `type`
- `target`
- `value`
- `isRepeatable`
- `commanderRestriction`

Suggested upgrade types:

- `TowerStat`
- `CommanderPassive`
- `Economy`
- `Utility`

Suggested targets:

- `ArrowTower`
- `FlameTower`
- `AllTowers`
- `Commander`
- `Global`

Use ScriptableObject-backed data where practical. Filter out upgrades that are invalid, already consumed when non-repeatable, or restricted to another commander.

## Tower Scaling Rules

Week 2 should support stable stacking for:

- damage
- attack speed
- range
- optional crit chance
- optional burn damage or duration for flame towers

Apply upgrades to towers already on the field and to towers built later in the same run.

Keep final tower values composable. Prefer a structure such as:

- base values
- global modifiers
- tower-type modifiers
- commander modifiers

This keeps Week 3 evolution work from becoming a rewrite.

## Commanders

Support exactly two commanders in Week 2:

- `Flame Warden`
- `Ranger Captain`

Each commander should be selectable before battle and then remain fixed for the run.

Baseline intent:

- `Flame Warden`: make flame towers stronger through burn pressure
- `Ranger Captain`: make arrow towers stronger through sustained output

Suggested baseline passives:

- `Flame Warden`: flame towers gain extra burn damage or longer burn duration
- `Ranger Captain`: arrow towers gain attack speed and optionally crit chance

Prefer stable always-on passives. Do not add trigger chains, stacking state machines, or kill-trigger logic in Week 2 unless the user specifically asks.

## Minimum Upgrade Pool

Target at least 12 upgrades:

- Arrow upgrades: damage, attack speed, range, crit
- Flame upgrades: damage, burn duration, burn damage, range
- Shared upgrades: all towers damage, all towers attack speed
- Economy or utility: extra gold, base health up or build cost down

Include at least four commander-leaning upgrades:

- two arrow-leaning
- two flame-leaning

The goal is to make the same card feel more valuable under one commander than the other.

## UI Expectations

Week 2 must visibly communicate progression:

- commander selection UI
- current commander info in battle
- three upgrade cards at wave end
- visible confirmation that the chosen upgrade applied
- optional debug or detail readout for tower stats if useful

Keep visuals simple if needed, but never leave players unsure whether an upgrade worked.

## Acceptance Checklist

Before calling Week 2 complete, verify:

- The player can choose one of two commanders before battle.
- The chosen commander passive stays active for the whole run.
- Every completed wave enters one upgrade draft exactly once.
- Upgrade cards come from a unified data pool.
- Choosing one upgrade applies its effects immediately.
- Existing towers and new towers both inherit the correct upgraded values.
- Arrow and flame builds feel meaningfully different between commanders.
- A full run can produce at least 3 to 5 upgrade choices.
