using UnityEngine;

public class Bullet : MonoBehaviour
{
    public float speed = 10f;
    public int damage = 1;
    public int pierce = 2; // Pierce value set to 2

    private Rigidbody rb;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.velocity = transform.forward * speed;
        rb.freezeRotation = true;
    }

    void OnTriggerEnter(Collider other)
    {
        Debug.Log("Bullet collided with: " + other.gameObject.name); // Log for debugging
        if (other.CompareTag("Enemy"))
        {
            EnemyController enemy = other.GetComponent<EnemyController>();
            if (enemy != null)
            {
                enemy.TakeDamage(damage);
            }
            Destroy(gameObject); // Destroy bullet after hitting an enemy
        }
        else if (other.CompareTag("Dragon"))
        {
            DragonBossController dragon = other.GetComponent<DragonBossController>();
            if (dragon != null)
            {
                dragon.TakeDamage(damage);
            }
            Destroy(gameObject); // Destroy bullet after hitting the dragon
        }
        else if (other.CompareTag("Shield"))
        {
            ShieldScript shield = other.GetComponent<ShieldScript>();
            if (shield != null)
            {
                Debug.Log("Bullet hit shield. Calling TakeDamage."); // Log for debugging
                shield.TakeDamage(damage, pierce);
            }
            Destroy(gameObject); // Destroy bullet after hitting a shield
        }
        else if (other.CompareTag("Wall"))
        {
            Debug.Log("Bullet hit wall."); // Log for debugging
            Destroy(gameObject); // Destroy bullet after hitting a wall
        }
    }
}











