using UnityEngine;
using UnityEngine.AI;

public class SimpleEnemy : MonoBehaviour
{
    public NavMeshAgent agent;

    public Transform player;

    public LayerMask whatIsGround, whatIsPlayer;

    // Patrolling
    public Vector3 walkPoint;
    bool walkPointSet;
    public float walkPointRange;

    // Attacking
    public float timeBetweenAttacks, attackDuration, attackTimer;
    public int attackDamage;
    bool isAttacking;

    // States
    public float sightRange, attackRange;
    public bool playerInSightRange, playerInAttackRange;

    private void Awake()
    {
        player = GameObject.Find("Player").transform;
        agent = GetComponent<NavMeshAgent>();
    }

    private void Update()
    {
        playerInSightRange = Physics.CheckSphere(transform.position, sightRange, whatIsPlayer);
        playerInAttackRange = Physics.CheckSphere(transform.position, attackRange, whatIsPlayer);

        if (!playerInSightRange && !playerInAttackRange)
        {
            Patrolling();
        }

        // Player is in sight range
        if (playerInSightRange)
        {
            // Check if enemy is currently attacking
            if (isAttacking)
            {
                // Check if attack duration is elapsed
                attackTimer += Time.deltaTime;
                if (attackTimer >= attackDuration)
                {
                    StopAttacking();
                }
            }
            // If enemy is not attacking
            else
            {
                // If player is in attack range
                if (playerInAttackRange)
                {
                    // Start attacking the player
                    StartAttacking();
                }
                // If the player is not in attack range
                else
                {
                    Chasing();
                }
            }
        }
    }

    private void Patrolling()
    {
        if (!walkPointSet) SearchWalkPoint();

        if (walkPointSet)
            agent.SetDestination(walkPoint);

        Vector3 distanceToWalkPoint = transform.position - walkPoint;

        // Repeat upon reaching walkpoint
        if (distanceToWalkPoint.magnitude < 1f)
            walkPointSet = false;
    }

    private void SearchWalkPoint()
    {
        // Find new random point in range
        float randomX = Random.Range(-walkPointRange, walkPointRange);
        float randomZ = Random.Range(-walkPointRange, walkPointRange);

        walkPoint = new Vector3(transform.position.x + randomX, transform.position.y, transform.position.z + randomZ);

        if (Physics.Raycast(walkPoint, -transform.up, 2f, whatIsGround))
            walkPointSet = true;
    }

    private void Chasing()
    {
        walkPointSet = false;
        agent.SetDestination(player.position);
    }

    private void StartAttacking()
    {
        agent.isStopped = true;
        isAttacking = true;
        attackTimer = 0f;
        transform.LookAt(player);
    }

    private void StopAttacking()
    {
        Attack();
        agent.isStopped = false;
        isAttacking = false;
    }

    private void Attack()
    {
        HealthScript playerHealth = player.GetComponent<HealthScript>();
        if (playerHealth != null)
        {
            if (playerInAttackRange)
            {
                // Inflict damage
                playerHealth.TakeDamage(attackDamage);
            }
        }
    }
}
