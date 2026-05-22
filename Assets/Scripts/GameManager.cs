// ─────────────────────────────────────────────────────────────
// GameManager.cs
// Deserted Echoes | IT8101 Games Development | Group 3
// Author: Ali Husain Ali Alsaffar (202301152)
// Sprint: 1 | Created: April 9, 2026
// Description: Persistent singleton that manages global game
//              state across all scenes. Attach to a GameObject
//              in the MainMenu scene — it will never be destroyed.
//              Access from any script via GameManager.Instance
// ─────────────────────────────────────────────────────────────
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    // ── Singleton ────────────────────────────────────────────
    public static GameManager Instance { get; private set; }

    [Header("Difficulty")]
    [SerializeField] private EnemyDifficultyApplier difficultyApplier;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        GameDifficultySettings.Load();

        if (difficultyApplier == null)
            difficultyApplier = GetComponent<EnemyDifficultyApplier>();
        if (difficultyApplier == null)
            difficultyApplier = gameObject.AddComponent<EnemyDifficultyApplier>();
    }

    // ── Game State ───────────────────────────────────────────
    [Header("Level")]
    public int currentLevel = 1;

    [Header("Player")]
    public int playerLives = 3;
    public int playerScore = 0;

    [Header("Game Flow")]
    public bool isGamePaused = false;

    // ── Methods ──────────────────────────────────────────────
    /// <summary>Pause the game: freeze time and set pause flag.</summary>
    public void PauseGame()
    {
        isGamePaused = true;
        Time.timeScale = 0f;
    }

    /// <summary>Resume the game: restore time and clear pause flag.</summary>
    public void ResumeGame()
    {
        isGamePaused = false;
        Time.timeScale = 1f;
    }

    /// <summary>Trigger game over overlay (or fallback if no UI in scene).</summary>
    public void GameOver()
    {
        Debug.Log("GameManager: Game Over triggered.");
        if (GameOverUI.Instance != null)
        {
            GameOverUI.Instance.Show();
            return;
        }
        Debug.LogWarning("GameManager: No GameOverUI in scene. Add GameOverCanvas prefab to the level.");
    }

    /// <summary>Add points to the player score.</summary>
    public void AddScore(int amount)
    {
        playerScore += amount;
        Debug.Log($"GameManager: Score updated → {playerScore}");
    }

    /// <summary>Reset game state for a new run.</summary>
    public void ResetGame()
    {
        currentLevel = 1;
        playerLives = 3;
        playerScore = 0;
        isGamePaused = false;
        Time.timeScale = 1f;
    }

    // ─────────────────────────────────────────────────────────
    // OTHER SCRIPT
    // ─────────────────────────────────────────────────────────

    private void Start()
    {
        if (SaveManager.Instance == null)
        {
            GameObject saveManagerObj = new GameObject("SaveManager");
            saveManagerObj.AddComponent<SaveManager>();
        }

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

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Debug.Log("Scene loaded: " + scene.name);
        SaveManager.Instance.UpdateSceneName(scene.name);
        ApplySavedData();
    }

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

    public void LoadScene(string sceneName)
    {
        SaveCurrentPlayerData();
        SaveManager.Instance.SaveGame();
        SceneManager.LoadScene(sceneName);
    }

    public void SaveGameState()
    {
        SaveCurrentPlayerData();
        SaveManager.Instance.SaveGame();
        Debug.Log("Game state saved!");
    }

    public void StartNewGame(string startingSceneName)
    {
        ResetGame();
        EnsureSaveManagerExists();
        SaveManager.Instance.CreateNewGame(startingSceneName);
        SceneLoader.LoadScene(startingSceneName);
    }

    public void LoadSavedGame(string fallbackSceneName)
    {
        EnsureSaveManagerExists();

        if (!SaveManager.Instance.HasSaveFile())
        {
            Debug.LogWarning("No save file found. Load game cancelled.");
            return;
        }

        SaveManager.Instance.LoadGame();

        string sceneToLoad = SaveManager.Instance.CurrentSaveData.lastSceneName;
        if (string.IsNullOrWhiteSpace(sceneToLoad))
            sceneToLoad = fallbackSceneName;

        Time.timeScale = 1f;
        isGamePaused = false;
        SceneLoader.LoadScene(sceneToLoad);
    }

    private void EnsureSaveManagerExists()
    {
        if (SaveManager.Instance != null)
            return;

        GameObject saveManagerObj = new GameObject("SaveManager");
        saveManagerObj.AddComponent<SaveManager>();
    }

    private void SaveCurrentPlayerData()
    {
        if (SaveManager.Instance == null)
            return;

        GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
        if (playerObject == null)
            return;

        PlayerController playerController = playerObject.GetComponent<PlayerController>();
        if (playerController != null)
            playerController.SavePlayerData();
    }
}
