using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace EvoTowers.Task1
{
    public class WaveManager : MonoBehaviour
    {
        [SerializeField] private PathRoute route;
        [SerializeField] private GameObject enemyPrefab;
        [SerializeField] private List<WaveDefinition> waves = new List<WaveDefinition>();
        [SerializeField] private Transform enemyRoot;

        private int currentWaveIndex = -1;
        private int livingEnemies;
        private bool isSpawning;
        private bool hasStarted;
        private bool waveFullyResolved;
        private WaveDefinition activeWave;

        public int CurrentWaveNumber => Mathf.Clamp(currentWaveIndex + 1, 0, waves.Count);
        public int TotalWaves => waves.Count;
        public int LivingEnemies => livingEnemies;
        public bool IsWaveRunning => isSpawning || livingEnemies > 0;
        public bool HasMoreWaves => currentWaveIndex + 1 < waves.Count;
        public bool HasStarted => hasStarted;

        private void Awake()
        {
            if (enemyRoot == null)
            {
                enemyRoot = transform;
            }
        }

        public void StartNextWave()
        {
            if (GameManager.Instance != null && GameManager.Instance.IsGameOver)
            {
                return;
            }

            if (IsWaveRunning || !HasMoreWaves)
            {
                return;
            }

            hasStarted = true;
            waveFullyResolved = false;
            currentWaveIndex++;
            activeWave = waves[currentWaveIndex];
            GameManager.Instance?.NotifyWaveStarted(CurrentWaveNumber, activeWave.isBossWave);
            if (activeWave.isBossWave)
            {
                GameAudio.Instance?.PlayBoss();
            }
            else
            {
                GameAudio.Instance?.PlayWave();
            }
            StartCoroutine(SpawnWave(waves[currentWaveIndex]));
        }

        private IEnumerator SpawnWave(WaveDefinition wave)
        {
            isSpawning = true;
            GameManager.Instance?.NotifyStateChanged();

            foreach (WaveSpawnGroup group in wave.groups)
            {
                if (group.delayBeforeGroup > 0f)
                {
                    yield return new WaitForSeconds(group.delayBeforeGroup);
                }

                for (int i = 0; i < group.count; i++)
                {
                    SpawnEnemy(group.enemyType);

                    if (group.interval > 0f)
                    {
                        yield return new WaitForSeconds(group.interval);
                    }
                }
            }

            isSpawning = false;
            GameManager.Instance?.NotifyStateChanged();
            TryResolveWaveEnd();
        }

        private void SpawnEnemy(EnemyType enemyType)
        {
            if (enemyPrefab == null || route == null || GameManager.Instance == null)
            {
                Debug.LogWarning("WaveManager is missing enemy prefab, route, or GameManager.");
                return;
            }

            EnemyConfig config = GameManager.Instance.GetEnemyConfig(enemyType);
            if (config == null)
            {
                Debug.LogWarning($"Missing enemy config for {enemyType}.");
                return;
            }

            GameObject enemyObject = Instantiate(enemyPrefab, route.GetWaypointPosition(0), Quaternion.identity, enemyRoot);
            enemyObject.name = config.displayName;

            EnemyPathFollower follower = enemyObject.GetComponent<EnemyPathFollower>();
            EnemyHealth health = enemyObject.GetComponent<EnemyHealth>();

            health.Initialize(config);
            follower.Initialize(route, config);
            health.Resolved += HandleEnemyResolved;
            health.BossHealthThresholdReached += HandleBossHealthThresholdReached;

            livingEnemies++;
            GameManager.Instance.RegisterEnemy(health);
        }

        private void HandleEnemyResolved(EnemyHealth enemy, TowerController killer)
        {
            if (enemy != null)
            {
                enemy.Resolved -= HandleEnemyResolved;
                enemy.BossHealthThresholdReached -= HandleBossHealthThresholdReached;
            }

            livingEnemies = Mathf.Max(0, livingEnemies - 1);
            GameManager.Instance?.NotifyStateChanged();
            TryResolveWaveEnd();
        }

        private void TryResolveWaveEnd()
        {
            if (waveFullyResolved || isSpawning || livingEnemies > 0 || !hasStarted)
            {
                return;
            }

            waveFullyResolved = true;
            if (activeWave != null)
            {
                GameManager.Instance?.NotifyWaveCompleted(CurrentWaveNumber, activeWave.clearReward);
            }

            if (HasMoreWaves)
            {
                if (activeWave != null && activeWave.triggersUpgradeDraft)
                {
                    GameManager.Instance?.OpenUpgradeDraft(3);
                }
                else
                {
                    GameManager.Instance?.CloseUpgradeDraftToPrepare();
                }
            }
            else
            {
                GameManager.Instance?.WinGame();
            }
        }

        private void HandleBossHealthThresholdReached(EnemyHealth boss, float threshold)
        {
            if (boss == null || !boss.IsAlive)
            {
                return;
            }

            StartCoroutine(SpawnBossSupport(threshold));
        }

        private IEnumerator SpawnBossSupport(float threshold)
        {
            EnemyType supportType = threshold <= 0.25f ? EnemyType.Fast : EnemyType.Basic;
            int count = threshold <= 0.5f ? 6 : 4;
            for (int i = 0; i < count; i++)
            {
                SpawnEnemy(supportType);
                yield return new WaitForSeconds(0.4f);
            }
        }
    }
}
