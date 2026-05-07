using System.Collections.Generic;
using UnityEngine;

namespace EvoTowers.Task1
{
    public class PathRoute : MonoBehaviour
    {
        [SerializeField] private List<Transform> waypoints = new List<Transform>();

        public int Count => waypoints.Count;

        public Transform GetWaypoint(int index)
        {
            if (index < 0 || index >= waypoints.Count)
            {
                return null;
            }

            return waypoints[index];
        }

        public Vector3 GetWaypointPosition(int index)
        {
            Transform waypoint = GetWaypoint(index);
            return waypoint != null ? waypoint.position : transform.position;
        }

        public float EstimateRemainingDistance(int waypointIndex, Vector3 currentPosition)
        {
            if (waypoints.Count == 0)
            {
                return 0f;
            }

            int clampedIndex = Mathf.Clamp(waypointIndex, 0, waypoints.Count - 1);
            float distance = Vector3.Distance(currentPosition, waypoints[clampedIndex].position);

            for (int i = clampedIndex; i < waypoints.Count - 1; i++)
            {
                distance += Vector3.Distance(waypoints[i].position, waypoints[i + 1].position);
            }

            return distance;
        }

        public void SetWaypoints(List<Transform> routeWaypoints)
        {
            waypoints = routeWaypoints;
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.yellow;
            for (int i = 0; i < waypoints.Count; i++)
            {
                if (waypoints[i] == null)
                {
                    continue;
                }

                Gizmos.DrawSphere(waypoints[i].position, 0.12f);

                if (i + 1 < waypoints.Count && waypoints[i + 1] != null)
                {
                    Gizmos.DrawLine(waypoints[i].position, waypoints[i + 1].position);
                }
            }
        }
    }
}
