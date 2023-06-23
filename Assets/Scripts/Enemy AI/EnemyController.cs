using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyController : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] float patrolSpeed = 3f;
    [SerializeField] float chaseSpeed = 5f;

    [Header("Ranges")]
    [SerializeField] float chaseRange = 10f;

    [Header("Attack")]
    [SerializeField] float attackRange = 1f;
    [SerializeField] int attackDamage = 10;
    [SerializeField] float attackDuration = 2f;
    [SerializeField] float attackTimer;


    [Header("Player")]
    [SerializeField] Transform playerTransform;

    [Header("Booleans")]
    private bool isChasing;
    private bool isAttacking;

    private void Start()
    {
        playerTransform = GameObject.FindGameObjectWithTag("Player").transform;
    }

    private void Update()
    {
        float distanceToPlayer = Vector3.Distance(transform.position, playerTransform.position);

        if (distanceToPlayer <= attackRange && !isAttacking)
        {
            if (!isChasing)
                StopPatrolling();

            isChasing = false;
            isAttacking = true;
            attackTimer = attackDuration;
        }
        else if (distanceToPlayer <= attackRange && isAttacking)
        {
            attackTimer -= Time.deltaTime;

            if (attackTimer <= 0f)
            {
                Attack();
                attackTimer = attackDuration;
            }
        }
        else if (distanceToPlayer <= chaseRange && !isChasing && !isAttacking)
        {
            StartChasing();
        }
        else if (distanceToPlayer > chaseRange && isChasing)
        {
            StopChasing();
        }

        if (isChasing && !isAttacking)
        {
            Chase();
        }
        else if (!isChasing && !isAttacking)
        {
            Patrol();
        }
    }

    private void StartChasing()
    {

    }

    private void StopChasing()
    {

    }

    private void Chase()
    {

    }

    private void Patrol()
    {

    }

    private void StopPatrolling()
    {

    }

    private void Attack()
    {

    }

    //void Update()
    //{
    //    float distanceToPlayer = Vector3.Distance(transform.position, playerTransform.position);

    //    if (distanceToPlayer <= detectionRange)
    //    {
    //        // Move towards the player
    //        transform.LookAt(playerTransform);
    //        transform.Translate(Vector3.forward * Time.deltaTime);

    //        if (distanceToPlayer <= attackRange)
    //        {
    //            // Initiate an attack on the player
    //            AttackPlayer();
    //        }
    //    }
    //}

    //void AttackPlayer()
    //{
    //    HealthScript playerHealth = playerTransform.GetComponent<HealthScript>();
    //    if (playerHealth != null)
    //    {
    //        // Inflict damage
    //        playerHealth.TakeDamage(attackDamage);
    //    }
    //}
}
