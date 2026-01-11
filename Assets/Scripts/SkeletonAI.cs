using UnityEngine;
using UnityEngine.AI;

public class SkeletonAI : MonoBehaviour
{
    public Animator animator;
    public Transform player;
    public float attackDistance = 2f;

    [Header("Attaque")]
    public float attackDamageDelay = 0.5f;  // Délai avant que l'attaque touche
    public float attackCooldown = 1.5f;     // Temps entre les attaques

    private NavMeshAgent agent;
    private bool alive = true;
    private bool isAttacking = false;
    private float lastAttackTime = -999f;
    
    public bool IsAlive => alive;

    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
    }

    void Update()
    {
        if(!alive) return;

        float distance = Vector3.Distance(transform.position, player.position);

        if(distance > attackDistance)
        {
            agent.isStopped = false;
            agent.SetDestination(player.position);
            animator.SetBool("IsWalking", true);
            animator.SetBool("IsAttacking", false);
            isAttacking = false;
        }
        else
        {
            agent.isStopped = true;
            animator.SetBool("IsWalking", false);
            animator.SetBool("IsAttacking", true);

            // Lancer l'attaque si pas en cooldown
            if (!isAttacking && Time.time - lastAttackTime >= attackCooldown)
            {
                StartCoroutine(PerformAttack());
            }
        }
    }

    private System.Collections.IEnumerator PerformAttack()
    {
        isAttacking = true;
        lastAttackTime = Time.time;

        // Attendre le moment de l'impact de l'animation
        yield return new WaitForSeconds(attackDamageDelay);

        if (!alive) yield break;

        // Vérifier si le joueur est toujours à portée
        float distance = Vector3.Distance(transform.position, player.position);
        if (distance <= attackDistance * 1.2f)
        {
            // Le joueur est touché - Game Over Squelette!
            if (GameOverManager.Instance != null)
            {
                GameOverManager.Instance.TriggerSkeletonGameOver();
            }
        }

        yield return new WaitForSeconds(attackCooldown - attackDamageDelay);
        isAttacking = false;
    }

    public void Die()
    {
        alive = false;
        isAttacking = false;
        agent.isStopped = true;
        animator.SetTrigger("IsDead");
        Destroy(gameObject, 3f);
    }
}
