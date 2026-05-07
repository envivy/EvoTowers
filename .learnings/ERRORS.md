# Errors

Command failures and integration errors.

---

## [ERR-20260507-005] luban_tower_csv_sharing_violation

**Logged**: 2026-05-07T16:10:18+08:00
**Priority**: medium
**Status**: resolved
**Area**: config

### Summary
Running `Create Week3 Battle Scene` failed when the Luban tower CSV was temporarily locked by another process.

### Error
```text
IOException: Sharing violation on path E:\AgentGames\EvoTowers\Tools\Luban\Datas\Tower.csv
System.IO.File.ReadAllLines
EvoTowers.EditorTools.LubanTowerConfigImporter.LoadTowerRows
```

### Context
- Operation attempted: Unity menu `EvoTowers/Task3/Create Week3 Battle Scene`
- The scene builder imports tower configs from `Tools/Luban/Datas/Tower.csv`.
- `File.ReadAllLines` can fail when Excel, Unity import, source-control tooling, or sync tooling holds the CSV with a conflicting lock.

### Suggested Fix
Read the CSV through a `FileStream` with `FileShare.ReadWrite | FileShare.Delete` and retry briefly before surfacing an actionable error.

### Metadata
- Reproducible: yes
- Related Files: Assets/Editor/Luban/LubanTowerConfigImporter.cs, Tools/Luban/Datas/Tower.csv

### Resolution
- **Resolved**: 2026-05-07T16:10:18+08:00
- **Notes**: Updated `LubanTowerConfigImporter` to read with shared access and a short retry loop.

---

## [ERR-20260507-004] unity_batchmode_license_ipc_timeout

**Logged**: 2026-05-07T16:00:08+08:00
**Priority**: medium
**Status**: pending
**Area**: config

### Summary
Unity batchmode scene generation could not run because the editor failed to connect to the LicensingClient IPC channel.

### Error
```text
IPC channel to LicensingClient doesn't exist; aborting
Application will terminate with return code 199
```

### Context
- Command attempted: `Unity.exe -batchmode -quit -projectPath E:\AgentGames\EvoTowers -executeMethod EvoTowers.Task1.Editor.Task1SceneBuilder.CreateWeek3BattleScene`
- The shell returned 0, but Unity log showed licensing failure and the execute method did not update `Assets/Scenes/Battle.unity`.

### Suggested Fix
Open Unity interactively once to refresh licensing, or run batchmode from an environment where the Unity LicensingClient can start and accept IPC connections.

### Metadata
- Reproducible: unknown
- Related Files: Assets/Editor/Task1/Task1SceneBuilder.cs, Temp/task4-scene-build.log

---

## [ERR-20260507-003] powershell_select_object_range

**Logged**: 2026-05-07T15:26:52+08:00
**Priority**: low
**Status**: pending
**Area**: config

### Summary
PowerShell rejected `Select-Object -Index 300..380` because the range expression was passed as a string-like argument instead of an evaluated array.

### Error
```text
Cannot bind parameter 'Index'. Cannot convert value "300..380" to type "System.Int32".
```

### Context
- Command attempted: `Get-Content Assets\Editor\Task1\Task1SceneBuilder.cs | Select-Object -Index 300..380`
- The failure was caused by PowerShell syntax, not project code.

### Suggested Fix
Use `Select-Object -Skip <n> -First <n>` for file snippets, or wrap ranges as `(300..380)` when an `-Index` array is required.

### Metadata
- Reproducible: yes
- Related Files: none
- See Also: ERR-20260506-002

---

## [ERR-20260506-004] unity_builtin_font_arial_removed

**Logged**: 2026-05-06T17:00:00+08:00
**Priority**: medium
**Status**: resolved
**Area**: config

### Summary
Unity 2022.3.62 reports `Arial.ttf` is no longer a valid built-in font for `Resources.GetBuiltinResource`.

### Error
```text
ArgumentException: Arial.ttf is no longer a valid built in font. Please use LegacyRuntime.ttf
```

### Context
- Operation attempted: generate Week 1 battle scene via `EvoTowers/Task1/Create Week1 Battle Scene`
- Failing file: `Assets/Editor/Task1/Task1SceneBuilder.cs`
- Failing call: `Resources.GetBuiltinResource<Font>("Arial.ttf")`

### Suggested Fix
Use `Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf")` for generated legacy UGUI Text.

### Metadata
- Reproducible: yes
- Related Files: Assets/Editor/Task1/Task1SceneBuilder.cs

### Resolution
- **Resolved**: 2026-05-06T17:00:00+08:00
- **Notes**: Replaced `Arial.ttf` with `LegacyRuntime.ttf` in the scene builder.

---

## [ERR-20260506-002] powershell_regex_quote_parse

**Logged**: 2026-05-06T17:00:00+08:00
**Priority**: low
**Status**: pending
**Area**: config

### Summary
A PowerShell command failed because the inline `rg` regex quoting was parsed incorrectly by PowerShell.

### Error
```text
Missing closing ')' in expression.
```

### Context
- Command attempted: `rg "FindProperty\(\"|FindPropertyRelative\(\"" Assets/Editor/Task1/Task1SceneBuilder.cs`
- The failure was caused by shell quoting, not project code.

### Suggested Fix
Use simpler search patterns or single-quoted PowerShell strings for regex-heavy commands.

### Metadata
- Reproducible: yes
- Related Files: Assets/Editor/Task1/Task1SceneBuilder.cs

---

## [ERR-20260506-003] git_status_not_repo

**Logged**: 2026-05-06T17:00:00+08:00
**Priority**: low
**Status**: pending
**Area**: config

### Summary
`git status` could not run because the workspace is not a git repository.

### Error
```text
fatal: not a git repository (or any of the parent directories): .git
```

### Context
- Command attempted: `git status --short`
- Workspace: `E:\TeamGames\EvoTowers`

### Suggested Fix
Initialize git for the project or skip git-based summaries in this workspace.

### Metadata
- Reproducible: yes
- Related Files: none

---

## [ERR-20260506-001] skill_validation_missing_yaml

**Logged**: 2026-05-06T17:00:00+08:00
**Priority**: medium
**Status**: pending
**Area**: config

### Summary
Skill validation script could not run because the active Python environment lacks PyYAML.

### Error
```text
ModuleNotFoundError: No module named 'yaml'
```

### Context
- Command attempted: `python C:\Users\admin\.codex\skills\.system\skill-creator\scripts\quick_validate.py skills\task1`
- Workspace: `E:\TeamGames\EvoTowers`
- The failure is in the local validation tool environment, not in the `task1` skill files.

### Suggested Fix
Install PyYAML in the Python environment used by Codex, or update the validation script environment to include its dependencies.

### Metadata
- Reproducible: yes
- Related Files: skills/task1/SKILL.md
- See Also: repeated on 2026-05-07 while validating skills/task3/SKILL.md and skills/rules/SKILL.md
- Recurrence-Count: 3

---

## [ERR-20260507-001] unity_generated_csproj_missing_new_scripts

**Logged**: 2026-05-07T14:18:10+08:00
**Priority**: low
**Status**: resolved
**Area**: config

### Summary
`dotnet build` could not resolve newly added Unity script types because the generated `Assembly-CSharp.csproj` had not yet been refreshed to include the new `.cs` files.

### Error
```text
CS0246: could not find TowerEvolutionDefinition, TowerBranchId, TowerTag, EnemyStatusType, and related new types
```

### Context
- Command attempted: `dotnet build Assembly-CSharp-Editor.csproj --no-restore`
- New scripts existed under `Assets/Scripts/Task1/`, but `Assembly-CSharp.csproj` still listed only older files.
- Unity normally regenerates project files, but direct dotnet validation needs the csproj to include new compile items.

### Suggested Fix
Refresh Unity project files or manually add new script compile entries before using direct `dotnet build` as a validation shortcut.

### Metadata
- Reproducible: yes
- Related Files: Assembly-CSharp.csproj, Assets/Scripts/Task1/TowerEvolutionDefinition.cs

### Resolution
- **Resolved**: 2026-05-07T14:20:00+08:00
- **Notes**: Added new Task1 script compile entries to `Assembly-CSharp.csproj`; runtime and editor projects then built with 0 errors.

---

## [ERR-20260507-002] git_push_github_network_unreachable

**Logged**: 2026-05-07T15:05:00+08:00
**Priority**: medium
**Status**: pending
**Area**: infra

### Summary
Pushing the local EvoTowers repository to GitHub failed before authentication because the environment could not connect to `github.com:443`.

### Error
```text
fatal: unable to access 'https://github.com/envivy/EvoTowers.git/': Failed to connect to github.com port 443 after 21132 ms: Could not connect to server
```

### Context
- Command attempted: `git push -u origin main`
- Remote configured: `https://github.com/envivy/EvoTowers.git`
- Local repository exists and has initial commit `be3bb23`.
- Git LFS is configured locally.

### Suggested Fix
Retry `git push -u origin main` from a network environment that can reach GitHub, or configure a proxy/VPN/credential helper if required.

### Metadata
- Reproducible: unknown
- Related Files: .git/config

---
