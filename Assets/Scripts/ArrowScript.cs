using UnityEngine;

public class Arrow : MonoBehaviour
{
    public float speed = 10f;
    public int damage = 1;
    public int pierce = 0;

    private Rigidbody rb;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.velocity = transform.forward * speed;
        rb.freezeRotation = true;
        Destroy(gameObject, 5f); // Destroy the arrow after 5 seconds to avoid clutter
    }

    void OnTriggerEnter(Collider other)
    {
        Debug.Log("Arrow collided with: " + other.gameObject.name);
        if (other.CompareTag("Enemy"))
        {
            EnemyController enemy = other.GetComponent<EnemyController>();
            if (enemy != null)
            {
                enemy.TakeDamage(damage); // You can implement logic for pierce if needed
            }
        }
        Destroy(gameObject); // Destroy arrow after hitting an enemy or another object
    }
}

