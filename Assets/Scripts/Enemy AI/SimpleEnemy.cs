using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class SimpleEnemy : MonoBehaviour
{
    Animator animator;

    public NavMeshAgent agent;

    public Transform player;

    public LayerMask whatIsObstacle, whatIsPlayer;

    // Patrolling
    public Vector3 walkPoint;
    bool walkPointSet;
    DungeonCreatorScript dungeon;
    List<Vector3> walkPoints;

    // Attacking
    public int attackDamage;
    public bool isAttacking, isAttackAnimationFinished;

    // States
    public float sightRange, attackRange;
    [Range(0, 360)]
    public float angle;
    public bool playerInSightRange, playerInAttackRange;


    [Header("Audio")]
    private bool patrolGroanPlayed = false;
    private float patrolGroanCooldown = 10f;
    private float patrolGroanTimer = 9.9f;
    [SerializeField] private AudioSource patrolGroan;

    private bool chaseGroanPlayed = false;
    private float chaseGroanCooldown = 3f;
    private float chaseGroanTimer = 2.9f;
    [SerializeField] private AudioSource chaseGroan;

    private bool attackGroanPlayed = false;
    private float attackGroanCooldown = 1f;
    private float attackGroanTimer = 0.9f;
    [SerializeField] private AudioSource attackGroan;

    private bool chaseSoundPlayed = false;
    private float chaseSoundCooldown = 11f;
    private float chaseSoundTimer = 10.9f;
    [SerializeField] private AudioSource chaseSound;


    private void Awake()
    {
        player = GameObject.FindGameObjectWithTag("Player").transform;
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();
        dungeon = GameObject.FindGameObjectWithTag("GameManager").GetComponent<DungeonCreatorScript>();
        walkPoints = dungeon.midPoints;
    }

    private void Update()
    {
        FieldOfViewCheck();

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
                // Check if attack animation is finished
                if (isAttackAnimationFinished)
                    StopAttacking();
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
        animator.SetBool("isRunning", false);
        animator.SetBool("isAttacking", false);

        if (!patrolGroanPlayed && patrolGroanTimer >= patrolGroanCooldown)
        {
            patrolGroan.Play();
            patrolGroanPlayed = true;
            patrolGroanTimer = 0f;
        }
        else if (!patrolGroanPlayed)
        {
            patrolGroanTimer += Time.deltaTime;
        }

        if (!walkPointSet) SearchWalkPoint();

        if (walkPointSet)
        {
            NavMeshPath path = new NavMeshPath();
            if (agent.CalculatePath(walkPoint, path))
            {
                if (path.status == NavMeshPathStatus.PathComplete)
                {
                    agent.SetDestination(walkPoint);
                }
                else
                {
                    // Path is not complete, reset walkPointSet to false
                    walkPointSet = false;
                }
            }
            else
            {
                walkPointSet = false;
            }
        }

        animator.SetBool("isWalking", true);

        Vector3 distanceToWalkPoint = transform.position - walkPoint;

        // Repeat upon reaching walkpoint
        if (distanceToWalkPoint.magnitude < 1f)
            walkPointSet = false;
    }

    private void SearchWalkPoint()
    {
        List<Vector3> validPoints = new List<Vector3>();

        foreach (Vector3 point in walkPoints)
        {
            NavMeshPath path = new NavMeshPath();
            if (agent.CalculatePath(point, path))
            {
                if (path.status == NavMeshPathStatus.PathComplete)
                {
                    // Found a reachable point, exit the loop
                    validPoints.Add(point);
                }
            }
        }

        if (validPoints.Count > 0)
        {
            int randomIndex = Random.Range(0, validPoints.Count);
            walkPoint = validPoints[randomIndex];
            walkPointSet = true;
        }
    }

    private void Chasing()
    {
        patrolGroan.Stop();
        if (!chaseGroanPlayed && chaseGroanTimer >= chaseGroanCooldown)
        {
            chaseGroan.Play();
            chaseGroanPlayed = true;
            chaseGroanTimer = 0f;
        }
        else if (!chaseGroanPlayed)
        {
            chaseGroanTimer += Time.deltaTime;
        }

        if (!chaseSoundPlayed && chaseSoundTimer >= chaseSoundCooldown)
        {
            chaseSound.Play();
            chaseSoundPlayed = true;
            chaseSoundTimer = 0f;
        }
        else if (!chaseSoundPlayed)
        {
            chaseSoundTimer += Time.deltaTime;
        }

        animator.SetBool("isWalking", false);
        animator.SetBool("isAttacking", false);

        walkPointSet = false;
        agent.SetDestination(player.position); 
        animator.SetBool("isRunning", true);
    }


    private void FieldOfViewCheck()
    {
        Collider[] attackRangeChecks = Physics.OverlapSphere(transform.position, attackRange, whatIsPlayer);

        if (attackRangeChecks.Length != 0)
        {
            Transform target = attackRangeChecks[0].transform;
            Vector3 directionToTarget = (target.position - transform.position).normalized;

            if (Vector3.Angle(transform.forward, directionToTarget) < angle / 2)
            {
                float distanceToTarget = Vector3.Distance(transform.position, target.position);

                if (!Physics.Raycast(transform.position, directionToTarget, distanceToTarget, whatIsObstacle))
                    playerInAttackRange = true;
                else
                    playerInAttackRange = false;
            }
            else
                playerInAttackRange = false;
        }
        else if (playerInAttackRange)
            playerInAttackRange = false;


        Collider[] rangeChecks = Physics.OverlapSphere(transform.position, sightRange, whatIsPlayer);

        if (rangeChecks.Length != 0)
        {
            Transform target = rangeChecks[0].transform;
            Vector3 directionToTarget = (target.position - transform.position).normalized;

            if (Vector3.Angle(transform.forward, directionToTarget) < angle / 2)
            {
                float distanceToTarget = Vector3.Distance(transform.position, target.position);

                if (!Physics.Raycast(transform.position, directionToTarget, distanceToTarget, whatIsObstacle))
                    playerInSightRange = true;
                else
                    playerInSightRange = false;
            }
            else
                playerInSightRange = false;
        }
        else if (playerInSightRange)
            playerInSightRange = false;
    }

    private void StartAttacking()
    {
        walkPointSet = false;

        animator.SetBool("isWalking", false);
        animator.SetBool("isRunning", false);

        chaseGroan.Stop();
        patrolGroan.Stop();

        if (!attackGroanPlayed && attackGroanTimer >= attackGroanCooldown)
        {
            attackGroan.Play();
            attackGroanPlayed = true;
            attackGroanTimer = 0f;
        }
        else if (!attackGroanPlayed)
        {
            attackGroanTimer += Time.deltaTime;
        }

        agent.isStopped = true;
        isAttacking = true;
        animator.SetBool("isAttacking", true);
        transform.LookAt(player);
    }

    private void StopAttacking()
    {
        walkPointSet = false;

        animator.SetBool("isAttacking", false);
        isAttackAnimationFinished = false;
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
