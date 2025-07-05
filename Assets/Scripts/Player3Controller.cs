using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement; // Required for scene management
using TMPro; // Import TextMeshPro namespace

public class Player3Controller : MonoBehaviour
{
    public float speed = 5f;
    public Animator animator;
    public AudioSource audioSource;
    public int maxHealth = 3; // Maximum health
    private int currentHealth;
    private Vector3 targetPosition;
    private bool isMoving = false;
    public bool hasTool = false; // Track if the player has picked up the tool
    public TextMeshProUGUI demoCompleteText; // TMP UI Text for "Demo Complete"
    public GridSystem gridSystem; // Reference to GridSystem
    public Pathfinding pathfinding; // Reference to Pathfinding
    private Rigidbody rb; // Rigidbody for floating logic
    public Image healthImage; // Single Image component
    public Sprite[] healthSprites; // Array of sprites (frames of the sprite sheet)
    private bool isInvincible = false; // Track invincibility status
    [Header("Sound Clips")] 
    public AudioClip meowSound;
    public AudioClip deathSound;
    public GameObject laserBeamPrefab;
    public Transform laserBeamSpawnPoint; // The point from where the laser will be fired



    void Start()
    {
        // Reset playersAtPortal at the start of the level
        GameManager.playersAtPortal = 0;

        targetPosition = transform.position;
        currentHealth = maxHealth;
        audioSource = GetComponent<AudioSource>(); // Get the AudioSource component
        rb = GetComponent<Rigidbody>(); // Get the Rigidbody component
        rb.collisionDetectionMode = CollisionDetectionMode.Continuous;
        demoCompleteText.gameObject.SetActive(false); // Hide the text initially
    }

    void Update()
    {
        HandleMouseInput();
        MoveAlongPath(); // Call MoveAlongPath instead of Move
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

    
    void HandleMouseInput()
{
    if (Input.GetMouseButtonDown(0)) // Left click to move
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit))
        {
            if (hit.collider.CompareTag("Floor"))
            {
                targetPosition = hit.point;
                pathfinding.FindPath(transform.position, targetPosition); // Use pathfinding to find path
                isMoving = true;
                animator.SetTrigger("Walk");
            }
            else if (hit.collider.CompareTag("Platform"))
            {
                targetPosition = hit.point;
                StartCoroutine(FloatToTarget(targetPosition)); // Float towards the platform
                isMoving = false;
            }
        }
    }
    if (Input.GetMouseButtonDown(1)) // Right click to meow and shoot laser beam
    {
        Debug.Log("Right click detected. Triggering Miau and Laser Beam.");
        animator.SetTrigger("Sit");
        PlaySound(meowSound);
        ShootLaserBeam();
    }
}


    void MoveAlongPath()
    {
        if (pathfinding.path != null && pathfinding.path.Count > 0)
        {
            Vector3 direction = (pathfinding.path[0].worldPosition - transform.position).normalized;
            if (direction != Vector3.zero)
            {
                Quaternion targetRotation = Quaternion.LookRotation(direction);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 10f); // Adjust rotation speed as needed
            }
            transform.position = Vector3.MoveTowards(transform.position, pathfinding.path[0].worldPosition, speed * Time.deltaTime);

            if (Vector3.Distance(transform.position, pathfinding.path[0].worldPosition) < 0.1f)
            {
                pathfinding.path.RemoveAt(0);
                if (pathfinding.path.Count == 0)
                {
                    isMoving = false;
                    animator.SetTrigger("Idle");
                }
            }
        }
    }
    IEnumerator FloatToTarget(Vector3 target)
    {
        // Move vertically first to avoid obstacles
        while (Mathf.Abs(target.y - transform.position.y) > 0.1f)
        {
            Vector3 verticalDirection = new Vector3(0, target.y - transform.position.y, 0).normalized;
            transform.position = Vector3.MoveTowards(transform.position, new Vector3(transform.position.x, target.y, transform.position.z), speed * Time.deltaTime);
            yield return null;
        }

        // Move horizontally to target
        while (Vector3.Distance(new Vector3(target.x, transform.position.y, target.z), transform.position) > 0.1f)
        {
            Vector3 horizontalDirection = (new Vector3(target.x, transform.position.y, target.z) - transform.position).normalized;
            transform.position = Vector3.MoveTowards(transform.position, new Vector3(target.x, transform.position.y, target.z), speed * Time.deltaTime);
            yield return null;
        }

        rb.velocity = Vector3.zero; // Stop any remaining velocity
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
        else if (other.gameObject.CompareTag("Tool"))
        {
            PickUpTool();
            Destroy(other.gameObject); // Destroy the tool object
        }
        else if (other.gameObject.CompareTag("Portal"))
        {
            Debug.Log("Portal collided with!");
            StartCoroutine(HandlePortalCollision());
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
        if (collision.gameObject.CompareTag("Obstacle"))
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
        else if (collision.gameObject.CompareTag("Enemy"))
        {
            TakeDamage(); // Player3 takes damage on collision with an Enemy
        }
    }

    public void TakeDamage()
    {
        currentHealth--;
        Debug.Log("Player Health: " + currentHealth);
        UpdateHealthBar();
        if (currentHealth <= 0)
        {
            Die();
        }
    }

    void GainHealth()
    {
        currentHealth = Mathf.Min(currentHealth + 1, maxHealth); 
        UpdateHealthBar();
        Debug.Log("Player Health: " + currentHealth);
    }

    void UpdateHealthBar()
    {
        // Clamp currentHealth between 0 and the maximum index of the healthSprites array
        currentHealth = Mathf.Clamp(currentHealth, 0, healthSprites.Length - 1);
        // Update the health image to the corresponding sprite based on current health
        healthImage.sprite = healthSprites[currentHealth]; 
        Debug.Log("Updating health bar");
    }

    void PickUpTool()
    {
        hasTool = true;
        Debug.Log("Tool picked up.");
    }
    void Die()
    {
        animator.SetTrigger("Sit");
        Debug.Log("Player is dead. Game Over.");
        PlaySound(deathSound);
        this.enabled = false;
        Destroy(gameObject, 1f); // Destroy the player object after 1 second
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
    void PlaySound(AudioClip clip)
     { 
        if (clip != null && audioSource != null)
         { 
            audioSource.PlayOneShot(clip); 
            } 
     }
     void ShootLaserBeam()
{
    if (laserBeamPrefab != null && laserBeamSpawnPoint != null)
    {
        GameObject laserBeam = Instantiate(laserBeamPrefab, laserBeamSpawnPoint.position, laserBeamSpawnPoint.rotation);
        Rigidbody rb = laserBeam.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.velocity = laserBeamSpawnPoint.forward * 20f; // Adjust the speed of the laser beam
        }
        Destroy(laserBeam, 2f); // Destroy the laser beam after 2 seconds to clean up
    }
}



    IEnumerator FlinchAndInvincibility()
    {
        isInvincible = true;
        float flinchDuration = 0.5f; // Duration of flinch
        float flinchStartTime = Time.time;
        Vector3 flinchDirection = -transform.forward * 10; // Increase the strength of the flinch

        Debug.Log("Flinch applied with direction: " + flinchDirection);

        while (Time.time < flinchStartTime + flinchDuration)
        {
            rb.AddForce(flinchDirection * Time.deltaTime, ForceMode.VelocityChange); // Apply continuous force
            yield return null; // Wait for the next frame
        }

        yield return new WaitForSeconds(1f); // Duration of invincibility
        isInvincible = false;
    }
}


