using UnityEngine;
using System.Collections;

public class Explosion : MonoBehaviour
{
    public int damage = 2;
    public int pierce = 3;
    public float radius = 3f;
    public float lifetime = 1f; // Lifetime of the explosion
    public float destroyDelay = 0.5f; // Delay before destruction after explosion
    public AudioClip explosionSound; // Sound clip for the explosion
    private AudioSource audioSource; // AudioSource component to play the sound

    void Start()
    {
        Debug.Log("Explosion started.");
        Destroy(gameObject, lifetime); // Destroy explosion after a short time

        // Add and configure SphereCollider
        SphereCollider collider = gameObject.AddComponent<SphereCollider>();
        collider.isTrigger = true;
        collider.radius = radius; // Set the size of the sphere collider

        // Add and configure AudioSource
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.clip = explosionSound;
        audioSource.playOnAwake = false;

        // Play explosion sound
        PlayExplosionSound();
    }

    void OnTriggerEnter(Collider other)
    {
        Debug.Log("Explosion collided with: " + other.gameObject.name); // Log collision for debugging
        if (other.CompareTag("Enemy"))
        {
            EnemyController enemy = other.GetComponent<EnemyController>();
            SimpleEnemyController simpleEnemy = other.GetComponent<SimpleEnemyController>();
            ComplexEnemyController complexEnemy = other.GetComponent<ComplexEnemyController>();
            ZombieCharacterControl zombie = other.GetComponent<ZombieCharacterControl>();
            if (enemy != null)
            {
                enemy.TakeDamage(damage);
                Debug.Log("Enemy took damage from explosion.");
            }
            else if (simpleEnemy != null)
            {
                simpleEnemy.TakeDamage(damage);
                Debug.Log("Simple enemy took damage from explosion.");
            }
            else if (complexEnemy != null)
            {
                complexEnemy.TakeDamage(damage);
                Debug.Log("Complex enemy took damage from explosion.");
            }
            else if (zombie != null)
            {
                zombie.TakeDamage(damage);
                Debug.Log("Zombie took damage from explosion.");
            }

            StartCoroutine(DestroyAfterDelay()); // Delay destruction after hitting an enemy
        }
        else if (other.CompareTag("Shield"))
        {
            ShieldScript shield = other.GetComponent<ShieldScript>();
            if (shield != null)
            {
                shield.TakeDamage(damage, pierce);
                Debug.Log("Shield hit by explosion.");
            }
            StartCoroutine(DestroyAfterDelay()); // Delay destruction after hitting a shield
        }
    }

    void PlayExplosionSound()
    {
        if (explosionSound != null && audioSource != null)
        {
            audioSource.Play();
            Debug.Log("Explosion sound played.");
        }
        else
        {
            Debug.LogWarning("Explosion sound or audioSource is not set.");
        }
    }

    IEnumerator DestroyAfterDelay()
    {
        yield return new WaitForSeconds(destroyDelay);
        Destroy(gameObject);
    }
}


