# EvoTowers Week 2 Requirements

## Goal

Week 2 should not expand raw content count. The goal is to turn the Week 1 tower-defense skeleton into a prototype with build identity.

The single core objective is:

- after each wave, the player should reliably enter one upgrade choice
- upgrades should visibly alter tower or commander performance
- different commanders should push the same basic towers toward different playstyles

## Scope

Build only:

- wave-end pause
- three-choice upgrade flow
- upgrade pool and upgrade draw rules
- tower stat upgrades
- commander selection
- commander passive framework
- two basic commander synergies
- return from upgrade state into the next wave

Do not build:

- tower branch evolution
- bosses
- risk-reward systems
- meta progression
- save system
- multi-map support
- complex cutscene animation

## Core Experience Targets

Week 2 must prove:

1. Players expect and want the next upgrade choice after each wave.
2. Upgrades affect combat in ways players can feel.
3. The same base towers behave differently under different commanders.

## Standard Match Flow

The intended loop is:

1. Choose a commander.
2. Enter the battle map.
3. Place initial towers.
4. Start the wave.
5. Let enemies finish dying or leaking.
6. End the wave and pause combat.
7. Show three upgrade choices.
8. Force one selection.
9. Apply the upgrade immediately.
10. Return to prepare state.
11. Start the next wave.

## Wave-End Pause System

At wave end, battle must pause and move into an upgrade selection state.

Wave end is valid only when:

- all enemies from that wave have spawned
- every spawned enemy is either dead or leaked

At wave end:

- stop further enemy spawning
- pause battle time or battle interactions
- disable or restrict tower placement input
- show the upgrade draft UI

Use a small state machine. Suggested states:

- `Prepare`
- `Battle`
- `Upgrade`
- `GameOver`
- `Victory` reserved or active

Week 2 must at minimum support:

- `Battle -> Upgrade`
- `Upgrade -> Prepare`

Acceptance:

- each wave triggers the upgrade UI exactly once
- no duplicate popup
- no premature upgrade entry

## Upgrade Draft UI

Each wave end should present exactly three clickable upgrade cards.

The UI should include:

- upgrade title
- upgrade description
- three selectable cards
- forced choice; no skipping
- immediate close after one valid selection

Each card should show:

- name
- type
- short description

Example:

- `Rapid Arrows`
- `Tower Upgrade`
- `Arrow Towers gain +25% attack speed`

Interaction rules:

- click one card to choose it
- apply the effect immediately
- close the panel after success
- block repeat selection

Acceptance:

- every draft shows three valid options
- only one option can be chosen
- chosen effects immediately alter battle logic

## Upgrade Pool Data

Upgrade UI must draw from data, not hardcoded UI entries.

Each upgrade item should at minimum contain:

- `id`
- `name`
- `description`
- `rarity`
- `type`
- `target`
- `value`
- `isRepeatable`
- `commanderRestriction`

Suggested types:

- `TowerStat`
- `CommanderPassive`
- `Economy`
- `Utility`

Suggested targets:

- `ArrowTower`
- `FlameTower`
- `IceTower` reserved if useful
- `AllTowers`
- `Commander`
- `Global`

Recommended source:

- ScriptableObject assets

Target pool size:

- 12 to 18 upgrades

Suggested composition:

- 3 to 4 arrow upgrades
- 3 to 4 flame upgrades
- 3 to 4 general tower upgrades
- 2 to 4 commander-linked upgrades
- 1 to 2 economy or utility upgrades

Draw rules:

- randomly draw 3 valid upgrades
- no duplicates in one offer
- already owned non-repeatables are removed
- commander-restricted upgrades only appear for matching commanders

Acceptance:

- upgrade choices are data-driven
- different runs and waves produce different combinations
- no invalid upgrade should appear

## Tower Stat Upgrade Rules

Support these basic upgrade outputs:

- damage up
- attack speed up
- range up
- optional crit chance up
- optional burn damage up
- optional burn duration up

Supported targets:

- `ArrowTower`
- `FlameTower`
- `AllTowers`

Rules:

- existing towers update immediately
- future towers inherit all active modifiers
- multiple upgrades stack

Recommended value composition:

- base values
- global modifiers
- tower-type modifiers
- commander modifiers

Acceptance:

- arrow attack speed upgrades immediately affect current arrow towers
- newly built arrow towers also inherit the change
- upgrades stack cleanly

## Commander Selection

Choose one commander at the start of the run.

Only implement two commanders:

1. `Flame Warden`
2. `Ranger Captain`

Selection UI should include:

- portrait or placeholder image
- commander name
- short passive description
- selection button
- handoff into battle after selection

Commander intent should be obvious:

- `Flame Warden`: fire towers deal stronger burn pressure
- `Ranger Captain`: arrow towers gain attack speed and crit potential

Acceptance:

- one commander can be selected before battle
- the chosen commander remains fixed for the run
- current commander info is visible during battle

## Commander Passive Framework

Do not hardcode commander identity directly inside tower scripts without a reusable framework.

A commander passive should be able to describe:

- unique id
- name and description
- affected tower types
- effect type
- effect value
- whether upgrades can amplify it

Week 2 should only use stable passive effects, not complex event chains.

Avoid for now:

- kill triggers
- chain reactions
- conditional stacks
- complex commander state logic

## Commander Definitions

### Flame Warden

Role:

- push flame towers through burn synergy

Recommended simple baseline:

- flame tower damage +15%
- or flame tower burn duration +50%
- or flame towers apply a baseline burn DOT automatically

Preferred Week 2 version:

- flame tower hits apply a fixed burn DOT

Player-facing goal:

- flame towers feel clearly more valuable under this commander
- higher-health enemies feel more vulnerable to fire builds

### Ranger Captain

Role:

- push arrow towers through fast sustained damage

Recommended simple baseline:

- arrow tower attack speed +15%
- optional small crit chance boost

Preferred Week 2 version:

- arrow towers gain attack speed
- optional light crit support

Player-facing goal:

- arrow towers clear small enemies faster
- players feel rewarded for leaning into arrow builds

## Commander And Upgrade Synergy

Week 2 must create cases where the same upgrade has different value under different commanders.

Examples:

- `Arrow Tower Attack Speed +25%` is stronger under `Ranger Captain`
- `Flame Tower Burn Damage +30%` is stronger under `Flame Warden`

Minimum requirement:

- at least four clearly commander-biased upgrades
- two arrow-leaning
- two flame-leaning

## Recommended Upgrade List

Suggested first 12 upgrades:

Arrow:

- `Arrow Damage Up`
- `Arrow Attack Speed Up`
- `Arrow Range Up`
- `Arrow Critical Chance Up`

Flame:

- `Flame Damage Up`
- `Burn Duration Up`
- `Burn Damage Up`
- `Flame Range Up`

Shared:

- `All Towers Damage Up`
- `All Towers Attack Speed Up`

Utility:

- `Gain Extra Gold`
- `Base Health Up` or `Build Cost Down`

## Feedback Requirements

Players must feel progression.

At minimum include:

- upgrade choice UI
- current commander info
- a simple confirmation message when an upgrade is chosen
- optional stat or debug display for tower values

You do not need flashy animation, but you do need clear confirmation that a choice changed gameplay.

## Scene And Flow Structure

Preferred structure:

- Scene A: `Commander Select`
- Scene B: `Battle`

For prototyping speed, it is acceptable to keep this in one scene:

- show a commander selection panel at the start
- after selection, transition into battle flow

## Minimum Content Target

Recommended Week 2 content:

- 2 commanders
- 12 upgrades
- 3-card upgrade reward after each wave
- 3 to 5 test waves
- 1 upgrade UI
- 1 commander selection UI

## Acceptance Criteria

Week 2 is complete when:

1. The player can choose one of two commanders before battle.
2. Commander baseline passive effects remain active during the run.
3. Every wave end reliably enters a three-choice upgrade state.
4. Upgrades come from a unified pool.
5. Choosing an upgrade immediately changes tower behavior.
6. The same basic towers feel different under different commanders.
7. One run can produce at least 3 to 5 upgrade choices.

## Recommended Build Order

Implement in this order:

1. Add the game state machine: `Prepare / Battle / Upgrade`
2. Add reliable wave-end detection
3. Build the upgrade popup
4. Implement the upgrade pool data structure
5. Apply tower stat upgrades
6. Build the commander selection UI
7. Add the two baseline commander passives
8. Add commander-linked upgrades
9. Tune balance and add feedback
