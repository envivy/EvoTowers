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

        public event Action<EnemyHealth, TowerController> Resolved;

        public float HealthProgress => config != null && config.maxHealth > 0f ? health / config.maxHealth : 0f;
        public float CurrentHealth => health;
        public float MaxHealth => config != null ? config.maxHealth : 0f;
        public bool IsAlive => !isResolved && health > 0f;
        public bool IsElite => config != null && config.isElite;
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

            health = Mathf.Max(0f, health - amount);
            UpdateHealthView();

            if (health <= 0f)
            {
                isResolved = true;
                GameManager.Instance?.AddGold(config != null ? config.goldReward : 0);
                AwardKillExperience(source);
                PlayDeathEffect();
                Finish(source);
                return true;
            }

            return false;
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
