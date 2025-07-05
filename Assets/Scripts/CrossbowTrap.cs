using UnityEngine;
using System.Collections;

public class CrossbowTrap : MonoBehaviour
{
    public Transform arrowSpawnPoint;
    public GameObject arrowPrefab;
    public float detectionRange = 20f; // Increase detection range
    public float reloadTime = 1f;
    private Transform player;
    private bool isReloading = false;
    public LayerMask detectionLayers; // Add this

    void Update()
    {
        DetectPlayer();
    }

    void DetectPlayer()
    {
        RaycastHit hit;

        // Debug line to visualize raycast in Scene view
        Debug.DrawRay(transform.position, transform.forward * detectionRange, Color.red);

        // Detect the player in a straight line with the specified layer mask
        if (Physics.Raycast(transform.position, transform.forward, out hit, detectionRange, detectionLayers))
        {
            Debug.Log("Raycast hit something: " + hit.collider.name);
            if (hit.collider.CompareTag("Player"))
            {
                Debug.Log("Player detected: " + hit.collider.name);
                player = hit.transform;
                if (!isReloading)
                {
                    StartCoroutine(FireArrow());
                }
            }
        }
        else
        {
            Debug.Log("No hit detected");
            player = null;
        }
    }

    IEnumerator FireArrow()
    {
        isReloading = true;

        Debug.Log("Firing arrow");

        // Fire the arrow
        GameObject arrow = Instantiate(arrowPrefab, arrowSpawnPoint.position, arrowSpawnPoint.rotation);
        arrow.GetComponent<Rigidbody>().velocity = arrowSpawnPoint.forward * 20f;

        // Wait for reload time
        yield return new WaitForSeconds(reloadTime);

        Debug.Log("Reloading complete");

        isReloading = false;
    }
}





















