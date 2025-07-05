using System.Collections;
using UnityEngine;

public class SimpleEnemyController : MonoBehaviour
{
    public int health = 3; // Enemy health
    public GameObject shieldPrefab; // Reference to the shield prefab
    public float stabDuration = 2f; // Duration between each stab
    private Animator animator;
    private bool isStabbing = false;
    private bool isOnCooldown = false; // Track if cooldown is active
    private bool isHit = false; // Track if the enemy has been hit
    private bool isInvincible = false; // Track invincibility status
    private Rigidbody rb; // Reference to Rigidbody
    private Quaternion initialRotation; // Store initial rotation

    void Start()
    {
        animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody>();
        rb.constraints = RigidbodyConstraints.FreezeRotation | RigidbodyConstraints.FreezePositionY;
        initialRotation = transform.rotation; // Save initial rotation
        Debug.Log("Enemy initial Health: " + health); // Ensure health is set correctly
        StartCoroutine(StabRoutine()); // Start the stabbing routine
    }

    IEnumerator StabRoutine()
    {
        while (true)
        {
            if (!isStabbing && !isOnCooldown)
            {
                isStabbing = true;
                isOnCooldown = true;
                animator.SetTrigger("Stab"); // Trigger stab animation
                animator.SetBool("isStabbing", true); // Set isStabbing to true

                // Stab duration
                yield return new WaitForSeconds(1f);

                isStabbing = false;
                animator.SetBool("isStabbing", false);

                // Reset rotation after stab
                transform.rotation = initialRotation; // Reset to initial rotation

                // Cooldown before next stab
                yield return new WaitForSeconds(stabDuration);
                isOnCooldown = false;
            }
            yield return null;
        }
    }

    void Update()
    {
        // Other enemy behavior like movement or attacks
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
                // Optionally destroy the fire effect upon hitting the enemy
                // Destroy(other.gameObject);
            }
        }
        else if (other.CompareTag("Projectile")) // Add this block for handling projectiles
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
        Debug.Log("Enemy is dead.");
        TryDropShield(); // Attempt to drop the shield upon death
        Destroy(gameObject);
    }

    void TryDropShield()
    {
        float dropChance = 0.0f; // 50% chance
        if (Random.value < dropChance)
        {
            Debug.Log("Shield dropped.");
            Instantiate(shieldPrefab, transform.position, Quaternion.identity);
        }
        else
        {
            Debug.Log("Shield not dropped.");
        }
    }
}







