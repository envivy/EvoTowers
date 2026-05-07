---
name: task1
description: EvoTowers first-week tower-defense prototype scope and acceptance rules. Use when planning, implementing, reviewing, or validating Week 1 features for this Unity project: map loading, fixed path movement, enemy spawning, fixed build slots, basic towers, tower auto-attacks, enemy damage/death, gold/lives, single-wave or three-wave combat, and minimal HUD.
---

# Task1

## Purpose

Use this skill to keep Week 1 EvoTowers work tightly scoped to the playable battle skeleton. Favor the smallest complete loop over content breadth, polish, or future systems.

For the full requirement text and numeric configuration, read `references/week1-requirements.md`.

## Scope Guardrails

Implement only these systems:

- Map loading for one test battle map
- Enemy path movement
- Enemy spawning
- Fixed build-slot tower placement
- Tower auto-attack
- Enemy damage, death, rewards, and goal leakage
- Gold and player lives
- One to three test waves
- Minimal HUD

Do not implement these during Week 1 unless the user explicitly overrides the scope:

- Commanders
- Upgrade choice rewards
- Tower branch evolution
- Bosses
- Audio
- Saves
- Multiple map switching
- Meta progression

## Required Baseline

When designing or coding Week 1, target this minimum content set:

- 1 map
- 7 path waypoints
- 10 fixed build slots
- 2 basic towers: Arrow Tower and Flame Tower
- 2 enemy types: Basic Enemy and Fast Enemy
- 3 test waves when feasible; 1 wave is acceptable early in development
- 1 main battle scene

Use fixed tower slots rather than free placement. Use straight waypoint segments rather than curves. Enemy overlap is allowed.

## Gameplay Rules

Enemies move through waypoints in order. Reaching the final waypoint triggers goal settlement: subtract lives and destroy the enemy.

Towers search automatically, lock one target, attack on a fixed interval, and apply damage. Prefer targeting the enemy closest to the goal because it matches tower-defense intuition.

Build slots are clickable. If empty and the player has enough gold, build the selected basic tower. If gold is insufficient, reject the build.

Killed enemies grant gold. Leaked enemies subtract life. Player life reaching 0 is game over.

## Unity Structure

Prefer one main battle scene with this hierarchy:

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

Keep scripts boring and direct. Good first-week candidates include `GameManager`, `WaveManager`, `EnemyPathFollower`, `EnemyHealth`, `TowerController`, `BuildSlot`, `BuildManager`, and config assets or serializable structs for tower, enemy, and wave values.

## Acceptance Checklist

Before calling Week 1 complete, verify:

- The battle scene opens and runs.
- A single usable path exists.
- Enemies spawn and move from spawn to goal.
- The player can build basic towers on fixed slots.
- Towers automatically attack enemies.
- Enemy death grants gold.
- Enemy goal entry subtracts player life.
- Life reaching 0 ends the game.
- At least 1 full wave can finish; 3 waves are the preferred target.
