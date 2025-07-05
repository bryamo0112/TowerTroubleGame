using System.Collections;
using UnityEngine;

public class EnemyController : MonoBehaviour
{
    public int health = 3; // Enemy health
    public GameObject shieldPrefab; // Reference to the shield prefab
    public GameObject ammoBoxPrefab; // Reference to the ammo box prefab
    public float blockDuration = 4f; // Duration of the block
    public float blockCooldown = 1f; // Cooldown before blocking again
    private Animator animator;
    private bool isBlocking = false;
    private bool isOnCooldown = false;
    private bool isHit = false; // Track if the enemy has been hit
    private bool isInvincible = false; // Track invincibility status
    private Transform shieldTransform; // Reference to the shield transform

    void Start()
    {
        animator = GetComponent<Animator>();
        Rigidbody rb = GetComponent<Rigidbody>();
        rb.constraints = RigidbodyConstraints.FreezeRotation | RigidbodyConstraints.FreezePositionY;
        Debug.Log("Enemy initial Health: " + health); // Ensure health is set correctly
        shieldTransform = transform.Find("Skeleton_Warrior_ArmRight/Skeleton_Shield_Large_A"); // Set the shield transform
        StartCoroutine(BlockRoutine()); // Start the blocking routine
        IgnoreCollisionWithEquippedShield();
    }

    IEnumerator BlockRoutine()
    {
        while (true)
        {
            if (!isBlocking && !isOnCooldown && shieldTransform != null)
            {
                isBlocking = true;
                isOnCooldown = true;
                isInvincible = true; // Enable invincibility
                animator.SetBool("isBlocking", true); // Use SetBool for continuous animation

                // Move shield to new position
                shieldTransform.localPosition = new Vector3(0.15f, 0.8f, 0.45f); // Adjust these values to desired position
                shieldTransform.localRotation = Quaternion.Euler(0, 0, 0); // Adjust these values to desired rotation

                // Wait for the block duration
                yield return new WaitForSeconds(blockDuration);

                isBlocking = false;
                isInvincible = false; // Disable invincibility
                animator.SetBool("isBlocking", false);

                // Reset shield to original position
                shieldTransform.localPosition = new Vector3(1.0f, 0.7f, 0.3f); // Adjust these values to original position
                shieldTransform.localRotation = Quaternion.Euler(0, 0, 0); // Adjust these values to original rotation

                // Wait for the cooldown duration
                yield return new WaitForSeconds(blockCooldown);
                isOnCooldown = false;
            }
            yield return null;
        }
    }

    void Update()
    {
        // Check if shield is destroyed
        if (shieldTransform == null)
        {
            isBlocking = false;
            isInvincible = false;
            animator.SetBool("isBlocking", false);
        }
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
            else
            {
                animator.SetTrigger("Recoil"); // Trigger recoil animation
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
        Debug.Log("Enemy is dead.");
        TryDropShield(); // Attempt to drop the shield upon death
        Destroy(gameObject);
    }

    void TryDropShield()
    {
        // Chance to drop shield
        float shieldDropChance = 1f; // 100% chance for shield
        if (Random.value < shieldDropChance)
        {
            Debug.Log("Shield dropped.");
            Instantiate(shieldPrefab, transform.position, Quaternion.identity);
        }
        else
        {
            Debug.Log("Shield not dropped.");
        }

    }

    void IgnoreCollisionWithEquippedShield()
    {
        Collider enemyCollider = GetComponent<Collider>();
        if (shieldTransform != null)
        {
            Debug.Log("Shield found: " + shieldTransform.name);
            Collider shieldCollider = shieldTransform.GetComponent<Collider>();
            if (enemyCollider != null && shieldCollider != null)
            {
                Physics.IgnoreCollision(enemyCollider, shieldCollider);
            }
        }
        else
        {
            Debug.LogError("Shield not found in the enemy hierarchy.");
        }
    }
}

