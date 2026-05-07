using UnityEngine;

namespace EvoTowers.Task1
{
    public class Week2BattleFlow : MonoBehaviour
    {
        [SerializeField] private WaveManager waveManager;
        private bool isBound;

        private void Awake()
        {
            if (waveManager == null)
            {
                waveManager = FindObjectOfType<WaveManager>();
            }
        }

        private void OnEnable()
        {
            TryBind();
        }

        private void OnDisable()
        {
            if (isBound && GameManager.Instance != null)
            {
                GameManager.Instance.CommanderSelected -= HandleCommanderSelected;
                GameManager.Instance.UpgradeApplied -= HandleUpgradeApplied;
            }

            isBound = false;
        }

        private void Update()
        {
            if (!isBound)
            {
                TryBind();
            }
        }

        public void NotifyWaveStarted()
        {
            if (GameManager.Instance == null)
            {
                return;
            }

            GameManager.Instance.SetFlowState(GameFlowState.Battle);
        }

        private void HandleCommanderSelected(CommanderDefinition _)
        {
            GameManager.Instance?.SetFlowState(GameFlowState.Prepare);
        }

        private void HandleUpgradeApplied(UpgradeDefinition _)
        {
            if (GameManager.Instance == null || waveManager == null)
            {
                return;
            }

            if (waveManager.HasMoreWaves)
            {
                Time.timeScale = 1f;
                GameManager.Instance.SetFlowState(GameFlowState.Battle);
                waveManager.StartNextWave();
            }
            else
            {
                GameManager.Instance.CloseUpgradeDraftToPrepare();
            }
        }

        private void TryBind()
        {
            if (isBound || GameManager.Instance == null)
            {
                return;
            }

            GameManager.Instance.CommanderSelected += HandleCommanderSelected;
            GameManager.Instance.UpgradeApplied += HandleUpgradeApplied;
            isBound = true;
        }
    }
}
