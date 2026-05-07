# Learnings

Corrections, insights, and knowledge gaps captured during development.

**Categories**: correction | insight | knowledge_gap | best_practice

---

## [LRN-20260506-001] best_practice

**Logged**: 2026-05-06T17:00:00+08:00
**Priority**: medium
**Status**: pending
**Area**: frontend

### Summary
For quick Unity prototype enemy health bars, prefer SpriteRenderer bars over World Space UGUI unless rect transform scale is carefully verified.

### Details
The first Task1 enemy prefab used a World Space Canvas health bar. At runtime the green Image could render huge and cover the screen when prefab scale or RectTransform values were off.

### Suggested Action
Use small SpriteRenderer background/fill bars for world-space enemy health in prototypes, or explicitly validate Canvas scale, RectTransform size, and generated prefab replacement.

### Metadata
- Source: error
- Related Files: Assets/Scripts/Task1/EnemyHealth.cs, Assets/Editor/Task1/Task1SceneBuilder.cs
- Tags: unity, ui, prototype

---
