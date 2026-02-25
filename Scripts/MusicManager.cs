using UnityEngine;
using UnityEngine.SceneManagement;

public class MusicManager : MonoBehaviour
{
    private static MusicManager instance;
    private AudioSource audioSource;

    [Tooltip("Szenen, in denen keine Musik laufen soll")]
    public string[] silentScenes;

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);

            audioSource = GetComponent<AudioSource>();
            audioSource.loop = true;
            audioSource.Play();

            SceneManager.sceneLoaded += OnSceneLoaded;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        foreach (string name in silentScenes)
        {
            if (scene.name == name)
            {
                audioSource.Pause(); // Musik stoppen in dieser Szene
                return;
            }
        }
        if (!audioSource.isPlaying)
            audioSource.UnPause(); // Musik weiterlaufen lassen
    }

    void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }
}
