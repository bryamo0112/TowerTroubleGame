using UnityEngine;

public class BayonetCollider : MonoBehaviour
{
    public int damage = 1; // Amount of damage dealt
    public int pierce = 1; // Pierce value set to 1

    void OnTriggerEnter(Collider other)
    {
        Debug.Log("Bayonet collided with: " + other.gameObject.name); // Log for debugging
        if (other.CompareTag("Enemy"))
        {
            // Check for EnemyController
            var enemy = other.GetComponent<EnemyController>();
            if (enemy != null)
            {
                enemy.TakeDamage(damage);
                Debug.Log("Enemy hit by bayonet!");
            }
            else
            {
                // Check for SimpleEnemyController
                var simpleEnemy = other.GetComponent<SimpleEnemyController>();
                if (simpleEnemy != null)
                {
                    simpleEnemy.TakeDamage(damage);
                    Debug.Log("SimpleEnemy hit by bayonet!");
                }
                else
                {
                    // Check for ComplexEnemyController
                    var complexEnemy = other.GetComponent<ComplexEnemyController>();
                    if (complexEnemy != null)
                    {
                        complexEnemy.TakeDamage(damage);
                        Debug.Log("ComplexEnemy hit by bayonet!");
                    }
                    else
                    {
                        // Check for ZombieCharacterControl
                        var zombie = other.GetComponent<ZombieCharacterControl>();
                        if (zombie != null)
                        {
                            zombie.TakeDamage(damage);
                            Debug.Log("Zombie hit by bayonet!");
                        }
                    }
                }
            }
        }
        else if (other.CompareTag("Shield"))
        {
            ShieldScript shield = other.GetComponent<ShieldScript>();
            if (shield != null)
            {
                Debug.Log("Bayonet hit shield. Calling TakeDamage."); // Log for debugging
                shield.TakeDamage(damage, pierce);
                Debug.Log("Shield hit by bayonet!");
            }
        }
    }
}








