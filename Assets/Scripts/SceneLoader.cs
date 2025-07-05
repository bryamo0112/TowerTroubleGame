using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoader : MonoBehaviour
{
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.C)) // Use T key to test
        {
            Debug.Log("C key pressed. Directly loading the scene.");
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
    }
}

