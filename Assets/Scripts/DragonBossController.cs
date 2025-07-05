using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro; // Import TextMeshPro namespace
using UnityEngine.UI; // Import UI namespace

public class DragonBossController : MonoBehaviour
{
    public Animator animator;
    public float groundSpeed = 5f;
    public float flyingSpeed = 7f;
    public float pushBackDistance = 1f; // Distance to push back upon collision
    public GameObject fireballPrefab; // Fireball prefab for flame attacks
    public Transform fireballSpawnPoint; // Spawn point for fireballs
    public TMP_Text endGameText; // Reference to the end game text UI element
    public TMP_Text healthBarText; // Reference to the health bar text UI element
    public Slider healthBar; // Reference to the health bar slider UI element
    public int maxHealth = 100;
    private int currentHealth;
    private Vector3 targetPosition;
    private bool isFlying = false;
    private bool isMoving = false;
    private bool isDragonDead = false;

    [Header("Sound Clips")]
    public AudioClip basicAttackSound;
    public AudioClip clawAttackSound;
    public AudioClip flameAttackSound;
    public AudioClip flyFlameAttackSound;
    public AudioClip screamSound;
    public AudioClip defendSound;
    public AudioClip dieSound;
    public AudioClip sleepSound;
    public AudioClip flyForwardSound;
    public AudioClip wakeUpSound;
    public AudioClip landSound;
    public AudioClip startFlyingSound;
    private AudioSource audioSource;

    void Start()
    {
        targetPosition = transform.position;
        currentHealth = maxHealth;
        healthBar.maxValue = maxHealth;
        healthBar.value = currentHealth;
        UpdateHealthBarText();
        audioSource = GetComponent<AudioSource>();
        // Transition to sleep immediately after starting
        animator.SetTrigger("Sleep");
        PlaySound(sleepSound);
    }

    void Update()
    {
        HandleMouseInput();
        HandleAttackAndDefendInput();
        Move();
        RotateTowardsMovementDirection();
        CheckPlayers(); // Continuously check player status
    }

    void HandleMouseInput()
    {
        if (Input.GetMouseButtonDown(0)) // Left click to move or fly to the target position
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit))
            {
                if (isFlying)
                {
                    // Flying movement
                    targetPosition = hit.point;
                    isMoving = true;
                    animator.SetTrigger("FlyForward");
                    PlaySound(flyForwardSound);
                }
                else if (hit.collider.CompareTag("Floor"))
                {
                    // Ground movement
                    targetPosition = hit.point;
                    isMoving = true;
                    animator.SetTrigger("Run");
                }
                // Wake up the dragon on the first left click if it's sleeping
                if (animator.GetCurrentAnimatorStateInfo(0).IsName("Sleep"))
                {
                    animator.SetTrigger("WakeUp");
                    PlaySound(wakeUpSound);
                }
            }
        }

        if (Input.GetMouseButtonDown(1)) // Right click to toggle flying
        {
            isFlying = !isFlying;
            if (isFlying)
            {
                animator.SetTrigger("StartFlying");
                PlaySound(startFlyingSound);
            }
            else
            {
                animator.SetTrigger("Land");
                PlaySound(landSound);
                StopSound(startFlyingSound);
            }
        }
    }
    void HandleAttackAndDefendInput()
    {
        if (Input.GetKeyDown(KeyCode.Z)) // Basic attack
        {
            animator.SetTrigger("BasicAttack");
            PlaySound(basicAttackSound);
            CreateAttackCollider("BasicAttackCollider");
            StartCoroutine(ResetTriggerAfterDelay("BasicAttack", 1.0f)); // Adjust delay to match animation length
        }
        if (Input.GetKeyDown(KeyCode.X)) // Claw attack
        {
            animator.SetTrigger("ClawAttack");
            PlaySound(clawAttackSound);
            CreateAttackCollider("ClawAttackCollider");
            StartCoroutine(ResetTriggerAfterDelay("ClawAttack", 2.8f)); // Adjust delay to match animation length
        }
        if (Input.GetKeyDown(KeyCode.C)) // Flame attack or Flying Flame attack
        {
            if (isFlying)
            {
                animator.SetTrigger("FlyFlameAttack");
                PlaySound(flyFlameAttackSound);
                LaunchFireball();
                StartCoroutine(ResetTriggerAfterDelay("FlyFlameAttack", 1.5f)); // Adjust delay to match animation length
            }
            else
            {
                animator.SetTrigger("FlameAttack");
                PlaySound(flameAttackSound);
                LaunchFireball();
                StartCoroutine(ResetTriggerAfterDelay("FlameAttack", 1.7f)); // Adjust delay to match animation length
            }
        }
        if (Input.GetKeyDown(KeyCode.Q)) // Scream
        {
            animator.SetTrigger("Scream");
            PlaySound(screamSound);
            StartCoroutine(ResetTriggerAfterDelay("Scream", 2.3f)); // Adjust delay to match animation length
        }
        if (Input.GetKeyDown(KeyCode.F) && !isFlying) // Defend only when grounded
        {
            animator.SetTrigger("Defend");
            PlaySound(defendSound);
            StartCoroutine(ResetTriggerAfterDelay("Defend", 2.3f)); // Adjust delay to match animation length
        }
    }

    void CreateAttackCollider(string colliderName)
    {
        GameObject colliderObj = new GameObject(colliderName);
        colliderObj.transform.position = transform.position; // Adjust based on the dragon's position
        colliderObj.transform.rotation = transform.rotation;
        colliderObj.transform.parent = transform;

        BoxCollider collider = colliderObj.AddComponent<BoxCollider>();
        collider.isTrigger = true;
        collider.size = new Vector3(1.0f, 1.0f, 1.0f); // Adjust the size of the collider as needed
        collider.center = new Vector3(0, 1.0f, 2.0f); // Adjust the position of the collider as needed

        colliderObj.AddComponent<DestroyAfterTime>().Initialize(0.5f); // Adjust time for collider to exist
    }

    void PlaySound(AudioClip clip)
    {
        if (clip != null && audioSource != null)
        {
            audioSource.PlayOneShot(clip);
        }
    }

    void StopSound(AudioClip clip)
    {
        if (clip != null && audioSource.isPlaying)
        {
            audioSource.Stop();
        }
    }
    void Move()
    {
        if (isMoving)
        {
            float speed = isFlying ? flyingSpeed : groundSpeed;
            Vector3 newPosition = Vector3.MoveTowards(transform.position, targetPosition, speed * Time.deltaTime);

            // Check for collision with walls only when grounded
            if (!isFlying && Physics.Raycast(transform.position, newPosition - transform.position, out RaycastHit hit, speed * Time.deltaTime))
            {
                if (hit.collider.CompareTag("Wall"))
                {
                    // Stop movement and apply push-back effect
                    isMoving = false;
                    Vector3 pushBackDirection = (transform.position - hit.point).normalized;
                    transform.position += pushBackDirection * pushBackDistance;
                    animator.SetTrigger("Idle");
                }
                else
                {
                    transform.position = newPosition;
                }
            }
            else
            {
                transform.position = newPosition;
            }

            if (Vector3.Distance(transform.position, targetPosition) < 0.1f)
            {
                isMoving = false;
                animator.SetTrigger(isFlying ? "Float" : "Idle");
            }
        }
    }

    void RotateTowardsMovementDirection()
    {
        if (isMoving)
        {
            Vector3 direction = (targetPosition - transform.position).normalized;
            if (direction != Vector3.zero)
            {
                Quaternion targetRotation = Quaternion.LookRotation(direction);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 10f); // Adjust rotation speed as needed
            }
        }
    }

    void OnTriggerEnter(Collider other)
    {
        Debug.Log("Dragon collided with: " + other.gameObject.name); // Log for debugging
        if (other.CompareTag("Player"))
        {
            PlayerController player = other.GetComponent<PlayerController>();
            if (player != null)
            {
                player.TakeDamage(); // Adjust damage value as needed
                Debug.Log("Player hit by Dragon!");
            }
            else
            {
                Player2Controller player2 = other.GetComponent<Player2Controller>();
                if (player2 != null)
                {
                    player2.TakeDamage(); // Adjust damage value as needed
                    Debug.Log("Player2 hit by Dragon!");
                }
            }
        }
        else if (other.CompareTag("Bullet"))
        {
            TakeDamage(5); // Take 5 damage from Bullet
        }
        else if (other.CompareTag("Bayonet"))
        {
            TakeDamage(2); // Take 2 damage from Bayonet
        }
        else if (other.CompareTag("Projectile"))
        {
            TakeDamage(10); // Take 10 damage from Projectile
        }
        else if (other.CompareTag("Fire"))
        {
            TakeDamage(1); // Take 1 damage from Fire
        }
    }

    void LaunchFireball()
    {
        GameObject fireball = Instantiate(fireballPrefab, fireballSpawnPoint.position, fireballSpawnPoint.rotation);
        fireball.GetComponent<Rigidbody>().velocity = fireballSpawnPoint.forward * 10f; // Adjust speed as needed
    }

    public void TakeDamage(int damage)
    {
        currentHealth -= damage;
        healthBar.value = currentHealth; // Update health bar
        UpdateHealthBarText(); // Update health bar text

        if (currentHealth <= 0 && !isDragonDead)
        {
            isDragonDead = true;
            healthBar.gameObject.SetActive(false); // Hide the health bar
            healthBarText.gameObject.SetActive(false); // Hide the health bar text
            StartCoroutine(HandleDragonDeath());
        }
    }

    void UpdateHealthBarText()
    {
        healthBarText.text = $"{currentHealth} / {maxHealth}";
    }

    IEnumerator HandleDragonDeath()
    {
        animator.SetTrigger("Die");
        PlaySound(dieSound);
        yield return new WaitForSeconds(3.0f); // Adjust delay to match the length of the "Die" animation
        // Display message
        endGameText.text = "You defeated the Dragon Boss!";
        endGameText.gameObject.SetActive(true);
        yield return new WaitForSeconds(2.5f); // Adjust delay if necessary
        Destroy(gameObject);
    }

    IEnumerator HandlePlayersDeath()
    {
        yield return new WaitForSeconds(1.0f); // Adjust delay if necessary
        // Display message
        endGameText.text = "Player 3 Dragon wins";
        endGameText.gameObject.SetActive(true);
        yield return new WaitForSeconds(1.5f); // Adjust delay if necessary
        GameManager.instance.RestartLevel();
    }

    IEnumerator ResetTriggerAfterDelay(string triggerName, float delay)
    {
        yield return new WaitForSeconds(delay);
        animator.ResetTrigger(triggerName);
        animator.SetTrigger(isFlying ? "Float" : "Idle");
    }

    void CheckPlayers()
    {
        if (GameObject.FindGameObjectsWithTag("Player").Length == 0 && !isDragonDead)
        {
            StartCoroutine(HandlePlayersDeath());
        }
    }
}
