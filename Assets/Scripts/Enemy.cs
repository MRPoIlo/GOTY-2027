using UnityEngine;

public class Enemy : MonoBehaviour
{
    UnityEngine.AI.NavMeshAgent agent;
    public Transform player;

    public float currentDistance;
    public float distanceToFollow = 10f;
    public float distanceToFight = 2f;

    public float damagePercent = 0.2f;

    private float attackCooldown = 1.5f;
    private float lastAttackTime;

    Animator animator;

    void Start()
    {
        agent = GetComponent<UnityEngine.AI.NavMeshAgent>();
        animator = GetComponent<Animator>();
    }

    void Update()
    {
        currentDistance = Vector3.Distance(transform.position, player.position);

        if (currentDistance <= distanceToFight)
        {
            animator.SetInteger("Status", 2);
            agent.SetDestination(transform.position);

            if (Time.time > lastAttackTime + attackCooldown)
            {
                AttackPlayer();
                lastAttackTime = Time.time;
            }
        }
        else if (currentDistance <= distanceToFollow)
        {
            agent.SetDestination(player.position);
            animator.SetInteger("Status", 1);
        }
        else
        {
            agent.SetDestination(transform.position);
            animator.SetInteger("Status", 0);
        }
    }

    void AttackPlayer()
    {
        PlayerScript playerScript = player.GetComponent<PlayerScript>();

        if (playerScript != null)
        {
            playerScript.TakeDamage(damagePercent);
        }
    }
}