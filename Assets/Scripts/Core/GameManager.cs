using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        // Ensure SaveManager exists
        if (SaveManager.Instance == null)
        {
            GameObject saveManagerObj = new GameObject("SaveManager");
            saveManagerObj.AddComponent<SaveManager>();
        }

        // Load the game when the game starts
        SaveManager.Instance.LoadGame();
        ApplySavedData();
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    /// <summary>
    /// Called when a scene is loaded.
    /// </summary>
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Debug.Log("Scene loaded: " + scene.name);
        SaveManager.Instance.UpdateSceneName(scene.name);
        ApplySavedData();
    }

    /// <summary>
    /// Applies saved data to the player when a scene loads.
    /// </summary>
    private void ApplySavedData()
    {
        GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
        if (playerObject == null)
            return;

        PlayerController playerController = playerObject.GetComponent<PlayerController>();
        if (playerController != null)
        {
            playerController.LoadFromSave(SaveManager.Instance.CurrentSaveData);
        }
    }

    /// <summary>
    /// Call this when you want to load a scene.
    /// </summary>
    public void LoadScene(string sceneName)
    {
        // Save before changing scenes
        SaveManager.Instance.SaveGame();
        SceneManager.LoadScene(sceneName);
    }

    /// <summary>
    /// Saves the current game state.
    /// </summary>
    public void SaveGameState()
    {
        SaveManager.Instance.SaveGame();
        Debug.Log("Game state saved!");
    }
}
