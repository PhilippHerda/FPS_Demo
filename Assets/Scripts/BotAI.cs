using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class BotAI : MonoBehaviour
{
    public NavMeshAgent agent;

    public Transform player;
    public Transform gunpoint;

    public LayerMask isGround, isPlayer;

    public float health;
    Animator animator;

    // Patrolling
    public Vector3 walkPoint;
    bool walkPointSet;
    public float walkPointRange;

    // Attacking
    public float timeBetweenAttacks;
    bool alreadyAttacked;
    public GameObject projectile;

    // States
    public float sightRange, attackRange;
    public bool playerInSightRange, playerInAttackRange;
    bool isHitLocked;

    private void Awake()
    {
        player = GameObject.Find("Player").transform;
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();
        isHitLocked = false;
    }

    // Update is called once per frame
    void Update()
    {
        playerInSightRange = Physics.CheckSphere(transform.position, sightRange, isPlayer);
        playerInAttackRange = Physics.CheckSphere(transform.position, attackRange, isPlayer);

        if (!isHitLocked)
        {
            if (!playerInSightRange && !playerInAttackRange) Patrolling();
            if (playerInSightRange && !playerInAttackRange) ChasePlayer();
            if (playerInSightRange && playerInAttackRange) AttackPlayer();
        }
    }

    private void Patrolling()
    {
        animator.SetBool("isIdle", false);

        if (!walkPointSet) SearchWalkPoint();

        if (walkPointSet)
            agent.SetDestination(walkPoint);

        animator.SetBool("isWalking", true);
        
        Vector3 distanceToWalkPoint = transform.position - walkPoint;

        if ( distanceToWalkPoint.magnitude < 1f)
            walkPointSet = false;
    }

    private void SearchWalkPoint()
    {
        float randomZ = Random.Range(-walkPointRange, walkPointRange);
        float randomX = Random.Range(-walkPointRange, walkPointRange);

        walkPoint = new Vector3(transform.position.x + randomX, transform.position.y, transform.position.z + randomZ);

        if (Physics.Raycast(walkPoint, -transform.up, 2f, isGround))
            walkPointSet = true;
    }

    private void ChasePlayer()
    {
        animator.SetBool("isIdle", false);
        animator.SetBool("isShooting", false);

        agent.SetDestination(player.position);
        // TODO: perhaps add running for chasing later on
        animator.SetBool("isWalking", true);
    }

    private void AttackPlayer()
    {
        animator.SetBool("isIdle", false);
        animator.SetBool("isWalking", false);

        agent.SetDestination(transform.position);

        transform.LookAt(new Vector3(player.position.x, player.position.y - 1f, player.position.z));

        if (!alreadyAttacked)
        {
            animator.SetBool("isShooting", true);
            // attack code
            Rigidbody rb = Instantiate(projectile, gunpoint.position, Quaternion.identity).GetComponent<Rigidbody>();

            rb.AddForce(transform.forward * 32f, ForceMode.Impulse);

            alreadyAttacked = true;
            Invoke(nameof(ResetAttack), timeBetweenAttacks);
        }

        if (!alreadyAttacked)
        {
            animator.SetBool("isIdle", true);
            // Invoke(nameof(ResetIdle), timeBetweenAttacks);
        }
    }

    private void ResetAttack()
    {
        animator.SetBool("isShooting", false);
        alreadyAttacked = false;
    }

    public void TakeDamage(int damage)
    {
        health -= damage;

        if (health > 0)
        {
            isHitLocked = true;
            animator.SetBool("isGettingHit", true);
            transform.LookAt(new Vector3(player.position.x, player.position.y - 1f, player.position.z));
            Invoke(nameof(ResetHitLock), 4.6f);
        }
        
        if (health <= 0) Invoke(nameof(DestroyEnemy), 0.5f);
    }

    private void ResetHitLock()
    {
        //animator.SetBool("isGettingHit", false);
        isHitLocked = false;
    }

    private void  DestroyEnemy()
    {
        Destroy(gameObject);
    }

    private void OnDrawGizmoSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, sightRange);
    }
}
