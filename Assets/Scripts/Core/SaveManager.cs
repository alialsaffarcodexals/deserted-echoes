using UnityEngine;
using System.IO;

public class SaveManager : MonoBehaviour
{
    public static SaveManager Instance { get; private set; }
    
    private string savePath;
    private const string SAVE_FILE_NAME = "gamesave.json";
    
    public SaveData CurrentSaveData { get; private set; }

    private void Awake()
    {
        // Singleton pattern
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        savePath = Path.Combine(Application.persistentDataPath, SAVE_FILE_NAME);
        CurrentSaveData = new SaveData();
        
        Debug.Log("SaveManager initialized. Save path: " + savePath);
    }

    /// <summary>
    /// Loads the game save from disk.
    /// </summary>
    public void LoadGame()
    {
        if (!File.Exists(savePath))
        {
            Debug.LogWarning("Save file not found. Creating new game...");
            CurrentSaveData = new SaveData();
            return;
        }

        try
        {
            string json = File.ReadAllText(savePath);
            CurrentSaveData = JsonUtility.FromJson<SaveData>(json);
            Debug.Log("Game loaded successfully!");
        }
        catch (System.Exception e)
        {
            Debug.LogError("Failed to load game: " + e.Message);
            CurrentSaveData = new SaveData();
        }
    }

    /// <summary>
    /// Saves the game to disk.
    /// </summary>
    public void SaveGame()
    {
        if (CurrentSaveData == null)
        {
            Debug.LogError("No save data to save!");
            return;
        }

        try
        {
            CurrentSaveData.saveTimestamp = System.DateTime.Now.Ticks;
            string json = JsonUtility.ToJson(CurrentSaveData, true);
            File.WriteAllText(savePath, json);
            Debug.Log("Game saved successfully!");
        }
        catch (System.Exception e)
        {
            Debug.LogError("Failed to save game: " + e.Message);
        }
    }

    /// <summary>
    /// Updates player stats in the save data.
    /// </summary>
    public void UpdatePlayerStats(int health, int maxHealth, int level, int experience, Vector2 position)
    {
        CurrentSaveData.currentHealth = health;
        CurrentSaveData.maxHealth = maxHealth;
        CurrentSaveData.level = level;
        CurrentSaveData.experience = experience;
        CurrentSaveData.playerPositionX = position.x;
        CurrentSaveData.playerPositionY = position.y;
    }

    /// <summary>
    /// Updates the last scene name.
    /// </summary>
    public void UpdateSceneName(string sceneName)
    {
        CurrentSaveData.lastSceneName = sceneName;
    }

    /// <summary>
    /// Gets the saved player position.
    /// </summary>
    public Vector2 GetPlayerPosition()
    {
        return new Vector2(CurrentSaveData.playerPositionX, CurrentSaveData.playerPositionY);
    }

    /// <summary>
    /// Deletes the save file (for new game+).
    /// </summary>
    public void DeleteSave()
    {
        if (File.Exists(savePath))
        {
            File.Delete(savePath);
            CurrentSaveData = new SaveData();
            Debug.Log("Save file deleted.");
        }
    }

    /// <summary>
    /// Called when the application quits to save progress.
    /// </summary>
    private void OnApplicationQuit()
    {
        SaveGame();
    }

    /// <summary>
    /// Called when the application pauses.
    /// </summary>
    private void OnApplicationPause(bool pauseStatus)
    {
        if (pauseStatus)
        {
            SaveGame();
        }
    }
}
