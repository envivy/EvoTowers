using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace EvoTowers.Task1
{
    public class TowerController : MonoBehaviour
    {
        [SerializeField] private SpriteRenderer spriteRenderer;
        [SerializeField] private Transform firePoint;

        private readonly List<GameObject> spawnedVisuals = new List<GameObject>();
        private TowerConfig config;
        private TowerEvolutionDefinition evolution;
        private TowerRuntimeStats runtimeStats;
        private float attackTimer;
        private int experience;
        private int level = 1;
        private EnemyHealth comboTarget;
        private int comboStacks;
        private bool isCharging;

        public TowerType Type => config != null ? config.type : TowerType.Arrow;
        public TowerBranchId BranchId => evolution != null ? evolution.branchId : TowerBranchId.None;
        public string DisplayName => evolution != null ? evolution.displayName : config != null ? config.displayName : "Tower";
        public int Experience => experience;
        public int Level => level;
        public bool IsEvolved => evolution != null;
        public TowerEvolutionDefinition Evolution => evolution;
        public TowerRuntimeStats RuntimeStats => runtimeStats;

        private void OnEnable()
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.UpgradeApplied += HandleUpgradeApplied;
                GameManager.Instance.CommanderSelected += HandleCommanderChanged;
            }
        }

        private void OnDisable()
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.UpgradeApplied -= HandleUpgradeApplied;
                GameManager.Instance.CommanderSelected -= HandleCommanderChanged;
            }
        }

        private void OnMouseDown()
        {
            TowerSelectionManager.Instance?.SelectTower(this);
        }

        private void Update()
        {
            if (config == null || GameManager.Instance == null || GameManager.Instance.IsGameOver || isCharging)
            {
                return;
            }

            attackTimer -= Time.deltaTime;
            if (attackTimer > 0f)
            {
                return;
            }

            EnemyHealth target = FindTarget();
            if (target == null)
            {
                return;
            }

            Attack(target);
            attackTimer = Mathf.Max(0.05f, GetCurrentAttackInterval());
        }

        public void Initialize(TowerConfig towerConfig)
        {
            config = towerConfig;
            attackTimer = 0f;
            level = 1;
            experience = 0;
            RefreshStats();

            if (spriteRenderer == null)
            {
                spriteRenderer = GetComponentInChildren<SpriteRenderer>();
            }

            ApplyVisuals(config.sprite, config.visualPrefab, Color.white, config.visualScale);

            if (firePoint == null)
            {
                GameObject firePointObject = new GameObject("FirePoint");
                firePointObject.transform.SetParent(transform, false);
                firePointObject.transform.localPosition = new Vector3(0.35f, 0.1f, 0f);
                firePoint = firePointObject.transform;
            }

            CacheProjectileSprite();
        }

        public void AddHitExperience(EnemyHealth enemy)
        {
            AddExperience(enemy != null && enemy.IsElite ? 2 : 1);
        }

        public void AddExperience(int amount)
        {
            if (amount <= 0 || IsEvolved)
            {
                return;
            }

            int oldLevel = level;
            experience += amount;
            level = experience >= 100 ? 3 : experience >= 40 ? 2 : 1;
            if (level != oldLevel)
            {
                RefreshStats();
                PlayPulseEffect(new Color(1f, 0.92f, 0.35f, 0.9f), 0.45f, 1.25f);
            }

            GameManager.Instance?.NotifyStateChanged();
        }

        public bool CanEvolve(TowerEvolutionDefinition definition, out string reason)
        {
            if (definition == null)
            {
                reason = "No branch selected.";
                return false;
            }

            if (IsEvolved)
            {
                reason = "Already evolved.";
                return false;
            }

            if (definition.baseTowerType != Type)
            {
                reason = "Wrong base tower.";
                return false;
            }

            if (level < definition.requiredLevel && experience < definition.requiredExperience)
            {
                reason = $"Needs Lv{definition.requiredLevel} or {definition.requiredExperience} XP.";
                return false;
            }

            if (GameManager.Instance == null || GameManager.Instance.Gold < definition.evolveCost)
            {
                reason = "Not enough gold.";
                return false;
            }

            reason = string.Empty;
            return true;
        }

        public bool TryEvolve(TowerEvolutionDefinition definition)
        {
            if (!CanEvolve(definition, out _))
            {
                return false;
            }

            if (!GameManager.Instance.TrySpendGold(definition.evolveCost))
            {
                return false;
            }

            evolution = definition;
            comboTarget = null;
            comboStacks = 0;
            RefreshStats();
            ApplyVisuals(evolution.sprite, evolution.visualPrefab, evolution.accentColor, config.visualScale);
            PlayPulseEffect(evolution.accentColor, 0.75f, 1.45f);
            GameManager.Instance.NotifyStateChanged();
            return true;
        }

        public bool HasTag(TowerTag tag)
        {
            if (evolution != null)
            {
                return evolution.HasTag(tag);
            }

            switch (tag)
            {
                case TowerTag.Archer:
                case TowerTag.Physical:
                case TowerTag.SingleTarget:
                    return Type == TowerType.Arrow;
                case TowerTag.Fire:
                case TowerTag.Magic:
                case TowerTag.Area:
                    return Type == TowerType.Flame;
                case TowerTag.Control:
                    return false;
                default:
                    return Type == TowerType.Magic && tag == TowerTag.Magic;
            }
        }

        private EnemyHealth FindTarget()
        {
            TowerTargetPriority priority = evolution != null ? evolution.targetPriority : TowerTargetPriority.ClosestToGoal;
            EnemyHealth bestTarget = null;
            float bestScore = float.NegativeInfinity;
            float rangeSqr = runtimeStats.Range * runtimeStats.Range;

            foreach (EnemyHealth enemy in GameManager.Instance.ActiveEnemies)
            {
                if (enemy == null || !enemy.IsAlive)
                {
                    continue;
                }

                float distanceSqr = (enemy.transform.position - transform.position).sqrMagnitude;
                if (distanceSqr > rangeSqr)
                {
                    continue;
                }

                float score = ScoreTarget(enemy, priority);
                if (score > bestScore)
                {
                    bestScore = score;
                    bestTarget = enemy;
                }
            }

            return bestTarget;
        }

        private float ScoreTarget(EnemyHealth enemy, TowerTargetPriority priority)
        {
            switch (priority)
            {
                case TowerTargetPriority.HighestHealth:
                    return enemy.CurrentHealth;
                case TowerTargetPriority.MarkedThenHighestHealth:
                    return (enemy.IsMarked ? 100000f : 0f) + enemy.CurrentHealth;
                case TowerTargetPriority.MarkedThenClosestToGoal:
                    return (enemy.IsMarked ? 100000f : 0f) + enemy.GoalProgressScore;
                default:
                    return enemy.GoalProgressScore;
            }
        }

        private void Attack(EnemyHealth target)
        {
            FaceTarget(target);

            if (evolution != null && evolution.chargeDelay > 0f)
            {
                StartCoroutine(ChargeThenFire(target, evolution.chargeDelay));
                return;
            }

            FireAt(target);
        }

        private IEnumerator ChargeThenFire(EnemyHealth target, float delay)
        {
            isCharging = true;
            PlayPulseEffect(new Color(1f, 0.95f, 0.45f, 0.9f), delay, 1.18f);
            yield return new WaitForSeconds(delay);
            if (target != null && target.IsAlive)
            {
                FaceTarget(target);
                FireAt(target);
            }

            isCharging = false;
        }

        private void FireAt(EnemyHealth target)
        {
            if (evolution == null)
            {
                FireBasic(target);
                return;
            }

            switch (evolution.branchId)
            {
                case TowerBranchId.BlastFlame:
                    HitSingleTarget(target, false);
                    ApplyExplosion(target.transform.position, runtimeStats.Damage, evolution.explosionRadiusMultiplier, evolution.explosionEdgeDamagePercent);
                    break;
                case TowerBranchId.Burning:
                    HitSingleTarget(target, true);
                    break;
                case TowerBranchId.Frost:
                    HitSingleTarget(target, false);
                    ApplySlowSplash(target);
                    break;
                case TowerBranchId.ChainLightning:
                    ApplyChainLightning(target);
                    break;
                default:
                    HitSingleTarget(target, config.useDamageOverTime);
                    break;
            }
        }

        private void FireBasic(EnemyHealth target)
        {
            if (config.projectilePrefab != null)
            {
                Vector3 origin = firePoint != null ? firePoint.position : transform.position;
                GameObject projectileObject = CreateProjectileObject(origin, Color.white, Vector3.one);
                ProjectileController projectile = projectileObject.AddComponent<ProjectileController>();
                projectile.Initialize(target, runtimeStats, config.useDamageOverTime, this);
            }
            else
            {
                HitSingleTarget(target, config.useDamageOverTime);
            }
        }

        private void HitSingleTarget(EnemyHealth target, bool applyBurn)
        {
            if (target == null || !target.IsAlive)
            {
                return;
            }

            float damage = runtimeStats.Damage;
            if (evolution != null && target.IsElite)
            {
                damage *= 1f + evolution.eliteBonusDamage + GameManager.Instance.GetSniperEliteBonus(this);
            }

            if (evolution != null && evolution.branchId == TowerBranchId.RapidShot)
            {
                if (comboTarget == target)
                {
                    comboStacks = Mathf.Min(comboStacks + 1, GetMaxComboStacks());
                }
                else
                {
                    comboTarget = target;
                    comboStacks = 1;
                }
            }

            if (target.IsMarked && HasTag(TowerTag.Archer))
            {
                damage *= 1.2f;
            }

            float bonusMultiplier = runtimeStats.CritChance > 0f && Random.value < runtimeStats.CritChance ? 2f : 1f;
            target.TakeDamage(damage * bonusMultiplier, this);
            AddHitExperience(target);
            PlayHitEffect(target.transform.position, evolution != null ? evolution.accentColor : Color.white, GetHitEffectScale());

            if (applyBurn)
            {
                float burnDamage = runtimeStats.BurnDamagePerSecond;
                float burnDuration = runtimeStats.BurnDuration;
                int maxStacks = 1;
                if (evolution != null)
                {
                    burnDamage = runtimeStats.Damage * evolution.burnDamageMultiplier;
                    burnDuration = evolution.burnDuration + GameManager.Instance.GetBurnDurationBonus(this);
                    maxStacks = evolution.maxBurnStacks;
                }

                target.ApplyBurn(burnDamage, burnDuration, maxStacks, this);
            }
        }

        private void ApplyExplosion(Vector3 center, float centerDamage, float radiusMultiplier, float edgePercent)
        {
            float radius = 0.85f * Mathf.Max(0.1f, radiusMultiplier) + GameManager.Instance.GetExplosionRadiusBonus(this);
            foreach (EnemyHealth enemy in GameManager.Instance.ActiveEnemies)
            {
                if (enemy == null || !enemy.IsAlive)
                {
                    continue;
                }

                float distance = Vector3.Distance(center, enemy.transform.position);
                if (distance > radius || distance <= 0.01f)
                {
                    continue;
                }

                float t = Mathf.Clamp01(distance / radius);
                float damage = Mathf.Lerp(centerDamage, centerDamage * edgePercent, t);
                enemy.TakeDamage(damage, this);
                AddHitExperience(enemy);
            }

            PlayAreaEffect(center, new Color(1f, 0.32f, 0.04f, 0.55f), radius);
        }

        private void ApplySlowSplash(EnemyHealth target)
        {
            float radius = Mathf.Max(0.05f, evolution.splashRadius);
            foreach (EnemyHealth enemy in GameManager.Instance.ActiveEnemies)
            {
                if (enemy == null || !enemy.IsAlive)
                {
                    continue;
                }

                if (Vector3.Distance(target.transform.position, enemy.transform.position) > radius)
                {
                    continue;
                }

                enemy.ApplySlow(evolution.slowPercent, evolution.slowDuration);
            }

            PlayAreaEffect(target.transform.position, new Color(0.25f, 0.72f, 1f, 0.4f), radius);
        }

        private void ApplyChainLightning(EnemyHealth firstTarget)
        {
            List<EnemyHealth> hitTargets = new List<EnemyHealth>();
            EnemyHealth current = firstTarget;
            float damage = runtimeStats.Damage;
            int maxTargets = Mathf.Max(1, evolution.chainTargets + 1);

            for (int i = 0; i < maxTargets && current != null; i++)
            {
                current.TakeDamage(damage, this);
                AddHitExperience(current);
                PlayHitEffect(current.transform.position, evolution.accentColor, 0.3f);

                if (i > 0)
                {
                    DrawLightning(hitTargets[i - 1].transform.position, current.transform.position);
                }

                hitTargets.Add(current);
                damage *= Mathf.Max(0.05f, 1f - evolution.chainDamageFalloff);
                current = FindNextChainTarget(current, hitTargets);
            }
        }

        private EnemyHealth FindNextChainTarget(EnemyHealth current, List<EnemyHealth> alreadyHit)
        {
            EnemyHealth best = null;
            float bestDistance = float.PositiveInfinity;
            float range = Mathf.Max(0.1f, evolution.chainSearchRange);

            foreach (EnemyHealth enemy in GameManager.Instance.ActiveEnemies)
            {
                if (enemy == null || !enemy.IsAlive || alreadyHit.Contains(enemy))
                {
                    continue;
                }

                float distance = Vector3.Distance(current.transform.position, enemy.transform.position);
                if (distance <= range && distance < bestDistance)
                {
                    bestDistance = distance;
                    best = enemy;
                }
            }

            return best;
        }

        private void FaceTarget(EnemyHealth target)
        {
            Vector3 origin = firePoint != null ? firePoint.position : transform.position;
            Vector3 direction = target.transform.position - origin;
            if (direction.sqrMagnitude > 0.0001f)
            {
                transform.rotation = Quaternion.Euler(0f, 0f, Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg);
            }
        }

        private float GetCurrentAttackInterval()
        {
            float interval = runtimeStats.AttackInterval;
            if (evolution != null && evolution.branchId == TowerBranchId.RapidShot && comboStacks > 0)
            {
                interval /= 1f + comboStacks * evolution.comboAttackSpeedPerStack;
            }

            return interval;
        }

        private int GetMaxComboStacks()
        {
            int max = evolution != null ? evolution.maxComboStacks : 0;
            return max + GameManager.Instance.GetRapidShotExtraStacks(this);
        }

        private float GetHitEffectScale()
        {
            if (evolution == null)
            {
                return 0.22f;
            }

            return evolution.branchId == TowerBranchId.Sniper ? 0.42f : 0.26f;
        }

        private void RefreshStats()
        {
            if (config == null || GameManager.Instance == null)
            {
                return;
            }

            runtimeStats = GameManager.Instance.GetRuntimeStats(config, evolution, level);
        }

        private void ApplyVisuals(Sprite sprite, GameObject visualPrefab, Color accent, Vector2 scale)
        {
            if (spriteRenderer == null)
            {
                spriteRenderer = GetComponentInChildren<SpriteRenderer>();
            }

            if (spriteRenderer != null)
            {
                if (sprite != null)
                {
                    spriteRenderer.sprite = sprite;
                }

                spriteRenderer.color = accent == Color.clear ? Color.white : accent;
                spriteRenderer.transform.localScale = scale;
            }

            for (int i = 0; i < spawnedVisuals.Count; i++)
            {
                if (spawnedVisuals[i] != null)
                {
                    Destroy(spawnedVisuals[i]);
                }
            }

            spawnedVisuals.Clear();
            if (visualPrefab != null)
            {
                GameObject visualInstance = Instantiate(visualPrefab, transform.position, Quaternion.identity, transform);
                visualInstance.name = $"{DisplayName}_Visual";
                spawnedVisuals.Add(visualInstance);
            }
        }

        private void CacheProjectileSprite()
        {
            if (config == null || config.projectilePrefab == null || config.cachedProjectileSprite != null)
            {
                return;
            }

            SpriteRenderer projectileRenderer = config.projectilePrefab.GetComponentInChildren<SpriteRenderer>();
            if (projectileRenderer != null)
            {
                config.cachedProjectileSprite = projectileRenderer.sprite;
            }
        }

        private GameObject CreateProjectileObject(Vector3 origin, Color color, Vector3 scale)
        {
            GameObject projectileObject = new GameObject($"{DisplayName}_Projectile");
            projectileObject.transform.position = origin;

            ProjectileVisual projectileVisual = projectileObject.AddComponent<ProjectileVisual>();
            Sprite projectileSprite = config.cachedProjectileSprite != null ? config.cachedProjectileSprite : config.sprite;
            projectileVisual.Initialize(projectileSprite, color, 40, scale);

            CircleCollider2D collider = projectileObject.AddComponent<CircleCollider2D>();
            collider.isTrigger = true;
            collider.radius = 0.08f;

            return projectileObject;
        }

        private void PlayHitEffect(Vector3 position, Color color, float scale)
        {
            GameObject effect = new GameObject("HitEffect");
            effect.transform.position = position;
            effect.transform.localScale = new Vector3(scale, scale, 1f);
            SpriteRenderer renderer = effect.AddComponent<SpriteRenderer>();
            renderer.sprite = CreateCircleSprite();
            renderer.color = color;
            renderer.sortingOrder = 55;
            AttachAutoDestroy(effect, 0.18f);
        }

        private void PlayAreaEffect(Vector3 position, Color color, float radius)
        {
            GameObject effect = new GameObject("AreaEffect");
            effect.transform.position = position;
            effect.transform.localScale = new Vector3(radius * 2f, radius * 2f, 1f);
            SpriteRenderer renderer = effect.AddComponent<SpriteRenderer>();
            renderer.sprite = CreateCircleSprite();
            renderer.color = color;
            renderer.sortingOrder = 50;
            AttachAutoDestroy(effect, 0.28f);
        }

        private void PlayPulseEffect(Color color, float lifetime, float scale)
        {
            GameObject effect = new GameObject("TowerPulse");
            effect.transform.SetParent(transform, false);
            effect.transform.localScale = new Vector3(scale, scale, 1f);
            SpriteRenderer renderer = effect.AddComponent<SpriteRenderer>();
            renderer.sprite = CreateCircleSprite();
            renderer.color = new Color(color.r, color.g, color.b, 0.35f);
            renderer.sortingOrder = 4;
            AttachAutoDestroy(effect, lifetime);
        }

        private void DrawLightning(Vector3 start, Vector3 end)
        {
            GameObject lineObject = new GameObject("LightningChain");
            LineRenderer line = lineObject.AddComponent<LineRenderer>();
            line.material = new Material(Shader.Find("Sprites/Default"));
            line.positionCount = 2;
            line.SetPosition(0, start);
            line.SetPosition(1, end);
            line.startWidth = 0.05f;
            line.endWidth = 0.02f;
            line.startColor = evolution.accentColor;
            line.endColor = Color.white;
            line.sortingOrder = 58;
            AttachAutoDestroy(lineObject, 0.12f);
        }

        private static void AttachAutoDestroy(GameObject target, float lifetime)
        {
            AutoDestroyAfterDelay autoDestroy = target.GetComponent<AutoDestroyAfterDelay>();
            if (autoDestroy == null)
            {
                autoDestroy = target.AddComponent<AutoDestroyAfterDelay>();
            }

            autoDestroy.SetLifetime(lifetime);
        }

        private static Sprite CreateCircleSprite()
        {
            Texture2D texture = new Texture2D(16, 16);
            Color[] pixels = new Color[256];
            for (int y = 0; y < 16; y++)
            {
                for (int x = 0; x < 16; x++)
                {
                    float distance = Vector2.Distance(new Vector2(x, y), new Vector2(7.5f, 7.5f));
                    pixels[y * 16 + x] = distance <= 7f ? Color.white : Color.clear;
                }
            }

            texture.SetPixels(pixels);
            texture.Apply();
            texture.hideFlags = HideFlags.HideAndDontSave;
            return Sprite.Create(texture, new Rect(0f, 0f, 16f, 16f), new Vector2(0.5f, 0.5f), 16f);
        }

        private void OnDrawGizmosSelected()
        {
            if (config == null)
            {
                return;
            }

            float range = runtimeStats.Range > 0f ? runtimeStats.Range : config.range;
            Gizmos.color = new Color(1f, 0.35f, 0.1f, 0.25f);
            Gizmos.DrawWireSphere(transform.position, range);
        }

        private void HandleUpgradeApplied(UpgradeDefinition _)
        {
            RefreshStats();
        }

        private void HandleCommanderChanged(CommanderDefinition _)
        {
            RefreshStats();
        }
    }
}
