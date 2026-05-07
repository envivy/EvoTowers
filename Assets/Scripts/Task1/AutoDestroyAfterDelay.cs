using UnityEngine;

namespace EvoTowers.Task1
{
    public class AutoDestroyAfterDelay : MonoBehaviour
    {
        [SerializeField] private float lifetime = 1.2f;

        private void OnEnable()
        {
            Destroy(gameObject, lifetime);
        }

        public void SetLifetime(float seconds)
        {
            lifetime = Mathf.Max(0.05f, seconds);
        }
    }
}
