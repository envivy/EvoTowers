using System;
using System.Collections.Generic;
using UnityEngine;

namespace EvoTowers.Task1
{
    public class EnemyHealth : MonoBehaviour
    {
        [SerializeField] private LineRenderer healthFill;
        [SerializeField] private SpriteRenderer spriteRenderer;
        [SerializeField] private GameObject deathEffectPrefab;

        private readonly Dictionary<TowerController, float> damageContributors = new Dictionary<TowerController, float>();
        private readonly Dictionary<EnemyStatusType, GameObject> statusMarkers = new Dictionary<EnemyStatusType, GameObject>();
        private EnemyConfig config;
        private EnemyPathFollower pathFollower;
        private float health;
        private float dotTimer;
        private float dotDamagePerSecond;
        private float slowTimer;
        private float slowPercent;
        private float markTimer;
        private TowerController markSource;
        private int burnStacks;
        private int maxBurnStacks = 1;
        private bool isResolved;
        private float healCooldown;
        private bool bossSummon75Triggered;
        private bool bossSummon50Triggered;
        private bool bossSummon25Triggered;

        public event Action<EnemyHealth, TowerController> Resolved;
        public event Action<EnemyHealth, float> BossHealthThresholdReached;

        public float HealthProgress => config != null && config.maxHealth > 0f ? health / config.maxHealth : 0f;
        public float CurrentHealth => health;
        public float MaxHealth => config != null ? config.maxHealth : 0f;
        public bool IsAlive => !isResolved && health > 0f;
        public bool IsElite => config != null && config.isElite;
        public bool IsBoss => config != null && config.isBoss;
        public bool IsMarked => markTimer > 0f;
        public TowerController MarkSource => markSource;
        public float MoveSpeedMultiplier => slowTimer > 0f ? Mathf.Max(0.15f, 1f - slowPercent) : 1f;
        public float GoalProgressScore => pathFollower != null ? pathFollower.GoalProgressScore : 0f;

        private void Awake()
        {
            pathFollower = GetComponent<EnemyPathFollower>();
        }

        private void Update()
        {
            if (isResolved)
            {
                return;
            }

            TickBurn();
            TickSlow();
            TickMark();
            TickHealing();
        }

        public void Initialize(EnemyConfig enemyConfig)
        {
            config = enemyConfig;
            health = config.maxHealth;
            ClampHealthLineSize();

            if (spriteRenderer != null && config.sprite != null)
            {
                spriteRenderer.sprite = config.sprite;
                spriteRenderer.transform.localScale = config.visualScale;
            }

            Animator animator = GetComponentInChildren<Animator>();
            if (animator != null && config.animatorController != null)
            {
                animator.runtimeAnimatorController = config.animatorController;
            }

            UpdateHealthView();
        }

        public void Heal(float amount)
        {
            if (isResolved || amount <= 0f || config == null)
            {
                return;
            }

            health = Mathf.Min(config.maxHealth, health + amount);
            UpdateHealthView();
            PlayHealEffect();
        }

        public void TakeDamage(float amount)
        {
            ApplyDamage(amount, null);
        }

        public bool TakeDamage(float amount, TowerController source)
        {
            return ApplyDamage(amount, source);
        }

        public void ApplyDamageOverTime(float damagePerSecond, float duration)
        {
            ApplyBurn(damagePerSecond, duration, 1, null);
        }

        public void ApplyBurn(float damagePerSecond, float duration, int maxStacks, TowerController source)
        {
            dotDamagePerSecond = Mathf.Max(dotDamagePerSecond, damagePerSecond);
            dotTimer = Mathf.Max(dotTimer, duration);
            maxBurnStacks = Mathf.Max(1, maxStacks);
            burnStacks = Mathf.Clamp(burnStacks + 1, 1, maxBurnStacks);

            if (source != null && !damageContributors.ContainsKey(source))
            {
                damageContributors[source] = 0f;
            }

            SetStatusMarker(EnemyStatusType.Burning, true, new Color(1f, 0.33f, 0.08f, 0.95f));
        }

        public void ApplySlow(float percent, float duration)
        {
            slowPercent = Mathf.Max(slowPercent, percent);
            slowTimer = Mathf.Max(slowTimer, duration);
            SetStatusMarker(EnemyStatusType.Slowed, true, new Color(0.25f, 0.75f, 1f, 0.95f));
        }

        public void ApplyMark(float duration, TowerController source)
        {
            markTimer = Mathf.Max(markTimer, duration);
            markSource = source;
            SetStatusMarker(EnemyStatusType.Marked, true, new Color(1f, 0.92f, 0.18f, 0.95f));
        }

        public void ResolveLeak()
        {
            if (isResolved)
            {
                return;
            }

            isResolved = true;
            GameManager.Instance?.LoseLives(config != null ? config.lifeDamage : 1);
            Finish();
        }

        private void TickBurn()
        {
            if (dotTimer <= 0f)
            {
                return;
            }

            dotTimer -= Time.deltaTime;
            ApplyDamage(dotDamagePerSecond * Mathf.Max(1, burnStacks) * Time.deltaTime, null);
            if (dotTimer <= 0f)
            {
                burnStacks = 0;
                SetStatusMarker(EnemyStatusType.Burning, false, Color.white);
            }
        }

        private void TickSlow()
        {
            if (slowTimer <= 0f)
            {
                return;
            }

            slowTimer -= Time.deltaTime;
            if (slowTimer <= 0f)
            {
                slowPercent = 0f;
                SetStatusMarker(EnemyStatusType.Slowed, false, Color.white);
            }
        }

        private void TickMark()
        {
            if (markTimer <= 0f)
            {
                return;
            }

            markTimer -= Time.deltaTime;
            if (markTimer <= 0f)
            {
                markSource = null;
                SetStatusMarker(EnemyStatusType.Marked, false, Color.white);
            }
        }

        private bool ApplyDamage(float amount, TowerController source)
        {
            if (isResolved || amount <= 0f)
            {
                return false;
            }

            if (source != null)
            {
                if (!damageContributors.ContainsKey(source))
                {
                    damageContributors[source] = 0f;
                }

                damageContributors[source] += amount;
            }

            float armor = config != null ? config.armorPercent : 0f;
            if (config != null && config.isBoss && health <= config.maxHealth * 0.5f)
            {
                armor += 0.1f;
            }

            float finalAmount = amount * (1f - Mathf.Clamp01(armor));
            health = Mathf.Max(0f, health - finalAmount);
            UpdateHealthView();
            CheckBossThresholds();
            GameAudio.Instance?.PlayHit();

            if (health <= 0f)
            {
                isResolved = true;
                GameManager.Instance?.NotifyEnemyKilled(this);
                GameManager.Instance?.AddGold(config != null ? config.goldReward : 0);
                AwardKillExperience(source);
                PlayDeathEffect();
                GameAudio.Instance?.PlayDeath();
                Finish(source);
                return true;
            }

            return false;
        }

        private void TickHealing()
        {
            if (config == null || !config.isHealer)
            {
                return;
            }

            healCooldown -= Time.deltaTime;
            if (healCooldown > 0f)
            {
                return;
            }

            healCooldown = Mathf.Max(0.1f, config.healInterval);
            EnemyHealth target = FindHealTarget();
            if (target != null)
            {
                target.Heal(config.healAmount);
                PlayHealBeam(target);
            }
        }

        private EnemyHealth FindHealTarget()
        {
            if (GameManager.Instance == null)
            {
                return null;
            }

            EnemyHealth best = null;
            float lowestRatio = 1f;
            foreach (EnemyHealth enemy in GameManager.Instance.ActiveEnemies)
            {
                if (enemy == null || enemy == this || !enemy.IsAlive || enemy.IsBoss)
                {
                    continue;
                }

                if (Vector3.Distance(transform.position, enemy.transform.position) > config.healRange)
                {
                    continue;
                }

                float ratio = enemy.MaxHealth > 0f ? enemy.CurrentHealth / enemy.MaxHealth : 1f;
                if (ratio < lowestRatio)
                {
                    lowestRatio = ratio;
                    best = enemy;
                }
            }

            return best;
        }

        private void CheckBossThresholds()
        {
            if (config == null || !config.isBoss || health <= 0f)
            {
                return;
            }

            float progress = HealthProgress;
            if (!bossSummon75Triggered && progress <= 0.75f)
            {
                bossSummon75Triggered = true;
                BossHealthThresholdReached?.Invoke(this, 0.75f);
            }

            if (!bossSummon50Triggered && progress <= 0.5f)
            {
                bossSummon50Triggered = true;
                BossHealthThresholdReached?.Invoke(this, 0.5f);
                PlayHealEffect();
            }

            if (!bossSummon25Triggered && progress <= 0.25f)
            {
                bossSummon25Triggered = true;
                BossHealthThresholdReached?.Invoke(this, 0.25f);
            }
        }

        private void Finish(TowerController killer = null)
        {
            GameManager.Instance?.UnregisterEnemy(this);
            Resolved?.Invoke(this, killer);
            Destroy(gameObject);
        }

        private void UpdateHealthView()
        {
            if (healthFill != null)
            {
                ClampHealthLineSize();
                float progress = Mathf.Clamp01(HealthProgress);
                healthFill.SetPosition(0, new Vector3(-0.28f, 0f, 0f));
                healthFill.SetPosition(1, new Vector3(-0.28f + 0.56f * progress, 0f, 0f));
            }
        }

        private void ClampHealthLineSize()
        {
            if (healthFill == null)
            {
                return;
            }

            healthFill.useWorldSpace = false;
            healthFill.startWidth = 0.055f;
            healthFill.endWidth = 0.055f;
        }

        private void PlayDeathEffect()
        {
            if (deathEffectPrefab == null)
            {
                return;
            }

            GameObject effectInstance = Instantiate(deathEffectPrefab, transform.position, Quaternion.identity);
            effectInstance.name = "DeathEffect";
            AutoDestroyAfterDelay autoDestroy = effectInstance.GetComponent<AutoDestroyAfterDelay>();
            if (autoDestroy == null)
            {
                autoDestroy = effectInstance.AddComponent<AutoDestroyAfterDelay>();
            }

            autoDestroy.SetLifetime(0.35f);
        }

        private void PlayHealEffect()
        {
            GameObject effect = new GameObject("HealEffect");
            effect.transform.SetParent(transform, false);
            effect.transform.localScale = new Vector3(0.42f, 0.42f, 1f);
            SpriteRenderer renderer = effect.AddComponent<SpriteRenderer>();
            renderer.sprite = CreateStatusSprite();
            renderer.color = new Color(0.25f, 1f, 0.35f, 0.65f);
            renderer.sortingOrder = 56;
            AutoDestroyAfterDelay autoDestroy = effect.AddComponent<AutoDestroyAfterDelay>();
            autoDestroy.SetLifetime(0.35f);
        }

        private void PlayHealBeam(EnemyHealth target)
        {
            GameObject lineObject = new GameObject("HealBeam");
            LineRenderer line = lineObject.AddComponent<LineRenderer>();
            line.material = new Material(Shader.Find("Sprites/Default"));
            line.positionCount = 2;
            line.SetPosition(0, transform.position);
            line.SetPosition(1, target.transform.position);
            line.startWidth = 0.04f;
            line.endWidth = 0.02f;
            line.startColor = new Color(0.25f, 1f, 0.35f, 0.75f);
            line.endColor = new Color(0.75f, 1f, 0.75f, 0.4f);
            line.sortingOrder = 57;
            AutoDestroyAfterDelay autoDestroy = lineObject.AddComponent<AutoDestroyAfterDelay>();
            autoDestroy.SetLifetime(0.18f);
        }

        private void AwardKillExperience(TowerController killer)
        {
            int killExperience = IsElite ? 10 : 5;
            int assistExperience = Mathf.Max(1, killExperience / 2);

            foreach (TowerController contributor in damageContributors.Keys)
            {
                if (contributor == null || contributor == killer)
                {
                    continue;
                }

                contributor.AddExperience(assistExperience);
            }

            if (killer != null)
            {
                killer.AddExperience(killExperience);
            }
        }

        private void SetStatusMarker(EnemyStatusType status, bool active, Color color)
        {
            if (!active)
            {
                if (statusMarkers.TryGetValue(status, out GameObject existing) && existing != null)
                {
                    Destroy(existing);
                }

                statusMarkers.Remove(status);
                return;
            }

            if (statusMarkers.TryGetValue(status, out GameObject marker) && marker != null)
            {
                SpriteRenderer existingRenderer = marker.GetComponent<SpriteRenderer>();
                if (existingRenderer != null)
                {
                    existingRenderer.color = color;
                }

                return;
            }

            GameObject markerObject = new GameObject($"{status}Status");
            markerObject.transform.SetParent(transform, false);
            markerObject.transform.localPosition = StatusOffset(status);
            markerObject.transform.localScale = new Vector3(0.18f, 0.18f, 1f);
            SpriteRenderer renderer = markerObject.AddComponent<SpriteRenderer>();
            renderer.sprite = CreateStatusSprite();
            renderer.color = color;
            renderer.sortingOrder = 45;
            statusMarkers[status] = markerObject;
        }

        private static Vector3 StatusOffset(EnemyStatusType status)
        {
            switch (status)
            {
                case EnemyStatusType.Burning:
                    return new Vector3(-0.22f, 0.46f, 0f);
                case EnemyStatusType.Slowed:
                    return new Vector3(0f, 0.5f, 0f);
                case EnemyStatusType.Marked:
                    return new Vector3(0.22f, 0.46f, 0f);
                default:
                    return new Vector3(0f, 0.5f, 0f);
            }
        }

        private static Sprite CreateStatusSprite()
        {
            Texture2D texture = new Texture2D(8, 8);
            Color[] pixels = new Color[64];
            for (int y = 0; y < 8; y++)
            {
                for (int x = 0; x < 8; x++)
                {
                    float distance = Vector2.Distance(new Vector2(x, y), new Vector2(3.5f, 3.5f));
                    pixels[y * 8 + x] = distance <= 3.25f ? Color.white : Color.clear;
                }
            }

            texture.SetPixels(pixels);
            texture.Apply();
            texture.hideFlags = HideFlags.HideAndDontSave;
            return Sprite.Create(texture, new Rect(0f, 0f, 8f, 8f), new Vector2(0.5f, 0.5f), 8f);
        }
    }
}
