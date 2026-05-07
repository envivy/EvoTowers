using UnityEngine;

namespace EvoTowers.Task1
{
    public class ProjectileController : MonoBehaviour
    {
        [SerializeField] private float moveSpeed = 10f;
        [SerializeField] private float maxLifetime = 3f;

        private EnemyHealth target;
        private TowerRuntimeStats runtimeStats;
        private bool applyBurn;
        private TowerController owner;
        private float lifeTimer;

        public void Initialize(EnemyHealth enemyTarget, TowerRuntimeStats towerStats, bool shouldApplyBurn)
        {
            Initialize(enemyTarget, towerStats, shouldApplyBurn, null);
        }

        public void Initialize(EnemyHealth enemyTarget, TowerRuntimeStats towerStats, bool shouldApplyBurn, TowerController sourceTower)
        {
            target = enemyTarget;
            runtimeStats = towerStats;
            applyBurn = shouldApplyBurn;
            owner = sourceTower;
            lifeTimer = maxLifetime;
        }

        private void Update()
        {
            lifeTimer -= Time.deltaTime;
            if (lifeTimer <= 0f)
            {
                Destroy(gameObject);
                return;
            }

            if (target == null || !target.IsAlive)
            {
                Destroy(gameObject);
                return;
            }

            Vector3 destination = target.transform.position;
            Vector3 direction = destination - transform.position;
            float distanceThisFrame = moveSpeed * Time.deltaTime;

            if (direction.sqrMagnitude <= distanceThisFrame * distanceThisFrame)
            {
                HitTarget();
                return;
            }

            transform.position += direction.normalized * distanceThisFrame;
            transform.rotation = Quaternion.Euler(0f, 0f, Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg);
        }

        private void HitTarget()
        {
            if (target != null && target.IsAlive)
            {
                float bonusMultiplier =
                    runtimeStats.CritChance > 0f && Random.value < runtimeStats.CritChance ? 2f : 1f;
                bool killed = target.TakeDamage(runtimeStats.Damage * bonusMultiplier, owner);
                if (owner != null)
                {
                    owner.AddHitExperience(target);
                }

                if (applyBurn)
                {
                    target.ApplyBurn(runtimeStats.BurnDamagePerSecond, runtimeStats.BurnDuration, 1, owner);
                }
            }

            Destroy(gameObject);
        }
    }
}
