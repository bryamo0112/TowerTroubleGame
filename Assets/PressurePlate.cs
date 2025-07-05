using UnityEngine;

public class PressurePlate : MonoBehaviour
{
    public GameObject obstacle; // The obstacle to hide/show
    public bool isActivated = false; // To track pressure plate state

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player")) // Ensure the player has the "Player" tag
        {
            isActivated = true;
            HideObstacle();
            Debug.Log("Player stepped on the pressure plate. Obstacle hidden!");
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player")) // Ensure the player has the "Player" tag
        {
            isActivated = false;
            ShowObstacle();
            Debug.Log("Player left the pressure plate. Obstacle shown!");
        }
    }

    private void HideObstacle()
    {
        if (obstacle != null)
        {
            obstacle.SetActive(false); // Disable the obstacle
        }
    }

    private void ShowObstacle()
    {
        if (obstacle != null)
        {
            obstacle.SetActive(true); // Enable the obstacle
        }
    }
}
