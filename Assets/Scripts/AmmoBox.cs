using UnityEngine;

public class AmmoBox : MonoBehaviour
{
    public int ammoAmount = 10; // Amount of ammo this box gives

    void OnTriggerEnter(Collider other)
    {
        // Check if the player collides with the ammo box
        if (other.CompareTag("Player"))
        {
            // Try to get the PlayerController script from the collided object
            PlayerController player = other.GetComponent<PlayerController>();

            if (player != null)
            {
                // Add ammo to the player's inventory
                player.GainAmmo(ammoAmount);

                // Print debug message
                Debug.Log("Player picked up " + ammoAmount + " ammo!");

                // Destroy the ammo box after pickup
                Destroy(gameObject);
            }
        }
    }
}
