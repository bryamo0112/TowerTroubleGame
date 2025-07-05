using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections;

public class Player2Controller : MonoBehaviour
{
    public float speed = 5f;
    public float jumpForce = 5f;
    public int maxHealth = 3; // Maximum health
    private int currentHealth;
    private Rigidbody rb;
    private Animator animator;
    private AudioSource audioSource;
    private bool isGrounded;
    public bool hasTool = false; // Track if the player has picked up the tool
    public TextMeshProUGUI demoCompleteText; // TMP UI Text for "Demo Complete"
    public GameObject shieldPrefab; // Assign the shield prefab in the inspector
    public Transform leftHandTransform; // Assign this to the empty GameObject in the left hand in the inspector
    private GameObject currentShield;
    public Image healthImage; // Single Image component
    public Sprite[] healthSprites; // Array of sprites (frames of the sprite sheet)
    public bool isBlocking = false; // Track blocking status
    private bool isInvincible = false; // Track invincibility status
    public GameObject firePrefab; // Reference to the fire prefab
    public Transform fireSpawnPoint; // Point where fire is spawned

    public float maxFuel = 10; // Maximum fuel capacity (set to 10)
    public float currentFuel; // Current fuel level
    public TextMeshProUGUI ammoText; // UI text to display the current fuel/ammo
    [Header("Sound Clips")] 
    public AudioClip shootingflameSound;
    public AudioClip pickupShieldSound;
    public AudioClip BlockSound;
    public AudioClip deathSound;

    void Start()
    {
        // Initialization
        GameManager.playersAtPortal = 0;
        rb = GetComponent<Rigidbody>();
        animator = GetComponent<Animator>();
        audioSource = GetComponent<AudioSource>();
        currentHealth = maxHealth;
        currentFuel = maxFuel;
        UpdateAmmoUI();
        demoCompleteText.gameObject.SetActive(false); // Hide the text initially
        // Enable gravity and set Rigidbody properties for smoother movement and collision detection
        rb.useGravity = true;
        rb.interpolation = RigidbodyInterpolation.Interpolate;
        rb.collisionDetectionMode = CollisionDetectionMode.Continuous;
        rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
    }

    void Update()
    {
        // Update player movement and actions
        Move();
        Jump();
        ShootFlamethrower();
        CheckForShieldPickup();
        Block();
        
        // Restart level with R key
        if (Input.GetKeyDown(KeyCode.R))
        {
            Debug.Log("R key pressed. Restarting level.");
            if (GameManager.instance != null && !GameManager.instance.isRestarting)
            {
                GameManager.instance.RestartLevel();
            }
            else
            {
                Debug.LogError("GameManager instance is null or isRestarting is true");
            }
        }
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Debug.Log("Escape key pressed. Returning to the starting scene.");
            UnityEngine.SceneManagement.SceneManager.LoadScene("StartingScreen");
        }
    }

    void UpdateAmmoUI()
    {
        ammoText.text = "X" + currentFuel.ToString(); // Update the ammo display text
    }

    void ConsumeFuel()
    {
        if (currentFuel > 0)
        {
            currentFuel--; // Reduce fuel by 1 per shot
            UpdateAmmoUI();
        }
    }
    void Move()
    {
        if (!isShooting)
        {
            float moveHorizontal = 0;
            float moveVertical = 0;
            if (Input.GetKey(KeyCode.LeftArrow)) moveHorizontal = -1;
            if (Input.GetKey(KeyCode.RightArrow)) moveHorizontal = 1;
            if (Input.GetKey(KeyCode.UpArrow)) moveVertical = 1;
            if (Input.GetKey(KeyCode.DownArrow)) moveVertical = -1;
            Vector3 movement = new Vector3(moveHorizontal, 0.0f, moveVertical);
            rb.MovePosition(transform.position + movement * speed * Time.deltaTime);
            if (movement != Vector3.zero)
            {
                animator.SetBool("isRunning", true);
                animator.SetBool("isIdle", false);
                transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(movement), 0.15f);
            }
            else
            {
                animator.SetBool("isRunning", false);
                animator.SetBool("isIdle", true);
            }
        }
    }

    private bool isShooting = false;
    
    void Jump()
    {
        if (Input.GetKeyDown(KeyCode.N) && isGrounded)
        {
            rb.AddForce(new Vector3(0, jumpForce, 0), ForceMode.Impulse);
            animator.SetTrigger("isJumping");
            isGrounded = false;
        }
    }
    
    void ShootFlamethrower()
    {
        // Shooting and fuel consumption logic
        if (Input.GetKey(KeyCode.M) && currentFuel > 0) // Only shoot if there's fuel
        {
            isShooting = true;
            if (!animator.GetCurrentAnimatorStateInfo(0).IsName("Shooting"))
            {
                animator.SetTrigger("Shooting");
            }
            InstantiateFire();
            ConsumeFuel(); // Consume 1 fuel per shot
        }
        else
        {
            isShooting = false;
        }
    }
    
    void InstantiateFire()
    {
        GameObject fireInstance = Instantiate(firePrefab, fireSpawnPoint.position, fireSpawnPoint.rotation);
        // Add further fire customization if needed, like setting damage and pierce values here
    }

    void Block()
    {
        if (currentShield != null)
        {
            if (Input.GetKeyDown(KeyCode.B))
            {
                animator.SetTrigger("Block");
                PlaySound(BlockSound);
                animator.SetBool("isBlocking", true);
                isBlocking = true; // Enable blocking
                isInvincible = true; // Enable invincibility
                RotateShieldForBlock(); // Rotate the shield for blocking
                Debug.Log("Blocking animation triggered.");
            }
            if (Input.GetKey(KeyCode.B))
            {
                // Maintain rotation during blocking
                RotateShieldForBlock();
            }
            if (Input.GetKeyUp(KeyCode.B))
            {
                animator.SetBool("isBlocking", false);
                isBlocking = false; // Disable blocking
                isInvincible = false; // Disable invincibility
                ResetShieldRotation(); // Reset rotation when not blocking
                Debug.Log("Blocking animation stopped.");
            }
        }
        else
        {
            isBlocking = false; // Ensure blocking is false if shield is destroyed
            isInvincible = false; // Ensure invincibility is false if shield is destroyed
            animator.SetBool("isBlocking", false); // Exit blocking animation
        }
    }

    void RotateShieldForBlock()
    {
        if (currentShield != null)
        {
            // Adjust the shield rotation to face forward during blocking
            currentShield.transform.localRotation = Quaternion.Euler(0, 0, 180); // Adjust these values to get the correct rotation
        }
    }

    void ResetShieldRotation()
    {
        if (currentShield != null)
        {
            // Reset the shield rotation to its default
            currentShield.transform.localRotation = Quaternion.Euler(0, 0, 0); // Adjust these values if needed
        }
    }
    void OnCollisionEnter(Collision collision)
    {
        Debug.Log("Collided with: " + collision.gameObject.name);
        if (collision.gameObject.CompareTag("Floor"))
        {
            isGrounded = true;
            rb.velocity = Vector3.zero;
            animator.ResetTrigger("isJumping");
            // Ensure the correct animation state is set after landing
            if (rb.velocity.magnitude > 0.1f)
            {
                animator.SetBool("isRunning", true);
                animator.SetBool("isIdle", false);
            }
            else
            {
                animator.SetBool("isRunning", false);
                animator.SetBool("isIdle", true);
            }
        }
        else if (collision.gameObject.CompareTag("Obstacle"))
        {
            if (hasTool)
            {
                Destroy(collision.gameObject); // Destroy the obstacle if the player has the tool
                Debug.Log("Door should open with tool.");
            }
            else
            {
                Debug.Log("Obstacle is impassable without the tool.");
            }
        }
    }

    void OnTriggerEnter(Collider other)
    {
        Debug.Log("Triggered by: " + other.gameObject.name);
        if (other.gameObject.CompareTag("Trap"))
        {
            TakeTrapDamage();
        }
        else if (other.gameObject.CompareTag("Powerup"))
        {
            GainHealth();
            Destroy(other.gameObject); // Destroy the power-up object
        }
        else if (other.gameObject.CompareTag("Enemy"))
        {
            TakeDamage();
        }
        else if (other.gameObject.CompareTag("FireTrap"))
        {
            // Check if the fire trap is active
            FireTrap fireTrap = other.gameObject.GetComponent<FireTrap>();
            if (fireTrap != null && fireTrap.isActive && !isInvincible)
            {
                TakeDamage();
            }
        }
        else if (other.gameObject.CompareTag("Tool"))
        {
            PickUpTool();
            Destroy(other.gameObject); // Destroy the tool object
        }
        else if (other.gameObject.CompareTag("Portal"))
        {
            Debug.Log("Player entered the portal!");
            StartCoroutine(HandlePortalCollision());
        }
        else if (other.gameObject.CompareTag("Fuel"))
        {
            ReplenishFuel(3); // Replenish fuel when picking up a fuel item
            Destroy(other.gameObject); // Destroy the fuel pickup object
        }
    }

    public void ReplenishFuel(int amount)
    {
        currentFuel = Mathf.Min(currentFuel + amount, maxFuel); // Ensure fuel doesn't exceed maxFuel
        UpdateAmmoUI();
    }

    void PickUpTool()
    {
        hasTool = true;
        Debug.Log("Tool picked up!");
    }

    public void TakeDamage()
    {
        if (!isInvincible)
        {
            currentHealth--;
            Debug.Log("Player Health: " + currentHealth);
            UpdateHealthUI();
            if (currentHealth <= 0)
            {
                Die();
            }
        }
    }

    void TakeTrapDamage()
    {
        if (!isInvincible)
        {
            currentHealth--;
            Debug.Log("Player Health: " + currentHealth);
            UpdateHealthUI();
            if (currentHealth <= 0)
            {
                Die();
            }
        }
    }

    void GainHealth()
    {
        if (currentHealth < maxHealth)
        {
            currentHealth++;
            UpdateHealthUI();
        }
    }

    void UpdateHealthUI()
    {
        if (healthImage != null && healthSprites.Length > 0)
        {
            healthImage.sprite = healthSprites[currentHealth];
        }
    }

    void Die()
    {
        animator.SetTrigger("Death");
        PlaySound(deathSound);
        Debug.Log("Player is dead. Game Over.");
        this.enabled = false;
        Destroy(gameObject, 1f);
    }
    public bool IsBlocking()
    {
        return isBlocking;
    }

    private void CheckForShieldPickup()
    {
        if (currentShield == null && Input.GetKeyDown(KeyCode.P))
        {
            PlaySound(pickupShieldSound);
            if (shieldPrefab != null)
            {
                currentShield = Instantiate(shieldPrefab, leftHandTransform.position, Quaternion.identity);
                currentShield.transform.SetParent(leftHandTransform);
                currentShield.transform.localPosition = Vector3.zero; // Adjust the position to match the hand
            }

            // Destroy the shield with the "PlayerShield" tag on the ground
            GameObject groundShield = GameObject.FindGameObjectWithTag("PlayerShield");
            if (groundShield != null)
            {
                Destroy(groundShield);
            }
        }
    }

    IEnumerator HandlePortalCollision()
    {
        if (!gameObject.CompareTag("Processed"))
        {
            GameManager.playersAtPortal++;
            gameObject.tag = "Processed"; // Mark the player as processed to prevent double counting
        }
        Debug.Log("PlayerController: Players at portal: " + GameManager.playersAtPortal);
        yield return new WaitForSeconds(1f); // Delay before disabling controls
        Debug.Log("Destroying player: " + gameObject.name);
        
        // Check if all players have reached the portal
        if (GameManager.playersAtPortal == 3 || GameManager.playersAtPortal == 4 || GameManager.playersAtPortal == 5)
        {
            Debug.Log("All players have reached the portal!");
            if (demoCompleteText != null)
            {
                demoCompleteText.gameObject.SetActive(true);
            }
            Debug.Log("Loading Level2 scene...");
            yield return new WaitForSeconds(.5f); // Wait for the message to be displayed
            GameManager.instance.LoadNextScene();
        }
        yield return new WaitForSeconds(1f);
        Destroy(gameObject,1f);
    }
    void OnDestroy()
    {
        if (GameManager.instance == null)
        {
            Debug.LogError("GameManager instance is null");
            return;
        }

        if (AllPlayersGone() && GameManager.playersAtPortal != 3 && GameManager.playersAtPortal != 4 && GameManager.playersAtPortal != 5)
        {
            Debug.Log("All players gone! Restarting the level.");
            GameManager.instance.RestartLevel();
        }
    }
    bool AllPlayersGone()
    {
        // Check if there are no active players left in the scene
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
        Debug.Log("Number of players left: " + players.Length);
        return players.Length == 0;
    }

    void PlaySound(AudioClip clip)
    {
        if (clip != null && audioSource != null)
        {
            audioSource.PlayOneShot(clip);
        }
    }
}





