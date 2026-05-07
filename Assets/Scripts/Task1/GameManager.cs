using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace EvoTowers.Task1
{
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        [SerializeField] private int startingGold = 500;
        [SerializeField] private int startingLives = 20;
        [SerializeField] private List<TowerConfig> towerConfigs = new List<TowerConfig>();
        [SerializeField] private List<EnemyConfig> enemyConfigs = new List<EnemyConfig>();
        [SerializeField] private List<CommanderDefinition> commanderDefinitions = new List<CommanderDefinition>();
        [SerializeField] private List<UpgradeDefinition> upgradeDefinitions = new List<UpgradeDefinition>();
        [SerializeField] private List<TowerEvolutionDefinition> towerEvolutions = new List<TowerEvolutionDefinition>();

        private readonly List<EnemyHealth> activeEnemies = new List<EnemyHealth>();
        private readonly List<UpgradeDefinition> ownedUpgrades = new List<UpgradeDefinition>();
        private readonly Dictionary<UpgradeTarget, TowerModifierSnapshot> towerModifiers = new Dictionary<UpgradeTarget, TowerModifierSnapshot>();

        public event Action StateChanged;
        public event Action GameOver;
        public event Action Victory;
        public event Action<UpgradeDraftResult> UpgradeDraftReady;
        public event Action<UpgradeDefinition> UpgradeApplied;
        public event Action<CommanderDefinition> CommanderSelected;

        public int Gold { get; private set; }
        public int Lives { get; private set; }
        public bool IsGameOver { get; private set; }
        public bool IsVictory { get; private set; }
        public GameFlowState FlowState { get; private set; }
        public CommanderDefinition ActiveCommander { get; private set; }
        public IReadOnlyList<EnemyHealth> ActiveEnemies => activeEnemies;
        public IReadOnlyList<UpgradeDefinition> OwnedUpgrades => ownedUpgrades;
        public UpgradeDraftResult CurrentUpgradeDraft { get; private set; }
        public float RunTime { get; private set; }
        public int Kills { get; private set; }
        public int TowersBuilt { get; private set; }
        public int TowersEvolved { get; private set; }
        public int CompletedWaves { get; private set; }
        public int FailedWave { get; private set; }
        public bool BossAppeared { get; private set; }

        private void Update()
        {
            if (!IsGameOver && FlowState != GameFlowState.Upgrade)
            {
                RunTime += Time.deltaTime;
            }
        }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            Gold = startingGold;
            Lives = startingLives;
            FlowState = GameFlowState.Prepare;
            towerModifiers[UpgradeTarget.Global] = default;
            towerModifiers[UpgradeTarget.ArrowTower] = default;
            towerModifiers[UpgradeTarget.FlameTower] = default;
            towerModifiers[UpgradeTarget.MagicTower] = default;
            EnsureWeek3Defaults();
        }

        private void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
            }
        }

        public TowerConfig GetTowerConfig(TowerType type)
        {
            return towerConfigs.FirstOrDefault(config => config.type == type);
        }

        public EnemyConfig GetEnemyConfig(EnemyType type)
        {
            return enemyConfigs.FirstOrDefault(config => config.type == type);
        }

        public bool TrySpendGold(int amount)
        {
            if (IsGameOver || FlowState == GameFlowState.Upgrade || Gold < amount)
            {
                return false;
            }

            Gold -= amount;
            StateChanged?.Invoke();
            return true;
        }

        public void AddGold(int amount)
        {
            if (amount <= 0)
            {
                return;
            }

            Gold += amount;
            StateChanged?.Invoke();
        }

        public void LoseLives(int amount)
        {
            if (IsGameOver || amount <= 0)
            {
                return;
            }

            Lives = Mathf.Max(0, Lives - amount);
            StateChanged?.Invoke();

            if (Lives <= 0)
            {
                IsGameOver = true;
                IsVictory = false;
                FlowState = GameFlowState.GameOver;
                StateChanged?.Invoke();
                GameAudio.Instance?.PlayFailure();
                GameOver?.Invoke();
            }
        }

        public void WinGame()
        {
            if (IsGameOver)
            {
                return;
            }

            IsGameOver = true;
            IsVictory = true;
            FlowState = GameFlowState.Victory;
            StateChanged?.Invoke();
            GameAudio.Instance?.PlayVictory();
            Victory?.Invoke();
        }

        public void SetFlowState(GameFlowState newState)
        {
            FlowState = newState;
            StateChanged?.Invoke();
        }

        public void SelectCommander(CommanderId commanderId)
        {
            CommanderDefinition commander = commanderDefinitions.FirstOrDefault(item => item.id == commanderId);
            if (commander == null)
            {
                return;
            }

            ActiveCommander = commander;
            CommanderSelected?.Invoke(commander);
            StateChanged?.Invoke();
        }

        public CommanderDefinition GetCommander(CommanderId commanderId)
        {
            return commanderDefinitions.FirstOrDefault(item => item.id == commanderId);
        }

        public IReadOnlyList<CommanderDefinition> GetCommanderDefinitions()
        {
            return commanderDefinitions;
        }

        public UpgradeDraftResult BuildUpgradeDraft(int optionCount)
        {
            List<UpgradeDefinition> pool = upgradeDefinitions
                .Where(IsUpgradeValidForDraft)
                .OrderBy(_ => UnityEngine.Random.value)
                .Take(optionCount)
                .ToList();

            if (pool.Count < optionCount)
            {
                List<UpgradeDefinition> fallbackPool = upgradeDefinitions
                    .Where(definition => definition != null)
                    .Where(definition => definition.commanderRestriction == CommanderId.None ||
                                         (ActiveCommander != null && ActiveCommander.id == definition.commanderRestriction))
                    .Where(definition => definition.isRepeatable || !ownedUpgrades.Contains(definition))
                    .OrderBy(_ => UnityEngine.Random.value)
                    .ToList();

                foreach (UpgradeDefinition fallback in fallbackPool)
                {
                    if (pool.Count >= optionCount)
                    {
                        break;
                    }

                    if (pool.Contains(fallback))
                    {
                        continue;
                    }

                    pool.Add(fallback);
                }
            }

            UpgradeDraftResult result = new UpgradeDraftResult
            {
                IsValid = pool.Count > 0,
                Options = pool
            };

            return result;
        }

        public void OpenUpgradeDraft(int optionCount)
        {
            SetFlowState(GameFlowState.Upgrade);
            Time.timeScale = 0f;
            CurrentUpgradeDraft = BuildUpgradeDraft(optionCount);
            UpgradeDraftReady?.Invoke(CurrentUpgradeDraft);
        }

        public bool ApplyUpgrade(UpgradeDefinition definition)
        {
            if (definition == null || !IsUpgradeValidForDraft(definition))
            {
                return false;
            }

            if (!definition.isRepeatable)
            {
                ownedUpgrades.Add(definition);
            }

            switch (definition.modifierType)
            {
                case StatModifierType.BonusGoldFlat:
                    AddGold(Mathf.RoundToInt(definition.value));
                    break;
                case StatModifierType.BaseHealthFlat:
                    Lives += Mathf.RoundToInt(definition.value);
                    break;
                default:
                    ApplyTowerModifier(definition);
                    break;
            }

            CurrentUpgradeDraft = default;
            UpgradeApplied?.Invoke(definition);
            StateChanged?.Invoke();
            return true;
        }

        public void CloseUpgradeDraftToPrepare()
        {
            Time.timeScale = 1f;
            SetFlowState(GameFlowState.Prepare);
        }

        public void RegisterEnemy(EnemyHealth enemy)
        {
            if (enemy == null || activeEnemies.Contains(enemy))
            {
                return;
            }

            activeEnemies.Add(enemy);
            StateChanged?.Invoke();
        }

        public void NotifyStateChanged()
        {
            StateChanged?.Invoke();
        }

        public void NotifyEnemyKilled(EnemyHealth enemy)
        {
            Kills++;
            StateChanged?.Invoke();
        }

        public void NotifyTowerBuilt()
        {
            TowersBuilt++;
            StateChanged?.Invoke();
        }

        public void NotifyTowerEvolved()
        {
            TowersEvolved++;
            StateChanged?.Invoke();
        }

        public void NotifyWaveCompleted(int waveNumber, int clearReward)
        {
            CompletedWaves = Mathf.Max(CompletedWaves, waveNumber);
            AddGold(clearReward);
        }

        public void NotifyWaveStarted(int waveNumber, bool isBossWave)
        {
            if (isBossWave)
            {
                BossAppeared = true;
            }

            StateChanged?.Invoke();
        }

        public void NotifyWaveFailed(int waveNumber)
        {
            FailedWave = Mathf.Max(FailedWave, waveNumber);
        }

        public string BuildSettlementSummary()
        {
            string result = IsVictory ? "Victory" : "Failure";
            int wave = IsVictory ? CompletedWaves : Mathf.Max(FailedWave, CompletedWaves + 1);
            string boss = BossAppeared ? "Boss appeared" : "Boss not reached";
            string commander = ActiveCommander != null ? ActiveCommander.displayName : "None";
            string suggestion = IsVictory ? "Boss defeated. Your build held." : BuildFailureSuggestion();
            return $"{result}\nTime: {FormatTime(RunTime)}\nWaves: {wave}\nLives: {Lives}/20\nKills: {Kills}\nTowers: {TowersBuilt}\nEvolved: {TowersEvolved}\nCommander: {commander}\n{boss}\n{suggestion}";
        }

        public void UnregisterEnemy(EnemyHealth enemy)
        {
            if (enemy == null)
            {
                return;
            }

            if (activeEnemies.Remove(enemy))
            {
                StateChanged?.Invoke();
            }
        }

        public IReadOnlyList<TowerConfig> GetTowerConfigs()
        {
            return towerConfigs;
        }

        private string BuildFailureSuggestion()
        {
            if (BossAppeared)
            {
                return "Suggestion: evolve Sniper or Burning branches before the Boss.";
            }

            if (CompletedWaves < 4)
            {
                return "Suggestion: build 2-3 towers before early waves snowball.";
            }

            if (TowersEvolved <= 0)
            {
                return "Suggestion: select a branch evolution by the mid game.";
            }

            return "Suggestion: add Frost, Blast Flame, or Chain Lightning for mixed waves.";
        }

        private static string FormatTime(float seconds)
        {
            int totalSeconds = Mathf.FloorToInt(seconds);
            int minutes = totalSeconds / 60;
            int remainder = totalSeconds % 60;
            return $"{minutes:00}:{remainder:00}";
        }

        public TowerRuntimeStats GetRuntimeStats(TowerConfig config)
        {
            return GetRuntimeStats(config, null, 1);
        }

        public TowerRuntimeStats GetRuntimeStats(TowerConfig config, TowerEvolutionDefinition evolution, int level)
        {
            TowerRuntimeStats stats = new TowerRuntimeStats
            {
                Damage = config.damage,
                AttackInterval = config.attackInterval,
                Range = config.range,
                CritChance = 0f,
                BurnDamagePerSecond = config.dotDamagePerSecond,
                BurnDuration = config.dotDuration
            };

            float levelDamageMultiplier = level >= 3 ? 1.15f : level >= 2 ? 1.1f : 1f;
            stats.Damage *= levelDamageMultiplier;

            if (evolution != null)
            {
                stats.Damage *= evolution.damageMultiplier;
                stats.AttackInterval /= Mathf.Max(0.05f, evolution.attackSpeedMultiplier);
                stats.Range *= evolution.rangeMultiplier;
                if (evolution.burnDamageMultiplier > 0f)
                {
                    stats.BurnDamagePerSecond = config.damage * evolution.burnDamageMultiplier;
                }

                if (evolution.burnDuration > 0f)
                {
                    stats.BurnDuration = evolution.burnDuration;
                }
            }

            ApplySnapshot(ref stats, GetModifierSnapshot(UpgradeTarget.Global));
            ApplySnapshot(ref stats, GetModifierSnapshot(MapTowerTypeToTarget(config.type)));
            ApplyCommander(ref stats, config.type, evolution);

            return stats;
        }

        public int GetBuildCost(TowerConfig config)
        {
            TowerModifierSnapshot global = GetModifierSnapshot(UpgradeTarget.Global);
            TowerModifierSnapshot type = GetModifierSnapshot(MapTowerTypeToTarget(config.type));
            float totalPercent = global.BuildCostPercent + type.BuildCostPercent;
            float cost = config.cost * Mathf.Max(0.1f, 1f + totalPercent);
            return Mathf.RoundToInt(cost);
        }

        public string GetCommanderSummary()
        {
            if (ActiveCommander == null)
            {
                return "Commander: None";
            }

            return $"{ActiveCommander.displayName}: {ActiveCommander.description}";
        }

        public IReadOnlyList<TowerEvolutionDefinition> GetEvolutionsFor(TowerType type)
        {
            return towerEvolutions.Where(definition => definition != null && definition.baseTowerType == type).ToList();
        }

        public float GetSniperEliteBonus(TowerController tower)
        {
            if (ActiveCommander == null || ActiveCommander.id != CommanderId.RangerCaptain || tower == null || tower.BranchId != TowerBranchId.Sniper)
            {
                return 0f;
            }

            return 0.15f;
        }

        public int GetRapidShotExtraStacks(TowerController tower)
        {
            if (ActiveCommander == null || ActiveCommander.id != CommanderId.RangerCaptain || tower == null || tower.BranchId != TowerBranchId.RapidShot)
            {
                return 0;
            }

            return 2;
        }

        public float GetBurnDurationBonus(TowerController tower)
        {
            if (ActiveCommander == null || ActiveCommander.id != CommanderId.FlameWarden || tower == null || !tower.HasTag(TowerTag.Fire))
            {
                return 0f;
            }

            return 1f;
        }

        public float GetExplosionRadiusBonus(TowerController tower)
        {
            if (ActiveCommander == null || ActiveCommander.id != CommanderId.FlameWarden || tower == null || tower.BranchId != TowerBranchId.BlastFlame)
            {
                return 0f;
            }

            return 0.1f;
        }

        private bool IsUpgradeValidForDraft(UpgradeDefinition definition)
        {
            if (definition == null)
            {
                return false;
            }

            if (definition.commanderRestriction != CommanderId.None)
            {
                if (ActiveCommander == null || ActiveCommander.id != definition.commanderRestriction)
                {
                    return false;
                }
            }

            if (!definition.isRepeatable && ownedUpgrades.Contains(definition))
            {
                return false;
            }

            return true;
        }

        private void ApplyTowerModifier(UpgradeDefinition definition)
        {
            UpgradeTarget target = definition.target;
            TowerModifierSnapshot snapshot = GetModifierSnapshot(target);

            switch (definition.modifierType)
            {
                case StatModifierType.DamagePercent:
                    snapshot.DamagePercent += definition.value;
                    break;
                case StatModifierType.AttackSpeedPercent:
                    snapshot.AttackSpeedPercent += definition.value;
                    break;
                case StatModifierType.RangeFlat:
                    snapshot.RangeFlat += definition.value;
                    break;
                case StatModifierType.CritChanceFlat:
                    snapshot.CritChanceFlat += definition.value;
                    break;
                case StatModifierType.BurnDamagePercent:
                    snapshot.BurnDamagePercent += definition.value;
                    break;
                case StatModifierType.BurnDurationPercent:
                    snapshot.BurnDurationPercent += definition.value;
                    break;
                case StatModifierType.BuildCostPercent:
                    snapshot.BuildCostPercent += definition.value;
                    break;
            }

            towerModifiers[target] = snapshot;
        }

        private TowerModifierSnapshot GetModifierSnapshot(UpgradeTarget target)
        {
            if (!towerModifiers.TryGetValue(target, out TowerModifierSnapshot snapshot))
            {
                snapshot = default;
                towerModifiers[target] = snapshot;
            }

            return snapshot;
        }

        private static UpgradeTarget MapTowerTypeToTarget(TowerType type)
        {
            switch (type)
            {
                case TowerType.Arrow:
                    return UpgradeTarget.ArrowTower;
                case TowerType.Flame:
                    return UpgradeTarget.FlameTower;
                case TowerType.Magic:
                    return UpgradeTarget.MagicTower;
                default:
                    return UpgradeTarget.Global;
            }
        }

        private void ApplyCommander(ref TowerRuntimeStats stats, TowerType towerType, TowerEvolutionDefinition evolution)
        {
            if (ActiveCommander == null)
            {
                return;
            }

            if (evolution != null)
            {
                if (ActiveCommander.id == CommanderId.FlameWarden && evolution.HasTag(TowerTag.Fire))
                {
                    stats.Damage *= 1.1f;
                    stats.BurnDuration += 1f;
                    return;
                }

                if (ActiveCommander.id == CommanderId.RangerCaptain && evolution.HasTag(TowerTag.Archer))
                {
                    stats.Range *= 1.1f;
                    return;
                }
            }

            UpgradeTarget towerTarget = MapTowerTypeToTarget(towerType);
            if (ActiveCommander.primaryTarget != towerTarget)
            {
                return;
            }

            stats.Damage *= 1f + ActiveCommander.baseDamagePercent;
            stats.AttackInterval /= Mathf.Max(0.05f, 1f + ActiveCommander.baseAttackSpeedPercent);
            stats.CritChance += ActiveCommander.baseCritChanceFlat;
            stats.BurnDamagePerSecond *= 1f + ActiveCommander.baseBurnDamagePercent;
            stats.BurnDuration *= 1f + ActiveCommander.baseBurnDurationPercent;
        }

        private static void ApplySnapshot(ref TowerRuntimeStats stats, TowerModifierSnapshot snapshot)
        {
            stats.Damage *= 1f + snapshot.DamagePercent;
            stats.AttackInterval /= Mathf.Max(0.05f, 1f + snapshot.AttackSpeedPercent);
            stats.Range += snapshot.RangeFlat;
            stats.CritChance += snapshot.CritChanceFlat;
            stats.BurnDamagePerSecond *= 1f + snapshot.BurnDamagePercent;
            stats.BurnDuration *= 1f + snapshot.BurnDurationPercent;
        }

        private void EnsureWeek3Defaults()
        {
            if (towerConfigs.All(config => config == null || config.type != TowerType.Magic))
            {
                towerConfigs.Add(new TowerConfig
                {
                    type = TowerType.Magic,
                    displayName = "Magic Tower",
                    cost = 130,
                    range = 2.65f,
                    damage = 24f,
                    attackInterval = 0.9f,
                    useDamageOverTime = false,
                    visualScale = Vector2.one
                });
            }

            if (towerEvolutions.Count > 0)
            {
                return;
            }

            towerEvolutions.Add(CreateEvolution("archer_sniper", TowerType.Arrow, TowerBranchId.Sniper, "Sniper Tower", "Single, High DMG, Elite", "Charges briefly, prefers high-health enemies, and deals extra damage to elites.", 120, 2.5f, 0.55f, 1.2f, TowerTargetPriority.MarkedThenHighestHealth, new Color(1f, 0.9f, 0.35f, 1f), new[] { TowerTag.Physical, TowerTag.Archer, TowerTag.SingleTarget }, eliteBonus: 0.3f, charge: 0.22f));
            towerEvolutions.Add(CreateEvolution("archer_rapid", TowerType.Arrow, TowerBranchId.RapidShot, "Rapid Shot Tower", "Fast, Combo, Clear", "Fires rapidly and gains attack speed while staying on the same target.", 110, 0.7f, 2.2f, 0.9f, TowerTargetPriority.MarkedThenClosestToGoal, new Color(0.55f, 1f, 0.58f, 1f), new[] { TowerTag.Physical, TowerTag.Archer, TowerTag.SingleTarget }, comboStacks: 5, comboSpeed: 0.06f));
            towerEvolutions.Add(CreateEvolution("flame_blast", TowerType.Flame, TowerBranchId.BlastFlame, "Blast Flame Tower", "Area, Burst, Splash", "Creates a large explosion with center-to-edge damage falloff.", 130, 1.8f, 0.7f, 1f, TowerTargetPriority.ClosestToGoal, new Color(1f, 0.34f, 0.06f, 1f), new[] { TowerTag.Magic, TowerTag.Fire, TowerTag.Area }, explosionRadius: 1.4f, edge: 0.6f));
            towerEvolutions.Add(CreateEvolution("flame_burning", TowerType.Flame, TowerBranchId.Burning, "Burning Tower", "DOT, Fire, Refresh", "Applies refreshable burning damage over time with capped stacks.", 120, 0.75f, 1.1f, 1f, TowerTargetPriority.ClosestToGoal, new Color(1f, 0.55f, 0.18f, 1f), new[] { TowerTag.Magic, TowerTag.Fire, TowerTag.DamageOverTime }, burnDuration: 4f, burnDamage: 0.25f, burnStacks: 3));
            towerEvolutions.Add(CreateEvolution("magic_frost", TowerType.Magic, TowerBranchId.Frost, "Frost Tower", "Control, Slow, Splash", "Deals lighter damage and refreshes a small splash slow.", 110, 0.65f, 0.9f, 1.1f, TowerTargetPriority.ClosestToGoal, new Color(0.38f, 0.78f, 1f, 1f), new[] { TowerTag.Magic, TowerTag.Ice, TowerTag.Control }, slow: 0.3f, slowDuration: 2f, splash: 0.75f));
            towerEvolutions.Add(CreateEvolution("magic_chain", TowerType.Magic, TowerBranchId.ChainLightning, "Chain Lightning Tower", "Bounce, Group, Lightning", "Bounces through nearby enemies without repeating targets.", 125, 0.9f, 1f, 1f, TowerTargetPriority.ClosestToGoal, new Color(0.7f, 0.85f, 1f, 1f), new[] { TowerTag.Magic, TowerTag.Lightning, TowerTag.Area }, chainTargets: 3, chainFalloff: 0.2f, chainRange: 1.45f));
        }

        private static TowerEvolutionDefinition CreateEvolution(string id, TowerType type, TowerBranchId branch, string name, string tags, string description, int cost, float damage, float attackSpeed, float range, TowerTargetPriority priority, Color color, TowerTag[] tagList, float eliteBonus = 0f, float explosionRadius = 1f, float edge = 0.6f, float burnDuration = 0f, float burnDamage = 0f, int burnStacks = 1, float slow = 0f, float slowDuration = 0f, float splash = 0f, int comboStacks = 0, float comboSpeed = 0f, int chainTargets = 0, float chainFalloff = 0f, float chainRange = 0f, float charge = 0f)
        {
            return new TowerEvolutionDefinition
            {
                id = id,
                baseTowerType = type,
                branchId = branch,
                displayName = name,
                gameplayTags = tags,
                description = description,
                evolveCost = cost,
                requiredLevel = 3,
                requiredExperience = 100,
                damageMultiplier = damage,
                attackSpeedMultiplier = attackSpeed,
                rangeMultiplier = range,
                targetPriority = priority,
                accentColor = color,
                tags = tagList,
                eliteBonusDamage = eliteBonus,
                explosionRadiusMultiplier = explosionRadius,
                explosionEdgeDamagePercent = edge,
                burnDuration = burnDuration,
                burnDamageMultiplier = burnDamage,
                maxBurnStacks = burnStacks,
                slowPercent = slow,
                slowDuration = slowDuration,
                splashRadius = splash,
                maxComboStacks = comboStacks,
                comboAttackSpeedPerStack = comboSpeed,
                chainTargets = chainTargets,
                chainDamageFalloff = chainFalloff,
                chainSearchRange = chainRange,
                chargeDelay = charge
            };
        }
    }
}
