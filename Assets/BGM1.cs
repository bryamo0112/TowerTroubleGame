using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class BGM1 : MonoBehaviour
{
    public static BGM1 instance;
    public AudioSource musicSource;
    public AudioClip backgroundMusic;

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void Start()
    {
        PlayBackgroundMusic(backgroundMusic);
    }

    void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Logic to change music based on the scene, if necessary
        // Play different music for different scenes
        AudioClip newMusic = GetSceneMusic(scene.name);
        ChangeMusic(newMusic);
    }

    AudioClip GetSceneMusic(string sceneName)
    {
        // Return the appropriate AudioClip based on the scene name
        switch (sceneName)
        {
            case "Scene1":
                return Resources.Load<AudioClip>("Scene1Music");
            case "Scene2":
                return Resources.Load<AudioClip>("Scene2Music");
            case "Scene3":
                return Resources.Load<AudioClip>("Scene3Music");
            case "Scene4":
                return Resources.Load<AudioClip>("Scene4Music");
            // Add cases for other scenes as needed
            default:
                return backgroundMusic; // Default background music
        }
    }

    public void PlayBackgroundMusic(AudioClip newMusic)
    {
        if (musicSource != null && newMusic != null)
        {
            if (musicSource.clip != newMusic)
            {
                musicSource.clip = newMusic;
                musicSource.loop = true; // Loop the music
                musicSource.Play();
            }
        }
    }

    public void ChangeMusic(AudioClip newMusic)
    {
        PlayBackgroundMusic(newMusic);
    }
}



