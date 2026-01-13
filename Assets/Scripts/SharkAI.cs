using UnityEngine;

public class SharkAI : MonoBehaviour
{
    public Transform player;
    public Transform[] waypoints;         
    public float waypointReachDistance = 0.5f;  
    public float swimSpeed = 3f;
    public float rotationSpeed = 2f;
    public float attackDistance = 1.5f;

    private bool isActive = false;
    private int currentWaypointIndex = 0;
    private bool waypointsCompleted = false;

    void Update()
    {
        if (!isActive || player == null) return;

        Transform target;

        if (!waypointsCompleted && waypoints != null && waypoints.Length > 0 && currentWaypointIndex < waypoints.Length)
        {
            target = waypoints[currentWaypointIndex];

            float distanceToWaypoint = Vector3.Distance(transform.position, target.position);
            if (distanceToWaypoint <= waypointReachDistance)
            {
                currentWaypointIndex++;

                if (currentWaypointIndex >= waypoints.Length)
                {
                    waypointsCompleted = true;
                }
                return;
            }
        }
        else
        {
            target = player;
            waypointsCompleted = true;
        }

        Vector3 direction = (target.position - transform.position).normalized;
        
        if (direction != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        }

        transform.position += transform.forward * swimSpeed * Time.deltaTime;

        if (waypointsCompleted)
        {
            float distance = Vector3.Distance(transform.position, player.position);
            if (distance <= attackDistance)
            {
                AttackPlayer();
            }
        }
    }

    public void Activate(Transform playerTarget)
    {
        player = playerTarget;
        isActive = true;
        currentWaypointIndex = 0;
        waypointsCompleted = (waypoints == null || waypoints.Length == 0);
    }

    private void AttackPlayer()
    {
        isActive = false;

        if (GameOverManager.Instance != null)
        {
            GameOverManager.Instance.TriggerSharkGameOver();
        }

        Destroy(gameObject, 0.5f);
    }
}
