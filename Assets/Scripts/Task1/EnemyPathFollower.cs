using UnityEngine;

namespace EvoTowers.Task1
{
    [RequireComponent(typeof(EnemyHealth))]
    public class EnemyPathFollower : MonoBehaviour
    {
        [SerializeField] private float arriveDistance = 0.04f;

        private PathRoute route;
        private EnemyConfig config;
        private EnemyHealth health;
        private int waypointIndex;

        public float GoalProgressScore
        {
            get
            {
                if (route == null || route.Count == 0)
                {
                    return 0f;
                }

                return -route.EstimateRemainingDistance(waypointIndex, transform.position);
            }
        }

        private void Awake()
        {
            health = GetComponent<EnemyHealth>();
        }

        private void Update()
        {
            if (route == null || config == null || !health.IsAlive)
            {
                return;
            }

            Transform target = route.GetWaypoint(waypointIndex);
            if (target == null)
            {
                health.ResolveLeak();
                return;
            }

            float speedMultiplier = health != null ? health.MoveSpeedMultiplier : 1f;
            Vector3 nextPosition = Vector3.MoveTowards(transform.position, target.position, config.moveSpeed * speedMultiplier * Time.deltaTime);
            Vector3 direction = nextPosition - transform.position;
            transform.position = nextPosition;

            if (direction.sqrMagnitude > 0.0001f)
            {
                float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
                transform.rotation = Quaternion.Euler(0f, 0f, angle);
            }

            if (Vector3.Distance(transform.position, target.position) <= arriveDistance)
            {
                waypointIndex++;
                if (waypointIndex >= route.Count)
                {
                    health.ResolveLeak();
                }
            }
        }

        public void Initialize(PathRoute pathRoute, EnemyConfig enemyConfig)
        {
            route = pathRoute;
            config = enemyConfig;
            waypointIndex = 0;

            if (route != null && route.Count > 0)
            {
                transform.position = route.GetWaypointPosition(0);
                waypointIndex = 1;
            }
        }
    }
}
