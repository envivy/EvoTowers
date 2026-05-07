# Errors

Command failures and integration errors.

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
