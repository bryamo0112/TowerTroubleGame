using UnityEngine;

public class FireTrap : MonoBehaviour
{
    public ParticleSystem fireParticles; // Assign the Particle System in the Inspector
    public float fireInterval = 4f; // Fire every 4 seconds
    private float fireTimer = 0f;
    public bool isActive = false; // Tracks if the trap is firing
    private bool playerDamaged = false; // Prevents multiple damage instances per fire cycle

    private void Update()
    {
        fireTimer += Time.deltaTime;

        if (fireTimer >= fireInterval)
        {
            ShootFire();
            fireTimer = 0f;
        }
    }

    private void ShootFire()
    {
        fireParticles.Play(); // Start the fire particles
        isActive = true; // Mark the trap as firing
        playerDamaged = false; // Reset the damage flag for the new fire cycle
        Invoke(nameof(StopFiring), fireParticles.main.duration); // Stop firing after the particles finish
    }

    private void StopFiring()
    {
        isActive = false; // Mark the trap as inactive
    }

    private void OnTriggerEnter(Collider other)
    {
        if (isActive && !playerDamaged && other.CompareTag("Player"))
        {
            DamagePlayer(other);
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (isActive && !playerDamaged && other.CompareTag("Player"))
        {
            DamagePlayer(other);
        }
    }

    private void DamagePlayer(Collider other)
    {
        var playerController = other.GetComponent<PlayerController>();
        if (playerController != null)
        {
            playerController.TakeDamage(); // Deal damage to Player 1
            playerDamaged = true; // Ensure damage happens only once per fire cycle
            return;
        }

        var player2Controller = other.GetComponent<Player2Controller>();
        if (player2Controller != null)
        {
            player2Controller.TakeDamage(); // Deal damage to Player 2
            playerDamaged = true; // Ensure damage happens only once per fire cycle
            return;
        }

        var player3Controller = other.GetComponent<Player3Controller>();
        if (player3Controller != null)
        {
            player3Controller.TakeDamage(); // Deal damage to Player 3
            playerDamaged = true; // Ensure damage happens only once per fire cycle
        }
    }
}

