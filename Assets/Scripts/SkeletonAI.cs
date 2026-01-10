using UnityEngine;
using UnityEngine.AI;

public class SkeletonAI : MonoBehaviour
{
    public Animator animator;
    public Transform player;
    public float attackDistance = 2f;

    private NavMeshAgent agent;
    private bool alive = true;

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
        }
        else
        {
            agent.isStopped = true;
            animator.SetBool("IsWalking", false);
            animator.SetBool("IsAttacking", true);
        }
    }

    public void Die()
    {
        alive = false;
        agent.isStopped = true;
        animator.SetTrigger("IsDead");
        Destroy(gameObject, 3f);
    }
}
