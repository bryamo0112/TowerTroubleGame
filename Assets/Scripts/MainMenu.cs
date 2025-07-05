using UnityEngine;
using UnityEngine.SceneManagement;

public class StartScreenManager : MonoBehaviour
{
    public void PlayGame()
    {
        SceneManager.LoadScene("Scene1");
    }

    public void ShowControls()
    {
        SceneManager.LoadScene("Control");
    }

    public void ShowBossBattle()
    {
        SceneManager.LoadScene("Scene4");
    }

    public void MainMenu()
    {
        SceneManager.LoadScene("StartingScreen");
    }

    public void QuitGame()
    {
        Application.Quit();
        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #endif
    }
}
