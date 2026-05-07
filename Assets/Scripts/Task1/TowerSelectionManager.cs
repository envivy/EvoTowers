using System;
using UnityEngine;

namespace EvoTowers.Task1
{
    public class TowerSelectionManager : MonoBehaviour
    {
        public static TowerSelectionManager Instance { get; private set; }

        public event Action<TowerController> SelectedTowerChanged;

        public TowerController SelectedTower { get; private set; }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
        }

        private void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
            }
        }

        public void SelectTower(TowerController tower)
        {
            if (SelectedTower == tower)
            {
                return;
            }

            SelectedTower = tower;
            SelectedTowerChanged?.Invoke(SelectedTower);
        }
    }
}
