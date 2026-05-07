# EvoTowers Week 1 Requirements

## Range

Only build:

- Map loading
- Enemy path movement
- Enemy spawning
- Basic tower placement
- Tower auto-attack
- Enemy damage and death
- Gold and lives
- Single-wave combat, later up to three test waves
- Basic HUD

Do not build:

- Commander
- Upgrade choice
- Tower branch evolution
- Boss
- Audio
- Save system
- Multi-map switching
- Meta progression

## Map

Create one test map only. This week validates the combat skeleton, not level content.

Use a single fixed path. Do not add dual lanes, branches, or terrain mechanics.

Think in a medium rectangular logical area such as 16 x 9 or 20 x 12. Visuals do not need to be strict grid tower defense, but tower placement should be managed as fixed build points.

Divide the map into:

- Spawn Area: enemy start area
- Path Area: enemy route area
- Build Area: valid tower slot area
- Goal Area: enemy destination that subtracts player lives

## Path

Use 6 to 8 waypoints; 7 is the preferred default.

Recommended route shape:

- Start near the upper-left or left-upper side
- Move right
- Turn down
- Move right again
- Turn down again
- End near the lower-right or bottom side

This should read like a simple S-shaped or folded route.

Rules:

- Put waypoints on the road centerline.
- Keep neighboring points as straight segments.
- Make turns obvious.
- Keep the path away from map edges enough to leave tower space.
- Avoid filling the exact map center with road; leave build space.
- Enemies move waypoint by waypoint.
- Reaching the last waypoint triggers goal settlement.
- Enemies do not need avoidance and may overlap.
- Facing direction is optional.

## Build Slots

Create 8 to 12 fixed tower slots; 10 is the preferred default.

Distribution:

- 2 near the spawn area
- 2 to 3 near the first corner
- 2 to 3 near the middle or long straight segment
- 2 to 3 before the goal

Rules:

- Slots must not be on the path.
- Slots should not be so dense that all ranges fully overlap.
- Slots should be close enough to path segments for basic towers to attack.
- Some slots should cover multiple path segments or corners to create strong positions.
- Do not implement free placement in Week 1.

Placement interaction:

- Click an empty tower slot.
- If the player has enough gold, build the selected basic tower.
- If not, reject the build.

## Towers

Create two basic towers:

| Tower | Role | Cost |
| --- | --- | --- |
| Arrow Tower | Single-target, medium attack speed, medium range, general-purpose | 100 |
| Flame Tower | Slower attack, higher damage or simple damage over time | 125 to 150 |

One tower type is acceptable only if reducing scope is necessary to finish the combat loop.

Required behavior:

- Automatically search enemies in range.
- Lock one target.
- Attack at a fixed interval.
- Deal damage.
- Show a minimal attack result; health loss is enough.
- No upgrades, selling, or branch evolution.

Targeting:

- Prefer the enemy closest to the goal.
- Nearest enemy is acceptable if implementation needs to stay simpler.

## Enemies

Create two enemy types:

| Enemy | Role | Reward | Leak Damage |
| --- | --- | --- | --- |
| Basic Enemy | Standard speed and health | 20 gold | 1 life |
| Fast Enemy | Faster, lower health | 25 gold | 1 life |

Required behavior:

- Spawn and follow the path.
- Have health.
- Take damage.
- Die and be destroyed.
- Grant gold on death.
- Subtract life and be destroyed at the goal.

## Waves

Build 1 to 3 waves.

Development sequence:

- Early: 1 wave
- Later: 3 waves

Suggested configuration:

- Wave 1: 10 Basic Enemies
- Wave 2: 8 Basic Enemies + 5 Fast Enemies
- Wave 3: 12 Basic Enemies + 8 Fast Enemies

Required behavior:

- Start wave via button.
- Spawn enemies at timed intervals.
- Wave ends when all enemies are spawned and all spawned enemies are dead or have left via goal.
- Do not add wave reward choices.

## Economy And Lives

Initial resources:

- Gold: 200
- Lives: 20

Costs:

- Arrow Tower: 100
- Flame Tower: 125 or 150

Rewards:

- Basic Enemy death: +20 gold
- Fast Enemy death: +25 gold

Leak penalties:

- Basic Enemy reaches goal: -1 life
- Fast Enemy reaches goal: -1 life

Failure:

- Player life at or below 0 causes game over.

## HUD

Show only:

- Current gold
- Current lives
- Current wave
- Start wave button
- Optional remaining enemy count

Do not add complex animation or visual packaging.

## Scene Structure

Use one main battle scene.

Recommended hierarchy:

```text
GameRoot
MapRoot
PathRoot
Waypoint_01
Waypoint_02
Waypoint_03
...
BuildSlotRoot
BuildSlot_01
BuildSlot_02
...
EnemySpawnPoint
GoalPoint
Managers
GameManager
WaveManager
BuildManager
UIRoot
```

## Acceptance Criteria

Week 1 is complete when:

- The battle scene can be entered.
- The map contains one usable path.
- Enemies move from spawn to goal through waypoints.
- The player can build basic towers on fixed tower slots.
- Towers automatically attack enemies.
- Enemy death grants gold.
- Enemy goal entry subtracts life.
- Life reaching 0 ends the game.
- At least 1 to 3 test waves can run to completion.
