using UnityEngine;

public class SharkAI : MonoBehaviour
{
    [Header("Références")]
    public Transform player;

    [Header("Mouvement")]
    public float swimSpeed = 3f;
    public float rotationSpeed = 2f;

    [Header("Attaque")]
    public float attackDistance = 1.5f;  // Distance pour toucher le joueur

    private bool isActive = false;

    void Update()
    {
        if (!isActive || player == null) return;

        // Calculer la direction vers le joueur
        Vector3 direction = (player.position - transform.position).normalized;
        
        // Rotation progressive vers le joueur
        if (direction != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        }

        // Avancer vers le joueur
        transform.position += transform.forward * swimSpeed * Time.deltaTime;

        // Vérifier si le requin a atteint le joueur
        float distance = Vector3.Distance(transform.position, player.position);
        if (distance <= attackDistance)
        {
            AttackPlayer();
        }
    }

    // Appelé par le spawner pour activer le requin
    public void Activate(Transform playerTarget)
    {
        player = playerTarget;
        isActive = true;
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
