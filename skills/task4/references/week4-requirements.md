# Week 4 Requirements: Complete Single-Run Loop

## Version Goal

Week 4 should complete the first playable run rather than expanding raw content. The player should enter one level, build towers, survive 10 waves, make upgrade and evolution choices, fight a Boss, then see a clear victory or failure result.

Core flow:

`Enter level -> tutorial prompt -> build towers -> kill enemies for resources -> advance waves -> choose upgrades -> commander effects -> tower branch evolution -> Boss -> win/fail settlement`

## Content Scope

Required:

- 10 waves
- 4 normal enemies
- 1 Boss
- victory settlement
- failure settlement
- pause menu
- basic audio
- one balance pass
- lightweight tutorial prompts

Do not include:

- out-of-run progression
- save system
- second map
- complex UI animation
- many new tower types
- many new commanders

## Run Structure

Use 10 waves.

| Phase | Waves | Goal |
| --- | --- | --- |
| Early | 1-3 | Teach basic building and build a first defense |
| Mid | 4-7 | Add upgrades, start build direction, add pressure |
| Late | 8-9 | Let branches mature and enemy combinations grow harder |
| Boss | 10 | Final build check |

Target length: 10-15 minutes.

Recommended timing:

- Normal wave: 45-70 seconds
- Preparation: 10-20 seconds
- Boss wave: 90-150 seconds

## Wave Data Shape

Each wave should support this shape:

```json
{
  "waveId": 1,
  "name": "Wave 1",
  "spawnGroups": [
    {
      "enemyId": "grunt",
      "count": 12,
      "interval": 1.2,
      "delay": 0
    }
  ],
  "clearReward": 30,
  "isBossWave": false
}
```

Wave progression:

`Prepare -> countdown -> spawn enemies -> wait all groups spawned -> wait all enemies dead/leaked -> grant clear reward -> next prepare/upgrade/victory`

## Recommended 10 Waves

| Wave | Enemy mix | Purpose |
| --- | --- | --- |
| 1 | Grunt x12 | Basic defense teaching |
| 2 | Grunt x18 | Encourage adding towers |
| 3 | Grunt + Runner | Introduce leak pressure |
| 4 | Armored | Test single-target damage |
| 5 | Grunt + Runner + Armored | First mixed pressure |
| 6 | Grunt swarm + Healer | Encourage area damage and priority |
| 7 | Runner + Armored | Test slow and point kill |
| 8 | Large mixed wave | Late pressure |
| 9 | Elite Armored + Healer | Final pre-Boss check |
| 10 | Boss + support enemies | Final battle |

## Enemy Roles

### Grunt

- Health: 100
- Speed: 1.0
- Armor: 0
- Gold: 5
- Base damage: 1
- Purpose: basic enemy and steady income

### Runner

- Health: 70
- Speed: 1.6
- Armor: 0
- Gold: 6
- Base damage: 1
- Purpose: leak pressure; values range, attack speed, and slow

### Armored

- Health: 260
- Speed: 0.75
- Armor: 20% damage reduction
- Gold: 10
- Base damage: 2
- Purpose: values Sniper and strong single-target builds

### Healer

- Health: 120
- Speed: 0.9
- Armor: 0
- Gold: 12
- Base damage: 1
- Heal range: medium
- Heal interval: 3 seconds
- Heal amount: 20
- Purpose: priority target; encourages burst, area, and targeting choices

Healing rule:

- Every interval, heal the nearby non-Boss enemy with lowest health.
- Do not heal Boss, or reduce Boss healing by 80%.
- Show simple green feedback.

## Boss

Example name: Molten Behemoth.

Baseline:

- Health: 3500-5000
- Speed: 0.45
- Armor: 15%
- Base damage: 10
- Gold: 0

Skills:

- Summon support at 75%, 50%, and 25% health.
- Below 50% health, armor increases by another 10%.
- Show warning text and play a Boss sound when it appears.

Boss goal:

- Weak early defense should be overwhelmed by support enemies.
- No evolved branches should make Boss damage feel too slow.
- Mature builds should win under pressure.

## Economy

Sources:

- starting gold
- enemy kill gold
- wave clear rewards

Recommended values:

- Starting gold: 180
- Basic tower costs: 80-120
- Wave rewards: 20-60
- Normal enemy gold: 5-12
- Starting lives: 20

Pace goals:

- Waves 1-2: build 2-3 basic towers.
- Waves 3-5: add key towers or early upgrades.
- Waves 6-8: evolve at least 1-2 towers.
- Waves 9-10: form an early build identity.

## Upgrade Draft Integration

Trigger three-choice upgrade drafts:

- after Wave 3
- after Wave 6
- after Wave 8

Requirements:

- show 3 options
- pause wave progression while choosing
- apply immediately
- persist until run end

Example upgrades:

- Archer attack speed +10%
- Flame damage +12%
- Magic range +8%
- Extra +1 gold on kill
- Evolution cost -15%
- Commander cooldown -10%

## Commander Integration

Support existing commanders:

- Flame Warden: flame, burning, blast, area pressure
- Ranger Captain: archer, sniper, rapid shot, elite/Boss handling

Minimum:

- passive effects work
- one active or triggerable effect if stable
- commander effect influences evolved branches
- UI shows selected commander and skill/passive status

## Tower Evolution Integration

The Week 3 evolution system must be reachable during normal run pacing.

Recommended pacing:

- Waves 1-3: base towers
- Waves 4-6: first tower reaches evolution condition
- Waves 7-9: branch-driven build decisions
- Wave 10: Boss checks the build

Validation targets:

- Sniper: meaningful against Armored and Boss
- Rapid Shot: meaningful against Runner and Grunt
- Blast Flame: meaningful against dense groups
- Burning: sustained Boss value
- Frost: extends damage windows
- Chain Lightning: handles medium-density enemy groups

## Settlement

Victory condition:

- Wave 10 Boss is dead
- all active enemies are cleared or leaked

Failure condition:

- base lives <= 0

Victory screen:

- title
- clear time
- completed waves
- remaining lives
- kills
- Boss defeated flag
- towers built
- towers evolved
- commander
- build tags
- restart
- return

Failure screen:

- title
- failed wave
- survival time
- Boss appeared flag
- kills
- towers evolved
- short suggestion
- restart
- return

Suggestion examples:

- If many fast enemies leaked, suggest Rapid Shot or Frost.
- If armored enemies survived too long, suggest Sniper.
- If swarm pressure was high, suggest Blast Flame or Chain Lightning.

## Pause Menu

Minimum:

- continue
- restart
- volume
- return
- Esc toggles pause

Pause should freeze:

- time scale
- enemies
- tower attacks
- skill cooldowns
- wave timers

UI should remain usable.

## Basic Audio

Must have:

- button click
- tower placement
- tower attack
- enemy hit
- enemy death
- wave start
- Boss appears
- victory
- failure

Recommended:

- gold gain
- tower upgrade
- tower evolution
- commander skill

Keep high-frequency sounds low volume, throttled, or pitch-varied.

## Tutorial Prompts

Use short bottom-screen text. Allow dismissal. Do not block gameplay except for very short critical pauses if needed. Show once per run.

Recommended prompts:

| Trigger | Prompt intent |
| --- | --- |
| Enter battle | Build the first tower in a highlighted area |
| Before Wave 1 | Enemies follow the road; defend the goal |
| After Wave 2 | Kills grant gold for building or upgrading |
| First tower level-up | Towers gain experience through battle |
| First evolution-ready tower | Choose a branch to change attack behavior |
| After Wave 9 | Boss incoming; check the defense |

## Balance Pass

Primary targets:

- Run length is 10-15 minutes.
- Waves 4-6 create first real pressure.
- Waves 6-8 allow at least one evolution.
- Boss requires a meaningful build to beat.
- First-time players may lose, but should understand why.
- Familiar players can win with reasonable play.

Tune in this order:

1. Enemy health
2. Enemy count
3. Gold income
4. Tower price
5. Tower damage
6. Evolution cost
7. Boss health

Avoid changing every variable at once.

## Definition Of Done

Week 4 is complete when a player can start from zero, defend through 10 waves, make at least one build choice, face the Boss, and receive a clear victory or failure result.
