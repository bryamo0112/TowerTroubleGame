using UnityEngine;
using System.Collections;

public class Fireball : MonoBehaviour
{
    public int damage = 10; // Damage dealt to players and shields
    public int pierce = 2; // Shield pierce value
    public float lifetime = 2f; // Lifetime of the fireball before it gets destroyed
    public float speed = 20f; // Speed at which the fireball travels
    public float colliderLength = 5f; // Length of the collider
    public float colliderRadius = 1f; // Radius of the collider

    private Rigidbody rb;

    void Start()
    {
        Destroy(gameObject, lifetime); // Destroy the fireball after a certain time

        BoxCollider collider = gameObject.AddComponent<BoxCollider>();
        collider.isTrigger = true;
        collider.size = new Vector3(colliderRadius, colliderRadius, colliderLength);
        collider.center = new Vector3(0, 0, colliderLength / 2);

        rb = GetComponent<Rigidbody>();
        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody>();
        }
        rb.useGravity = false;
        rb.velocity = transform.forward * speed; // Set the initial velocity
    }

    void Update()
    {
        // Move the fireball forward
        transform.Translate(Vector3.forward * speed * Time.deltaTime);
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerController player = other.GetComponent<PlayerController>();
            Player2Controller player2 = other.GetComponent<Player2Controller>();

            // Check if Player 2 is blocking
            if (player2 != null && player2.IsBlocking())
            {
                // If blocking, prioritize shield damage
                Collider[] hits = Physics.OverlapSphere(transform.position, 0.5f);
                foreach (var hit in hits)
                {
                    if (hit.CompareTag("PlayerShield"))
                    {
                        PlayerShield playerShield = hit.GetComponent<PlayerShield>();
                        if (playerShield != null)
                        {
                            playerShield.TakeDamage(damage, pierce);
                            Debug.Log("PlayerShield hit by fireball.");
                            Destroy(gameObject); // Destroy fireball after hitting shield
                            return;
                        }
                    }
                }
            }

            // Otherwise, deal damage to Player 2
            if (player2 != null)
            {
                player2.TakeDamage();
                Debug.Log("Player 2 took damage from fireball.");
            }
            else if (player != null)
            {
                player.TakeDamage();
                Debug.Log("Player took damage from fireball.");
            }
            Destroy(gameObject); // Destroy fireball after hitting a player
        }
        else if (other.CompareTag("PlayerShield"))
        {
            PlayerShield playerShield = other.GetComponent<PlayerShield>();
            if (playerShield != null)
            {
                playerShield.TakeDamage(damage, pierce);
                Debug.Log("PlayerShield hit by fireball.");
            }
            Destroy(gameObject); // Destroy fireball after hitting a shield
        }
        else if (other.CompareTag("Wall"))
        {
            Wall wall = other.GetComponent<Wall>();
            if (wall != null)
            {
                wall.TakeDamage();
                Debug.Log("Wall hit by fireball.");
            }
            // Destroy the fireball when it hits a wall
            Destroy(gameObject);
        }
    }
}





