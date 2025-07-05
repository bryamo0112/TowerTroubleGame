using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProjectileController : MonoBehaviour
{
    // --- Config ---
    public float speed = 100;
    public LayerMask collisionLayerMask;
    public int damage = 5; // Set damage to 5
    public int pierce = 5; // Set pierce to 5

    // --- Explosion VFX ---
    public GameObject rocketExplosion;

    // --- Projectile Mesh ---
    public MeshRenderer projectileMesh;

    // --- Script Variables ---
    private bool targetHit;

    // --- Audio ---
    public AudioSource inFlightAudioSource;

    // --- VFX ---
    public ParticleSystem disableOnHit;

    private void Start()
    {
        // Ensure the projectile has a Box Collider
        BoxCollider boxCollider = gameObject.AddComponent<BoxCollider>();
    }

    private void Update()
    {
        // --- Check to see if the target has been hit. We don't want to update the position if the target was hit ---
        if (targetHit) return;

        // --- moves the game object in the forward direction at the defined speed ---
        transform.position += transform.forward * (speed * Time.deltaTime);
    }

    private void OnCollisionEnter(Collision collision)
{
    // --- return if not enabled because OnCollision is still called if component is disabled ---
    if (!enabled) return;

    // --- Explode when hitting an object and disable the projectile mesh ---
    Explode();
    projectileMesh.enabled = false;
    targetHit = true;
    inFlightAudioSource.Stop();
    foreach (Collider col in GetComponents<Collider>())
    {
        col.enabled = false;
    }
    disableOnHit.Stop();

    // --- Check for collisions with enemies, shields, and boulders ---
    if (collision.gameObject.CompareTag("Enemy"))
{
    var enemy = collision.gameObject.GetComponent<EnemyController>();
    if (enemy != null)
    {
        enemy.TakeDamage(damage);
    }
    else
    {
        var simpleEnemy = collision.gameObject.GetComponent<SimpleEnemyController>();
        if (simpleEnemy != null)
        {
            simpleEnemy.TakeDamage(damage);
        }
        else
        {
            var complexEnemy = collision.gameObject.GetComponent<ComplexEnemyController>();
            if (complexEnemy != null)
            {
                complexEnemy.TakeDamage(damage);
            }
            else
            {
                var zombie = collision.gameObject.GetComponent<ZombieCharacterControl>();
                if (zombie != null)
                {
                    zombie.TakeDamage(damage);
                }
            }
        }
    }
}
else if (collision.gameObject.CompareTag("Dragon"))
{
    var dragon = collision.gameObject.GetComponent<DragonBossController>();
    if (dragon != null)
    {
        dragon.TakeDamage(damage);
    }
}


    else if (collision.gameObject.CompareTag("Shield"))
    {
        ShieldScript shield = collision.gameObject.GetComponent<ShieldScript>();
        if (shield != null)
        {
            shield.TakeDamage(damage, pierce);
        }
    }
    else if (collision.gameObject.CompareTag("Boulder")) // Add this block for handling boulders
    {
        Destroy(collision.gameObject);
        Debug.Log("Boulder destroyed by projectile!");
    }

    // --- Destroy this object after 5 seconds. Using a delay because the particle system needs to finish ---
    Destroy(gameObject, 5f);
}


    private void Explode()
    {
        // --- Instantiate new explosion option. I would recommend using an object pool ---
        GameObject newExplosion = Instantiate(rocketExplosion, transform.position, rocketExplosion.transform.rotation, null);
    }
}


