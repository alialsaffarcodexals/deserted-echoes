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
}
