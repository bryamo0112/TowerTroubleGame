using UnityEngine;

public class SwordCollider : MonoBehaviour
{
    public int damage = 1; // Amount of damage dealt
    public int pierce = 2; // Pierce value set to 2, used only for shields

    void OnTriggerEnter(Collider other)
    {
        Debug.Log("Sword collided with: " + other.gameObject.name); // Log for debugging
        if (other.CompareTag("Player"))
        {
            PlayerController player = other.GetComponent<PlayerController>();
            if (player != null)
            {
                player.TakeDamage(); // Call the existing TakeDamage method
                Debug.Log("Player hit by sword!");
            }
            else
            {
                Player2Controller player2 = other.GetComponent<Player2Controller>();
                if (player2 != null)
                {
                    player2.TakeDamage(); // Call the existing TakeDamage method
                    Debug.Log("Player2 hit by sword!");
                }
                else
                {
                    Player3Controller player3 = other.GetComponent<Player3Controller>();
                    if (player3 != null)
                    {
                        player3.TakeDamage(); // Call the existing TakeDamage method
                        Debug.Log("Player3 hit by sword!");
                    }
                }
            }
        }
        else if (other.CompareTag("PlayerShield"))
        {
            PlayerShield playerShield = other.GetComponent<PlayerShield>();
            if (playerShield != null)
            {
                Debug.Log("Sword hit shield. Calling TakeDamage."); // Log for debugging
                playerShield.TakeDamage(damage, pierce); // Apply damage and pierce to shield
                Debug.Log("Shield hit by sword!");
            }
        }
    }
}





