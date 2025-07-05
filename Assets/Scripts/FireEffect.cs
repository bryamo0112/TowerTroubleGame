using UnityEngine;
using System.Collections;

public class FireEffect : MonoBehaviour
{
    public int damage = 1;
    public int pierce = 3;
    public float lifetime = 0.5f; // Short lifetime to dissipate quickly
    public float colliderLength = 5f; // Length of the collider to match the flame
    public float colliderRadius = 1f; // Radius of each sphere collider
    public float destroyDelay = 2.0f; // Delay before destruction after hitting an enemy or shield
    public AudioClip fireSound; // Sound clip for the fire effect
    private AudioSource audioSource; // AudioSource component to play the sound
    private bool isSoundPlaying = false; // Flag to check if sound is playing
    private float soundCooldown; // Cooldown period before sound can play again

    void Start()
    {
        Debug.Log("Fire effect started.");
        Destroy(gameObject, lifetime); // Destroy fire effect after a short time

        BoxCollider collider = gameObject.AddComponent<BoxCollider>();
        collider.isTrigger = true;
        collider.size = new Vector3(colliderRadius, colliderRadius, colliderLength); // Set the size of the box collider
        collider.center = new Vector3(0, 0, colliderLength / 2); // Center the collider

        // Add and configure AudioSource
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.playOnAwake = false;

        // Calculate sound cooldown based on clip length plus a buffer
        if (fireSound != null)
        {
            soundCooldown = fireSound.length + 0.5f; // Adding a buffer to the sound length
        }

        // Play fire sound once at the start using PlayOneShot
        PlayFireSound();
    }

    void OnTriggerEnter(Collider other)
    {
        Debug.Log("Fire collided with: " + other.gameObject.name); // Log collision for debugging
        if (other.CompareTag("Enemy"))
        {
            EnemyController enemy = other.GetComponent<EnemyController>();
            SimpleEnemyController simpleEnemy = other.GetComponent<SimpleEnemyController>();
            ComplexEnemyController complexEnemy = other.GetComponent<ComplexEnemyController>();
            ZombieCharacterControl zombie = other.GetComponent<ZombieCharacterControl>();

            if (enemy != null)
            {
                enemy.TakeDamage(damage);
                Debug.Log("Enemy took damage from fire.");
            }
            else if (simpleEnemy != null)
            {
                simpleEnemy.TakeDamage(damage);
                Debug.Log("Simple enemy took damage from fire.");
            }
            else if (complexEnemy != null)
            {
                complexEnemy.TakeDamage(damage);
                Debug.Log("Complex enemy took damage from fire.");
            }
            else if (zombie != null)
            {
                zombie.TakeDamage(damage);
                Debug.Log("Zombie took damage from fire.");
            }
            StartCoroutine(DestroyAfterDelay()); // Delay destruction after hitting an enemy
        }
        else if (other.CompareTag("Shield"))
        {
            ShieldScript shield = other.GetComponent<ShieldScript>();
            if (shield != null)
            {
                shield.TakeDamage(damage, pierce);
                Debug.Log("Shield hit by fire.");
            }
            StartCoroutine(DestroyAfterDelay()); // Delay destruction after hitting a shield
        }
    }

    void PlayFireSound()
    {
        if (fireSound != null && audioSource != null && !isSoundPlaying)
        {
            isSoundPlaying = true;
            audioSource.PlayOneShot(fireSound);
            Debug.Log("Fire sound played.");
            StartCoroutine(ResetSoundFlag());
        }
        else
        {
            Debug.LogWarning("Fire sound or audioSource is not set, or sound is already playing.");
        }
    }

    IEnumerator ResetSoundFlag()
    {
        yield return new WaitForSeconds(soundCooldown);
        isSoundPlaying = false;
    }

    IEnumerator DestroyAfterDelay()
    {
        yield return new WaitForSeconds(destroyDelay);
        Destroy(gameObject);
    }
}











