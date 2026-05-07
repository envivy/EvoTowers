using UnityEngine;

namespace EvoTowers.Task1
{
    public class BuildSlot : MonoBehaviour
    {
        [SerializeField] private SpriteRenderer markerRenderer;
        [SerializeField] private Color normalColor = new Color(0.2f, 0.8f, 1f, 0.45f);
        [SerializeField] private Color occupiedColor = new Color(0.3f, 0.3f, 0.3f, 0.3f);
        [SerializeField] private GameObject buildEffectPrefab;

        private TowerController builtTower;

        public bool IsEmpty => builtTower == null;

        private void Awake()
        {
            RefreshView();
        }

        private void OnMouseDown()
        {
            if (!IsEmpty && builtTower != null)
            {
                TowerSelectionManager.Instance?.SelectTower(builtTower);
            }
            else if (BuildManager.Instance != null)
            {
                BuildManager.Instance.TryBuildOn(this);
            }
        }

        public bool TryPlaceTower(TowerConfig config)
        {
            if (!IsEmpty || config == null)
            {
                return false;
            }

            GameObject towerObject = new GameObject(config.displayName);
            towerObject.transform.SetParent(transform);
            towerObject.transform.localPosition = Vector3.zero;
            towerObject.transform.localRotation = Quaternion.identity;

            SpriteRenderer renderer = towerObject.AddComponent<SpriteRenderer>();
            renderer.sortingOrder = 5;

            TowerController tower = towerObject.AddComponent<TowerController>();
            builtTower = tower;
            tower.Initialize(config);

            if (buildEffectPrefab != null)
            {
                GameObject effectInstance = Instantiate(buildEffectPrefab, transform.position, Quaternion.identity);
                effectInstance.name = "BuildEffect";
                AttachAutoDestroy(effectInstance, 1.2f);
            }

            RefreshView();
            return true;
        }

        private void RefreshView()
        {
            if (markerRenderer != null)
            {
                markerRenderer.color = IsEmpty ? normalColor : occupiedColor;
            }
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
    }
}
