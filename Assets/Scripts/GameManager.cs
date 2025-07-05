using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;
    public static int playersAtPortal = 0;
    public bool isRestarting = false;
    private bool isTransitioning = false;

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
            Debug.Log("GameManager instance initialized");
        }
        else
        {
            Debug.LogWarning("Destroying duplicate GameManager instance");
            Destroy(gameObject);
        }
    }

    void Update()
    {
        // For testing: Transition to the next scene with key press 9
        if (Input.GetKeyDown(KeyCode.Alpha9) && !isTransitioning)
        {
            LoadNextScene();
        }
        // For testing: Go back to the previous scene with key press 0
        if (Input.GetKeyDown(KeyCode.Alpha0) && !isTransitioning)
        {
            LoadPreviousScene();
        }
    }

    public void RestartLevel()
    {
        if (isRestarting || isTransitioning) return; // Check for ongoing transitions
        isRestarting = true;
        isTransitioning = true;
        Debug.Log("RestartLevel called");
        StartCoroutine(RestartLevelRoutine());
    }

    IEnumerator RestartLevelRoutine()
    {
        Debug.Log("Entering RestartLevelRoutine");

        // Add a slight delay to ensure all destruction events are processed
        yield return new WaitForSeconds(0.1f);

        Debug.Log("Waited 0.1 seconds, now proceeding to restart");

        Debug.Log("Current scene before reload: " + SceneManager.GetActiveScene().name);

        Debug.Log("Restarting level...");

        SceneManager.LoadScene(SceneManager.GetActiveScene().name);

        // Ensure the flag is reset after the scene reloads
        yield return null; // Wait for the scene to load
        isRestarting = false;
        isTransitioning = false;
        playersAtPortal = 0; // Reset playersAtPortal after reload
        Debug.Log("Scene reloaded. isRestarting flag reset");
    }

    public void LoadNextScene()
    {
        if (isTransitioning) return; // Check for ongoing transitions
        Debug.Log("Loading the next scene...");
        StartCoroutine(LoadNextSceneRoutine());
    }

    IEnumerator LoadNextSceneRoutine()
    {
        isTransitioning = true;
        if (playersAtPortal == 3 || playersAtPortal == 4 || playersAtPortal == 5) 
        {
            Debug.Log("All players have reached the portal!");
            yield return new WaitForSeconds(0.5f); // Wait for the message to be displayed
            int currentSceneIndex = SceneManager.GetActiveScene().buildIndex;
            int nextSceneIndex = currentSceneIndex + 1;
            if (nextSceneIndex < SceneManager.sceneCountInBuildSettings)
            {
                SceneManager.LoadScene(nextSceneIndex); // Load the next scene in the build
            }
            else
            {
                Debug.Log("No more scenes in build settings.");
                // Handle end of game or loop back to the start
            }
            playersAtPortal = 0; // Reset playersAtPortal for the next scene
        }
        yield return new WaitForSeconds(1f);
        isTransitioning = false; // Reset the transition flag after completion
    }

    public void LoadPreviousScene()
    {
        if (isTransitioning) return; // Check for ongoing transitions
        Debug.Log("Loading the previous scene...");
        StartCoroutine(LoadPreviousSceneRoutine());
    }

    IEnumerator LoadPreviousSceneRoutine()
    {
        isTransitioning = true;
        int currentSceneIndex = SceneManager.GetActiveScene().buildIndex;
        int previousSceneIndex = currentSceneIndex - 1;
        if (previousSceneIndex >= 0)
        {
            SceneManager.LoadScene(previousSceneIndex); // Load the previous scene in the build
        }
        else
        {
            Debug.Log("No previous scenes in build settings.");
        }
        yield return new WaitForSeconds(1f);
        isTransitioning = false; // Reset the transition flag after completion
        playersAtPortal = 0; // Reset playersAtPortal after changing scenes
    }
}












