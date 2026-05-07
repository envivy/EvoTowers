using UnityEngine;

namespace EvoTowers.Task1
{
    [RequireComponent(typeof(SpriteRenderer))]
    public class ProjectileVisual : MonoBehaviour
    {
        [SerializeField] private SpriteRenderer spriteRenderer;

        private void Awake()
        {
            if (spriteRenderer == null)
            {
                spriteRenderer = GetComponent<SpriteRenderer>();
            }
        }

        public void Initialize(Sprite sprite, Color color, int sortingOrder, Vector3 scale)
        {
            if (spriteRenderer == null)
            {
                spriteRenderer = GetComponent<SpriteRenderer>();
            }

            spriteRenderer.sprite = sprite;
            spriteRenderer.color = color;
            spriteRenderer.sortingOrder = sortingOrder;
            transform.localScale = scale;
        }
    }
}
