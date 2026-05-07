---
name: task4
description: EvoTowers fourth-week complete single-run loop scope and validation rules. Use when planning, implementing, reviewing, or validating Week 4 systems for this Unity project: 10-wave full run, four normal enemies, one boss, win/loss settlement, pause menu, basic audio, tutorial prompts, balance pass, wave rewards, upgrade draft timing, commander effects, tower branch evolution integration, and end-to-end playable run acceptance.
---

# Task4

## Purpose

Use this skill to turn EvoTowers from separate prototype systems into one complete playable run. Favor a stable 10-wave loop with clear win/loss outcomes over expanding content volume.

For the full condensed Week 4 requirement reference, read `references/week4-requirements.md`.

## Scope Guardrails

Implement these Week 4 systems:

- 10-wave single-run structure
- Four normal enemy roles plus one Boss
- Ordered wave spawning, wave-end detection, and wave rewards
- Victory when the final Boss wave is cleared
- Failure when base lives reach zero
- Settlement screens for win and loss
- Pause, resume, restart, return menu, and volume control
- Basic combat, wave, Boss, victory, and failure audio feedback
- Lightweight tutorial prompts
- One practical balance pass for 10-15 minute runs
- Integration of existing upgrade draft, commanders, and tower evolution into the run

Keep these out unless explicitly requested:

- Meta progression or save data
- Second map
- Large new tower sets or commander roster expansion
- Complex UI animation polish
- Advanced audio mixer tooling
- Long-form tutorial scripting

## Core Experience Targets

Week 4 should prove one complete loop:

1. Player enters the battle.
2. Player builds towers and starts Wave 1.
3. Enemies leak to damage the base or die to grant gold.
4. Waves progress from early learning into mid-run pressure.
5. Upgrade choices at fixed milestones shape the build.
6. Commander passives and tower branch evolution matter during combat.
7. Wave 10 introduces a Boss plus support enemies.
8. The run ends in a clear victory or failure settlement.

If a feature does not make the run more complete, readable, or testable, treat it as secondary.

## Recommended Run Shape

Use 10 waves for the first complete version.

| Phase | Waves | Experience goal |
| --- | --- | --- |
| Early | 1-3 | Learn placement, earn gold, build a baseline defense |
| Mid | 4-7 | Add upgrades, choose direction, introduce pressure |
| Late | 8-9 | Let evolved towers and mixed enemies test the build |
| Boss | 10 | Validate the full build and trigger settlement |

Target run length: 10-15 minutes.

Trigger upgrade drafts after Waves 3, 6, and 8.

## Required Enemy Roles

Implement at least these enemies:

- Grunt: baseline enemy, many copies, low reward.
- Runner: fast, low health, leak pressure.
- Armored: slow, high health, armor or damage reduction, higher base damage.
- Healer: restores nearby non-Boss enemies on a cooldown, simple green feedback.
- Boss: high health, slow, large base damage, appears on Wave 10.

Boss baseline behavior:

- Shows a strong warning when it appears.
- Summons support enemies when health crosses 75%, 50%, and 25%.
- Gains extra armor below 50% health.
- Does not receive normal healer value, or receives heavily reduced healing.

## Wave System Requirements

Each wave should support:

- wave id and display name
- one or more spawn groups
- enemy id, count, interval, and delay per group
- clear reward
- Boss-wave flag

The state flow should be:

`Prepare -> countdown or start -> spawning -> wait spawned enemies clear/leak -> reward -> next Prepare/Upgrade/Victory`

Never grant a wave reward before all spawn groups finish and all active enemies are resolved.

## Economy And Build Pace

Use these first-pass values unless local balance data says otherwise:

- Starting gold: 180
- Starting lives: 20
- Basic tower costs: 80-120
- Wave clear rewards: 20-60
- Normal enemy gold: 5-12
- Boss base damage: 10 or direct failure only if the run is too easy

By Wave 6-8, normal play should allow at least one tower evolution. By the Boss wave, normal play should allow roughly 2-4 evolved towers.

## Existing Systems To Integrate

Upgrade draft:

- Trigger after Waves 3, 6, and 8.
- Pause wave progression until one of three choices is selected.
- Apply choices immediately and persist them for the run.

Commanders:

- Support Flame Warden and Ranger Captain.
- Show selected commander and skill/passive state in the UI.
- Ensure passives affect relevant evolved branches.
- Add an active skill only if it can be made readable and stable; otherwise keep a clear triggered/passive effect.

Tower evolution:

- Let tower experience and evolution happen during the run, not only in test scenes.
- Ensure at least one evolution is realistically reachable by Wave 6-8.
- Verify branch value against enemy roles and Boss pressure:
  - Sniper helps against Armored and Boss.
  - Rapid Shot helps against Runner and Grunt.
  - Blast Flame helps against dense groups.
  - Burning helps sustained Boss damage.
  - Frost extends damage windows.
  - Chain Lightning handles medium-density groups.

## Settlement Screens

Victory settlement should show:

- victory title
- clear time
- completed waves
- remaining lives
- kill count
- Boss defeated
- towers built
- towers evolved
- chosen commander
- main build tags or strategy summary
- restart and return buttons

Failure settlement should show:

- failure title
- failed wave
- survival time
- whether Boss appeared
- kill count
- towers evolved
- a short actionable suggestion
- restart and return buttons

## Pause And Audio

Pause menu minimum:

- continue
- restart
- volume setting
- return to menu
- Esc toggles pause

Pause must stop gameplay time, enemy movement, tower attacks, skill cooldowns, and wave timers while keeping UI usable.

Basic audio minimum:

- button click
- tower placement
- tower attack
- enemy hit
- enemy death
- wave start
- Boss appears
- victory
- failure

Keep high-frequency sounds quiet and throttled to avoid audio clutter.

## Tutorial Prompts

Use short non-blocking prompts. Show each prompt once per run.

Recommended trigger points:

- entering battle: build the first tower
- before Wave 1: enemies follow the path; defend the goal
- after Wave 2: kills grant gold for more towers or upgrades
- first tower level-up: towers gain experience from combat
- first evolution-ready tower: choose a branch to change behavior
- after Wave 9: Boss incoming; check the defense

## Development Order

P0:

- enemy movement and tower attacks remain stable
- resource spending and kill rewards work
- 10-wave progression works
- leaks reduce lives and can fail the run
- final Boss clear can win the run

P1:

- upgrade draft timing is integrated
- commander effects are visible in the run
- tower evolution is reachable and relevant
- Boss and settlement screens complete the loop

P2:

- pause menu
- basic audio
- tutorial prompts
- balance pass

## Acceptance Checklist

Before calling Week 4 complete, verify:

- The player can enter the battle and start Wave 1.
- Enemies follow the path and leak damage the base.
- The player can build towers and spend gold.
- Towers attack enemies automatically.
- Enemy kills grant gold.
- Waves progress from 1 through 10.
- At least four normal enemy types appear.
- Wave 10 includes one Boss.
- Base lives reaching zero opens failure settlement.
- Boss defeat plus no remaining enemies opens victory settlement.
- The run is roughly 10-15 minutes.
- Upgrade choices occur mid-run and affect combat.
- Commander identity affects the run.
- At least one tower evolution is reachable during normal play.
- Evolved branches matter in late waves or Boss combat.
- Pause, resume, restart, and return controls work.
- Basic audio feedback exists for combat, wave start, Boss, victory, and failure.
- Tutorial prompts help a new player complete the first run.
