using UnityEngine;

public class Fuel : MonoBehaviour
{
    public int fuelAmount = 3; // Amount of fuel this object gives

    void OnTriggerEnter(Collider other)
    {
        // Check if the player collides with the fuel
        if (other.CompareTag("Player"))
        {
            // Try to get the Player2Controller script from the collided object
            Player2Controller player2 = other.GetComponent<Player2Controller>();

            if (player2 != null)
            {
                // Add fuel to the player's inventory
                player2.ReplenishFuel(fuelAmount);

                // Print debug message
                Debug.Log("Player2 picked up " + fuelAmount + " fuel!");

                // Destroy the fuel object after pickup
                Destroy(gameObject);
            }
        }
    }
}

