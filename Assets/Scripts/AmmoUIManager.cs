using UnityEngine;
using UnityEngine.UI; // Make sure to include this to interact with UI elements

public class AmmoUIManager : MonoBehaviour
{
    public Text ammoText; // Reference to the UI Text component
    public PlayerController playerController; // Reference to the PlayerController to get ammo count

    void Update()
    {
        if (playerController != null && ammoText != null)
        {
            ammoText.text = "Ammo: " + playerController.currentAmmo.ToString(); // Update the ammo count on UI
        }
    }
}
