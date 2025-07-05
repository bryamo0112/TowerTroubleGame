using UnityEngine;
using System.Collections;

public class DestroyAfterTime : MonoBehaviour
{
    private float lifetime;

    public void Initialize(float time)
    {
        lifetime = time;
        StartCoroutine(DestroyAfterDelay());
    }

    private IEnumerator DestroyAfterDelay()
    {
        yield return new WaitForSeconds(lifetime);
        Destroy(gameObject);
    }
}

