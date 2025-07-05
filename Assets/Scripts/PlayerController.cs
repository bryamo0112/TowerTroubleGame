using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement; // Required for scene management
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using TMPro; // Import TextMeshPro namespace

public class PlayerController : MonoBehaviour
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
    public GameObject bulletPrefab; // Reference to the bullet prefab
    public Transform bulletSpawnPoint; // Point where bullets are spawned

    private bool isInvincible = false;
    public float invincibilityDuration = 2.0f;

    public Image healthImage; // Single Image component
    public Sprite[] healthSprites; // Array of sprites (frames of the sprite sheet)
    
    // Add references for the weapons
    public GameObject preEquippedBayonet;
    public GameObject bayonetPrefab;
    public GameObject rocketLauncherPrefab;
    
    private GameObject currentBayonet;
    private GameObject groundRocketLauncher;
    private GameObject equippedRocketLauncher;

    // Track if the rocket launcher has been equipped before
    private bool rocketLauncherEquippedBefore = false;

    // Position and rotation of the weapons
    private Vector3 bayonetPosition;
    private Quaternion bayonetRotation;
    private Vector3 rocketLauncherPosition;
    private Quaternion rocketLauncherRotation;

    // Reference to the weapon container
    private Transform weaponContainer;

    public int maxAmmo = 20; // Maximum ammo capacity
    public int currentAmmo;  // Current ammo
    public TextMeshProUGUI ammoText; // Reference to the UI text for ammo

    // Rocket ammo implementation
    public int maxRocket = 5; // Maximum rocket ammo capacity
    public int currentRocket; // Current rocket ammo
    public TextMeshProUGUI rocketText; // Reference to the UI text for rocket ammo

    [Header("Sound Clips")] 
    public AudioClip shootingSound;
    public AudioClip slashingSound;
    public AudioClip deathSound;

    void Start()
    {
        GameManager.playersAtPortal = 0;
        rb = GetComponent<Rigidbody>();
        animator = GetComponent<Animator>();
        audioSource = GetComponent<AudioSource>();
        currentHealth = maxHealth;
        currentAmmo = maxAmmo; // Initialize current ammo to the max value
        currentRocket = maxRocket; // Initialize current rocket ammo to the max value
        UpdateAmmoUI(); // Update the UI with the current ammo count
        UpdateRocketUI(); // Update the UI with the current rocket ammo count
        demoCompleteText.gameObject.SetActive(false); // Hide the text initially
        UpdateHealthBar();
        // Enable gravity and set Rigidbody properties for smoother movement and collision detection
        rb.useGravity = true;
        rb.interpolation = RigidbodyInterpolation.Interpolate;
        rb.collisionDetectionMode = CollisionDetectionMode.Continuous;
        rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationY | RigidbodyConstraints.FreezeRotationZ;

        // Track the weapon container for future weapon replacements
        weaponContainer = transform.Find("Bip001/Bip001 Pelvis/Bip001 Spine/Bip001 R Clavicle/Bip001 R UpperArm/Bip001 R Forearm/Bip001 R Hand/R_hand_container");

        // Track the pre-equipped bayonet's position and rotation
        preEquippedBayonet = transform.Find("Bip001/Bip001 Pelvis/Bip001 Spine/Bip001 R Clavicle/Bip001 R UpperArm/Bip001 R Forearm/Bip001 R Hand/R_hand_container/Bayonet").gameObject;

        // Ignore collision between the player and its bayonet
        Collider playerCollider = GetComponent<Collider>();
        if (preEquippedBayonet != null)
        {
            Collider bayonetCollider = preEquippedBayonet.GetComponent<Collider>();
            Physics.IgnoreCollision(playerCollider, bayonetCollider);
        }
        else
        {
            Debug.LogError("Bayonet not found in the player's hierarchy");
        }

        // Track position and rotation of the pre-equipped bayonet
        bayonetPosition = preEquippedBayonet.transform.localPosition;
        bayonetRotation = preEquippedBayonet.transform.localRotation;
    }
    void Update() 
    { 
        Move();
        Jump();
        if (equippedRocketLauncher == null) 
        {
            Shoot();
            Slash();
        }
        HandleEquip(); // Call HandleEquip in the Update loop 

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
    
    void Move()
    {
        float moveHorizontal = 0;
        float moveVertical = 0;
        if (Input.GetKey(KeyCode.A)) moveHorizontal = -1;
        if (Input.GetKey(KeyCode.D)) moveHorizontal = 1;
        if (Input.GetKey(KeyCode.W)) moveVertical = 1;
        if (Input.GetKey(KeyCode.S)) moveVertical = -1;
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
    void Jump()
    {
        if (Input.GetKeyDown(KeyCode.Space) && isGrounded)
        {
            rb.AddForce(new Vector3(0, jumpForce, 0), ForceMode.Impulse);
            animator.SetTrigger("isJumping");
            isGrounded = false;
        }
    }

    void Shoot()
    {
        if (Input.GetKeyDown(KeyCode.K) && currentAmmo > 0) // Check if player has ammo
        {
            animator.SetTrigger("Shooting");
            PlaySound(shootingSound);
            Instantiate(bulletPrefab, bulletSpawnPoint.position, bulletSpawnPoint.rotation);
            currentAmmo--; // Decrease ammo by 1
            UpdateAmmoUI(); // Update the ammo display
            Debug.Log("Bullet shot, remaining ammo: " + currentAmmo);
        }
        else if (currentAmmo <= 0)
        {
            Debug.Log("No ammo left!"); // Player can't shoot if ammo is 0
        }
    }
    public void UpdateAmmoUI()
    {
        ammoText.text = "X" + currentAmmo.ToString(); // Update the ammo display text
    }
    public void GainAmmo(int amount)
    {
        currentAmmo = Mathf.Min(currentAmmo + amount, maxAmmo); // Add ammo but don't exceed maxAmmo
        UpdateAmmoUI(); // Update the UI with the new ammo count
        Debug.Log("Ammo picked up. Current ammo: " + currentAmmo);
    }

    public void UpdateRocketUI()
    {
        rocketText.text = "X" + currentRocket.ToString(); // Update the rocket ammo display text
    }

    public void GainRocket(int amount)
    {
        currentRocket = Mathf.Min(currentRocket + amount, maxRocket); // Add rocket ammo but don't exceed maxRocket
        UpdateRocketUI(); // Update the UI with the new rocket ammo count
        Debug.Log("Rocket ammo picked up. Current rocket ammo: " + currentRocket);
    }
    void Slash()
    {
        if (Input.GetKeyDown(KeyCode.O))
        {
            animator.SetTrigger("Slashing");
            PlaySound(slashingSound);
            Debug.Log("Slashing trigger set");
            EnableInvincibility();
        }
    }

    void HandleEquip()
    {
        // Equip the rocket launcher if the player is near the rocket launcher object and presses "V"
        if (groundRocketLauncher != null && Input.GetKeyDown(KeyCode.V))
        {
            Destroy(preEquippedBayonet); // Ensure the pre-equipped bayonet is destroyed
            EquipRocketLauncher();
            Destroy(groundRocketLauncher); // Destroy the rocket launcher object on the ground
            groundRocketLauncher = null; // Clear the reference to prevent re-destruction
        }

        // Switch back to the bayonet with key "1"
        if (Input.GetKeyDown(KeyCode.Alpha1) && equippedRocketLauncher != null)
        {
            EquipBayonet();
        }

        // Switch to the rocket launcher with key "2" if it has been equipped before
        if (rocketLauncherEquippedBefore && Input.GetKeyDown(KeyCode.Alpha2))
        {
            EquipRocketLauncher();
        }
    }

    void EquipBayonet()
    {
        // Destroy the current weapon if it's the rocket launcher
        if (equippedRocketLauncher != null)
        {
            rocketLauncherPosition = equippedRocketLauncher.transform.localPosition;
            rocketLauncherRotation = equippedRocketLauncher.transform.localRotation;
            Destroy(equippedRocketLauncher);
            equippedRocketLauncher = null;
        }

        // Equip the bayonet prefab
        if (currentBayonet == null)
        {
            currentBayonet = Instantiate(bayonetPrefab, weaponContainer);
            currentBayonet.transform.localPosition = bayonetPosition;
            currentBayonet.transform.localRotation = bayonetRotation;
        }
    }

    void EquipRocketLauncher()
    {
        // Destroy the current weapon if it's the bayonet
        if (currentBayonet != null)
        {
            bayonetPosition = currentBayonet.transform.localPosition;
            bayonetRotation = currentBayonet.transform.localRotation;
            Destroy(currentBayonet);
            currentBayonet = null;
        }

        // Equip the rocket launcher prefab
        if (equippedRocketLauncher == null)
        {
            equippedRocketLauncher = Instantiate(rocketLauncherPrefab, weaponContainer.position, weaponContainer.rotation, weaponContainer);
            equippedRocketLauncher.transform.localPosition = Vector3.zero; // Set the local position to zero to match the hand position
            equippedRocketLauncher.transform.localRotation = Quaternion.Euler(0, -90, 0); // Set the y-axis rotation to -90 degrees
            rocketLauncherEquippedBefore = true;
        }
    }
    public void TakeDamage()
    {
        if (!isInvincible)
        {
            currentHealth--;
            UpdateHealthBar();
            Debug.Log("Player Health: " + currentHealth);
            if (currentHealth <= 0)
            {
                Die();
            }
        }
    }

    void EnableInvincibility()
    {
        isInvincible = true;
        StartCoroutine(DisableInvincibility());
    }

    IEnumerator DisableInvincibility()
    {
        yield return new WaitForSeconds(invincibilityDuration);
        isInvincible = false;
    }

    void GainHealth()
    {
        currentHealth = Mathf.Min(currentHealth + 1, maxHealth);
        UpdateHealthBar();
        Debug.Log("Player Health: " + currentHealth);
    }

    void UpdateHealthBar()
    {
        // Clamp currentHealth between 0 and maxHealth
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);

        // Ensure the array has enough sprites for each health level
        if (currentHealth <= healthSprites.Length - 1)
        {
            // Update the health image to the corresponding sprite based on current health
            healthImage.sprite = healthSprites[currentHealth];
        }
        else
        {
            Debug.LogError("Health sprite array does not have enough sprites for the current health value.");
        }

        Debug.Log("Updating health bar");
    }

    void PickUpTool()
    {
        hasTool = true;
        Debug.Log("Tool picked up.");
    }

    void Die()
    {
        animator.SetTrigger("Death");
        PlaySound(deathSound);
        Debug.Log("Player is dead. Game Over.");
        this.enabled = false;
        Destroy(gameObject, 1f);
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
    void OnCollisionEnter(Collision collision)
    {
        Debug.Log("Collided with: " + collision.gameObject.name);
        if (collision.gameObject.CompareTag("Floor"))
        {
            isGrounded = true;
            rb.velocity = Vector3.zero;
            animator.ResetTrigger("isJumping");
            // Ensure the correct animation state is set after landing
            if (rb.velocity.magnitude > 1f)
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
        else if (other.gameObject.CompareTag("RocketLauncher"))
        {
            // Reference the rocket launcher object
            if (groundRocketLauncher == null)
            {
                groundRocketLauncher = other.gameObject;
                Debug.Log("Rocket Launcher detected on ground");
            }
        }
        else if (other.gameObject.CompareTag("Portal"))
        {
            Debug.Log("Portal collided with!");
            StartCoroutine(HandlePortalCollision());
        }
        else if (other.gameObject.CompareTag("AmmoPickup"))
        {
            GainAmmo(5); // Gain 5 ammo when picking up an ammo box (or adjust as needed)
            Destroy(other.gameObject); // Destroy the ammo pickup object
        }
        else if (other.gameObject.CompareTag("RocketPickup"))
        {
            GainRocket(3); // Gain 3 rocket ammo when picking up a rocket ammo box (or adjust as needed)
            Destroy(other.gameObject); // Destroy the rocket ammo pickup object
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

    void TakeTrapDamage() 
    { 
        if (!isInvincible) 
        { 
            currentHealth--; 
            Debug.Log("Player Health: " + currentHealth); 
            UpdateHealthBar(); 
            StartCoroutine(FlinchAndInvincibility());
            if (currentHealth <= 0) 
            { 
                Die(); 
            }
        }
    }
    
    IEnumerator FlinchAndInvincibility()
    {
        isInvincible = true;
        // Apply flinch effect
        Vector3 flinchDirection = -transform.forward * 100; // Increase the strength of the flinch
        rb.AddForce(flinchDirection, ForceMode.Impulse);
        Debug.Log("Flinch applied with direction: " + flinchDirection);

        yield return new WaitForSeconds(1f); // Duration of invincibility
        isInvincible = false;
    }
    void PlaySound(AudioClip clip)
    { 
        if (clip != null && audioSource != null)
        { 
            audioSource.PlayOneShot(clip); 
        } 
    }
}
