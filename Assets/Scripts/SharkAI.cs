using UnityEngine;

public class SharkAI : MonoBehaviour
{
    [Header("Références")]
    public Transform player;

    [Header("Waypoints")]
    public Transform[] waypoints;           // Points de passage avant d'aller vers le joueur
    public float waypointReachDistance = 0.5f;  // Distance pour considérer un waypoint atteint

    [Header("Mouvement")]
    public float swimSpeed = 3f;
    public float rotationSpeed = 2f;

    [Header("Attaque")]
    public float attackDistance = 1.5f;  // Distance pour toucher le joueur

    private bool isActive = false;
    private int currentWaypointIndex = 0;
    private bool waypointsCompleted = false;

    void Update()
    {
        if (!isActive || player == null) return;

        Transform target;

        // Déterminer la cible : waypoint ou joueur
        if (!waypointsCompleted && waypoints != null && waypoints.Length > 0 && currentWaypointIndex < waypoints.Length)
        {
            target = waypoints[currentWaypointIndex];

            // Vérifier si on a atteint le waypoint actuel
            float distanceToWaypoint = Vector3.Distance(transform.position, target.position);
            if (distanceToWaypoint <= waypointReachDistance)
            {
                currentWaypointIndex++;

                // Si tous les waypoints sont passés, aller vers le joueur
                if (currentWaypointIndex >= waypoints.Length)
                {
                    waypointsCompleted = true;
                }
                return;
            }
        }
        else
        {
            // Tous les waypoints sont passés, cibler le joueur
            target = player;
            waypointsCompleted = true;
        }

        // Calculer la direction vers la cible
        Vector3 direction = (target.position - transform.position).normalized;
        
        // Rotation progressive vers la cible
        if (direction != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        }

        // Avancer vers la cible
        transform.position += transform.forward * swimSpeed * Time.deltaTime;

        // Vérifier si le requin a atteint le joueur (seulement après les waypoints)
        if (waypointsCompleted)
        {
            float distance = Vector3.Distance(transform.position, player.position);
            if (distance <= attackDistance)
            {
                AttackPlayer();
            }
        }
    }

    // Appelé par le spawner pour activer le requin
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

        // Déclencher le Game Over
        if (GameOverManager.Instance != null)
        {
            GameOverManager.Instance.TriggerGameOver();
        }

        // Optionnel : détruire le requin
        Destroy(gameObject, 0.5f);
    }
}
