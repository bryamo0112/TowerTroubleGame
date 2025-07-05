using UnityEngine;

public class PlayerShield : MonoBehaviour
{
    public int maxHealth = 5;
    public int currentHealth;
    public int durability = 1; // Durability set to 1

    void Start()
    {
        currentHealth = maxHealth;
    }

    public void TakeDamage(int damage, int pierce)
    {
        Debug.Log("Damage: " + damage + ", Pierce: " + pierce + ", Durability: " + durability); // Log for debugging
        if (pierce > durability)
        {
            currentHealth -= damage;
            Debug.Log("Shield took damage. Current health: " + currentHealth);
            if (currentHealth <= 0)
            {
                DestroyShield();
            }
        }
        else
        {
            Debug.Log("Shield blocked the attack.");
        }
    }

    void DestroyShield()
    {
        Debug.Log("Shield is destroyed.");
        Destroy(gameObject);
    }

    void OnTriggerEnter(Collider other)
    {
        Debug.Log("Shield collided with: " + other.gameObject.name);
        if (other.CompareTag("Bullet") || other.CompareTag("Bayonet"))
        {
            Bullet bullet = other.GetComponent<Bullet>();
            BayonetCollider bayonet = other.GetComponent<BayonetCollider>();
            if (bullet != null)
            {
                TakeDamage(bullet.damage, bullet.pierce);
            }
            else if (bayonet != null)
            {
                TakeDamage(bayonet.damage, bayonet.pierce);
            }
        }
        else if (other.CompareTag("Sword"))
        {
            SwordCollider sword = other.GetComponent<SwordCollider>();
            if (sword != null)
            {
                TakeDamage(sword.damage, sword.pierce);
                Debug.Log("Shield hit by sword!");
            }
        }
        else if (other.CompareTag("Projectile")) // Add Projectile handling
        {
            ProjectileController projectile = other.GetComponent<ProjectileController>();
            if (projectile != null)
            {
                TakeDamage(projectile.damage, projectile.pierce);
                Debug.Log("Shield hit by projectile!");
            }
        }
        else if (other.CompareTag("Fireball"))
        {
            Fireball fireball = other.GetComponent<Fireball>();
            if (fireball != null)
            {
                TakeDamage(fireball.damage, fireball.pierce);
                Debug.Log("Shield hit by Fireball!");
            }
        }
    }
}

