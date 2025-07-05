using System.Collections;
using UnityEngine;

public class ZombieCharacterControl : MonoBehaviour
{
    public int health = 3; // Zombie health
    public float detectionRange = 10f; // Range to detect players
    public float attackRange = 2f; // Range to attack players
    public float runSpeed = 2f; // Running speed of the zombie
    private Animator animator;
    private bool isAttacking = false;
    private bool isOnCooldown = false;
    private bool isHit = false; // Track if the zombie has been hit
    private bool isInvincible = false; // Track invincibility status
    private Transform playerTarget; // Reference to the player target
    private Rigidbody rb;

    void Start()
    {
        animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody>();
        
        Debug.Log("Zombie initial Health: " + health); // Ensure health is set correctly

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
        while (playerTarget != null)
        {
            float distanceToPlayer = Vector3.Distance(transform.position, playerTarget.position);

            // If the player is out of detection range, break the loop to retarget
            if (distanceToPlayer > detectionRange)
            {
                playerTarget = null;
                animator.SetBool("isRunning", false);
                animator.SetTrigger("StopRunning");
                yield break;
            }

            if (!isOnCooldown)
            {
                if (distanceToPlayer <= attackRange)
                {
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
                    transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, 0.15f); // Smooth rotation
                }
                yield return new WaitForSeconds(1f); // Adjust cooldown as necessary
                isOnCooldown = false;
            }
            yield return null;
        }
    }

    void Attack()
    {
        if (!isAttacking)
        {
            isAttacking = true;
            isOnCooldown = true;
            animator.SetTrigger("Attack");
            DealDamage(); // Attempt to deal damage during attack
            StartCoroutine(ResetAttack());
        }
    }

    void DealDamage()
    {
        if (playerTarget != null)
        {
            PlayerController player = playerTarget.GetComponent<PlayerController>();
            Player2Controller player2 = playerTarget.GetComponent<Player2Controller>();
            Player3Controller player3 = playerTarget.GetComponent<Player3Controller>();

            if (player != null)
            {
                Debug.Log("Zombie attacking Player");
                player.TakeDamage();
            }
            else if (player2 != null)
            {
                Debug.Log("Zombie attacking Player2");
                player2.TakeDamage();
            }
            else if (player3 != null)
            {
                Debug.Log("Zombie attacking Player3");
                player3.TakeDamage();
            }
        }
    }

    IEnumerator ResetAttack()
    {
        yield return new WaitForSeconds(0.5f); // Adjust duration of attack animation
        isAttacking = false;
        animator.SetTrigger("StopAttack"); // Transition back to idle or running
    }

    public void TakeDamage(int damage)
    {
        if (!isHit && !isInvincible)
        {
            health -= damage;
            Debug.Log("Zombie Health: " + health);
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
        Debug.Log("Zombie collided with: " + collision.gameObject.name); // Log for debugging
        if (collision.gameObject.CompareTag("Player"))
        {
            PlayerController player = collision.gameObject.GetComponent<PlayerController>();
            Player2Controller player2 = collision.gameObject.GetComponent<Player2Controller>();
            Player3Controller player3 = collision.gameObject.GetComponent<Player3Controller>();

            if (player != null)
            {
                player.TakeDamage();
                Debug.Log("Player hit by Zombie!");
            }
            else if (player2 != null)
            {
                player2.TakeDamage();
                Debug.Log("Player2 hit by Zombie!");
            }
            else if (player3 != null)
            {
                player3.TakeDamage();
                Debug.Log("Player3 hit by Zombie!");
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
                Debug.Log("Zombie hit by bayonet!");
            }
        }
        else if (other.CompareTag("Fire"))
        {
            FireEffect fire = other.GetComponent<FireEffect>();
            if (fire != null)
            {
                TakeDamage(fire.damage);
                Debug.Log("Zombie hit by fire!");
            }
        }
        else if (other.CompareTag("Projectile"))
        {
            ProjectileController projectile = other.GetComponent<ProjectileController>();
            if (projectile != null)
            {
                TakeDamage(projectile.damage);
                Debug.Log("Zombie hit by projectile!");
            }
        }
    }

    void Die()
    {
        StopAllCoroutines();
        animator.SetTrigger("Dead");
        Debug.Log("Zombie is dead.");
        Destroy(gameObject, 1f); // Delay destruction to allow animation to play
    }
}









