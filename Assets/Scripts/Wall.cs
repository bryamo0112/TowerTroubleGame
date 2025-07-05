using UnityEngine;

public class Wall : MonoBehaviour
{
    public int hitsToDestroy = 2; // Number of hits before the wall is destroyed
    private int currentHits = 0;

    public void TakeDamage()
    {
        currentHits++;
        Debug.Log("Wall took damage. Current hits: " + currentHits);

        if (currentHits >= hitsToDestroy)
        {
            Destroy(gameObject);
            Debug.Log("Wall destroyed.");
        }
    }
}


