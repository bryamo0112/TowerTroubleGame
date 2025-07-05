using System.Collections;
using UnityEngine;

public class ComplexEnemyController : MonoBehaviour
{
    public int health = 3; // Enemy health
    public float actionCooldown = 1f; // Cooldown between actions
    public float detectionRange = 10f; // Range to detect players
    public float attackRange = 2f; // Range to attack players
    public float runSpeed = 5f; // Running speed of the enemy
    private Animator animator;
    private bool isAttacking = false;
    private bool isOnCooldown = false;
    private bool isHit = false; // Track if the enemy has been hit
    private bool isInvincible = false; // Track invincibility status
    private Transform playerTarget; // Reference to the player target
    private Rigidbody rb;

    void Start()
    {
        animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody>();
        rb.constraints = RigidbodyConstraints.FreezeRotation | RigidbodyConstraints.FreezePositionY;
        Debug.Log("Enemy initial Health: " + health); // Ensure health is set correctly

        StartCoroutine(DetectPlayer()); // Start detecting players
    }

    IEnumerator DetectPlayer()
    {
        while (true)
        {
            Collider[] hitColliders = Physics.OverlapSphere(transform.position, detectionRange);
            foreach (Collider hitCollider in hitColliders)
            {
                if (hitCollider.CompareTag("Player") && HasLineOfSight(hitCollider.transform))
                {
                    playerTarget = hitCollider.transform;
                    animator.SetTrigger("StartRunning");
                    animator.SetBool("isRunning", true);
                    StartCoroutine(EngagePlayer()); // Engage the player
                    break;
                }
            }
            yield return new WaitForSeconds(0.5f); // Check for players every half second
        }
    }

    bool HasLineOfSight(Transform target)
    {
        RaycastHit hit;
        Vector3 directionToTarget = (target.position - transform.position).normalized;
        if (Physics.Raycast(transform.position, directionToTarget, out hit, detectionRange))
        {
            if (hit.collider.CompareTag("Player"))
            {
                return true; // Player is visible
            }
        }
        return false; // Player is not visible
    }

    IEnumerator EngagePlayer()
    {
        while (playerTarget != null && Vector3.Distance(transform.position, playerTarget.position) < detectionRange)
        {
            if (!isOnCooldown)
            {
                if (Vector3.Distance(transform.position, playerTarget.position) <= attackRange)
                {
                    animator.SetBool("isRunning", false);
                    Attack();
                }
                else
                {
                    Vector3 direction = (playerTarget.position - transform.position).normalized;
                    Vector3 movement = direction * runSpeed * Time.deltaTime;
                    rb.MovePosition(transform.position + movement); // Smooth movement
                    animator.SetBool("isRunning", true); // Ensure running animation continues
                    // Rotate towards the player
                    Quaternion targetRotation = Quaternion.LookRotation(direction);
                    transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, 0.1f); // Smooth rotation
                }
                yield return new WaitForSeconds(actionCooldown); // Apply cooldown
                isOnCooldown = false;
            }
            yield return null;
        }
        animator.SetBool("isRunning", false);
        animator.SetTrigger("StopRunning"); // Ensure transition back to Idle
    }

    void Attack()
    {
        if (!isAttacking)
        {
            isAttacking = true;
            isOnCooldown = true;
            animator.SetTrigger("Attack");
            StartCoroutine(ResetAttack());
        }
    }

    IEnumerator ResetAttack()
    {
        yield return new WaitForSeconds(0.5f); // Reduced duration of attack animation
        isAttacking = false;
        animator.SetTrigger("StopAttack"); // Transition back to idle or running
    }

    public void TakeDamage(int damage)
    {
        if (!isHit && !isInvincible)
        {
            health -= damage;
            Debug.Log("Enemy Health: " + health);
            if (health <= 0)
            {
                Die();
            }
            StartCoroutine(HitCooldown());
        }
    }

    IEnumerator HitCooldown()
    {
        isHit = true;
        yield return new WaitForSeconds(0.5f); // Adjust cooldown as necessary
        isHit = false;
    }

    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            PlayerController player = collision.gameObject.GetComponent<PlayerController>();
            Player2Controller player2 = collision.gameObject.GetComponent<Player2Controller>();

            if (player != null)
            {
                Debug.Log("Enemy collided with Player");
                player.TakeDamage();
            }
            else if (player2 != null)
            {
                Debug.Log("Enemy collided with Player2");
                player2.TakeDamage();
            }
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Bullet"))
        {
            Bullet bullet = other.GetComponent<Bullet>();
            if (bullet != null)
            {
                TakeDamage(bullet.damage);
                Destroy(other.gameObject);
            }
        }
        else if (other.CompareTag("Bayonet"))
        {
            BayonetCollider bayonet = other.GetComponent<BayonetCollider>();
            if (bayonet != null)
            {
                TakeDamage(bayonet.damage);
                Debug.Log("Enemy hit by bayonet!");
            }
        }
        else if (other.CompareTag("Fire"))
        {
            FireEffect fire = other.GetComponent<FireEffect>();
            if (fire != null)
            {
                TakeDamage(fire.damage);
                Debug.Log("Enemy hit by fire!");
            }
        }
        else if (other.CompareTag("Projectile"))
        {
            ProjectileController projectile = other.GetComponent<ProjectileController>();
            if (projectile != null)
            {
                TakeDamage(projectile.damage);
                Debug.Log("Enemy hit by projectile!");
            }
        }
    }

    void Die()
    {
        StopAllCoroutines();
        animator.SetTrigger("Die");
        Debug.Log("Enemy is dead.");
        Destroy(gameObject, 2f); // Delay destruction to allow animation to play
    }
}

























