using System;
using UnityEngine;

namespace EvoTowers.Task1
{
    public class BuildManager : MonoBehaviour
    {
        public static BuildManager Instance { get; private set; }

        [SerializeField] private TowerType selectedTowerType = TowerType.Arrow;

        public event Action SelectedTowerChanged;
        public TowerType SelectedTowerType => selectedTowerType;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            if (TowerSelectionManager.Instance == null && FindObjectOfType<TowerSelectionManager>() == null)
            {
                gameObject.AddComponent<TowerSelectionManager>();
            }
        }

        private void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
            }
        }

        public void SelectTower(int towerTypeValue)
        {
            selectedTowerType = (TowerType)towerTypeValue;
            SelectedTowerChanged?.Invoke();
        }

        public void TryBuildOn(BuildSlot slot)
        {
            if (slot == null || !slot.IsEmpty || GameManager.Instance == null)
            {
                return;
            }

            if (GameManager.Instance.FlowState == GameFlowState.Upgrade || GameManager.Instance.FlowState == GameFlowState.GameOver || GameManager.Instance.FlowState == GameFlowState.Victory)
            {
                return;
            }

            TowerConfig config = GameManager.Instance.GetTowerConfig(selectedTowerType);
            if (config == null)
            {
                Debug.LogWarning($"Missing tower config for {selectedTowerType}.");
                return;
            }

            int buildCost = GameManager.Instance.GetBuildCost(config);
            if (!GameManager.Instance.TrySpendGold(buildCost))
            {
                Debug.Log($"Not enough gold to build {config.displayName}. Need {buildCost}.");
                return;
            }

            if (!slot.TryPlaceTower(config))
            {
                GameManager.Instance.AddGold(buildCost);
            }
        }
    }
}
