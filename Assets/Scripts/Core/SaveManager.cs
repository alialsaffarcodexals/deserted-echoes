using UnityEngine;
using System;
using System.IO;
using System.Collections.Generic;

public class SaveManager : MonoBehaviour
{
    public static SaveManager Instance { get; private set; }
    
    private string savePath;
    private const string SAVE_FILE_NAME = "gamesave.json";
    
    public SaveData CurrentSaveData { get; private set; }
    public string SaveFilePath => GetSavePath();

    public bool HasSaveFile()
    {
        return File.Exists(GetSavePath());
    }

    public void CreateNewGame(string startingSceneName)
    {
        CurrentSaveData = new SaveData();
        CurrentSaveData.lastSceneName = startingSceneName;
        CurrentSaveData.sceneFogData = new List<SceneFogData>();
        SaveGame();
        Debug.Log("New game save created at: " + GetSavePath());
    }

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
        string path = GetSavePath();
        if (!File.Exists(path))
        {
            Debug.LogWarning("Save file not found at: " + path);
            CurrentSaveData = new SaveData();
            return;
        }

        try
        {
            string json = File.ReadAllText(path);
            CurrentSaveData = JsonUtility.FromJson<SaveData>(json);
            NormalizeSaveData();
            Debug.Log("Game loaded successfully from: " + path);
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
            string path = GetSavePath();
            string directory = Path.GetDirectoryName(path);
            if (!string.IsNullOrWhiteSpace(directory))
                Directory.CreateDirectory(directory);

            CaptureActivePlayerData();
            CurrentSaveData.saveTimestamp = System.DateTime.Now.Ticks;
            string json = JsonUtility.ToJson(CurrentSaveData, true);
            File.WriteAllText(path, json);
            Debug.Log("Game saved successfully at: " + path);
        }
        catch (System.Exception e)
        {
            Debug.LogError("Failed to save game: " + e.Message);
        }
    }

    /// <summary>
    /// Updates player stats in the save data.
    /// </summary>
    public void UpdatePlayerStats(int health, int maxHealth, int level, int experience, int attackDamage, Vector2 position)
    {
        CurrentSaveData.currentHealth = health;
        CurrentSaveData.maxHealth = maxHealth;
        CurrentSaveData.level = level;
        CurrentSaveData.experience = experience;
        CurrentSaveData.attackDamage = attackDamage;
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

    public bool HasSeenDialogue(string key)
    {
        if (string.IsNullOrEmpty(key))
            return false;

        NormalizeSaveData();
        return CurrentSaveData.seenDialogueKeys.Contains(key);
    }

    public void MarkDialogueSeen(string key)
    {
        if (string.IsNullOrEmpty(key))
            return;

        NormalizeSaveData();
        if (CurrentSaveData.seenDialogueKeys.Contains(key))
            return;

        CurrentSaveData.seenDialogueKeys.Add(key);
    }

    /// <summary>
    /// Gets the saved player position.
    /// </summary>
    public Vector2 GetPlayerPosition()
    {
        return new Vector2(CurrentSaveData.playerPositionX, CurrentSaveData.playerPositionY);
    }

    /// <summary>
    /// Prepares the save data for a retry after the player dies:
    /// restores HP to max and clears the saved position / scene so
    /// the level reloads from its authored spawn point instead of
    /// the death location.
    /// </summary>
    public void ResetForRetry()
    {
        if (CurrentSaveData == null)
            CurrentSaveData = new SaveData();

        // Restore HP so the player isn't loaded back in dead.
        if (CurrentSaveData.maxHealth <= 0)
            CurrentSaveData.maxHealth = 100;
        CurrentSaveData.currentHealth = CurrentSaveData.maxHealth;

        // Clear saved position; PlayerController.LoadFromSave only
        // applies the position when lastSceneName == current scene.
        CurrentSaveData.lastSceneName = string.Empty;
        CurrentSaveData.playerPositionX = 0f;
        CurrentSaveData.playerPositionY = 0f;
    }

    /// <summary>
    /// Deletes the save file (for new game+).
    /// </summary>
    public void DeleteSave()
    {
        string path = GetSavePath();
        if (File.Exists(path))
        {
            File.Delete(path);
            CurrentSaveData = new SaveData();
            Debug.Log("Save file deleted at: " + path);
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

    private void CaptureActivePlayerData()
    {
        GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
        if (playerObject == null)
            return;

        PlayerController playerController = playerObject.GetComponent<PlayerController>();
        if (playerController != null)
            playerController.SavePlayerData();
    }

    public byte[] GetFogAlphas(string sceneName)
    {
        if (CurrentSaveData?.sceneFogData == null) return null;
        SceneFogData entry = CurrentSaveData.sceneFogData.Find(d => d.sceneName == sceneName);
        if (entry == null || string.IsNullOrEmpty(entry.fogBase64)) return null;
        return Convert.FromBase64String(entry.fogBase64);
    }

    public void SetFogAlphas(string sceneName, byte[] alphas)
    {
        if (CurrentSaveData == null) return;
        if (CurrentSaveData.sceneFogData == null)
            CurrentSaveData.sceneFogData = new List<SceneFogData>();
        SceneFogData entry = CurrentSaveData.sceneFogData.Find(d => d.sceneName == sceneName);
        if (entry == null)
        {
            entry = new SceneFogData { sceneName = sceneName };
            CurrentSaveData.sceneFogData.Add(entry);
        }
        entry.fogBase64 = Convert.ToBase64String(alphas);
        SaveGame();
    }

    private void NormalizeSaveData()
    {
        if (CurrentSaveData == null)
            CurrentSaveData = new SaveData();

        if (CurrentSaveData.seenDialogueKeys == null)
            CurrentSaveData.seenDialogueKeys = new List<string>();

        if (CurrentSaveData.sceneFogData == null)
            CurrentSaveData.sceneFogData = new List<SceneFogData>();
    }

    private string GetSavePath()
    {
        if (string.IsNullOrWhiteSpace(savePath))
            savePath = Path.Combine(Application.persistentDataPath, SAVE_FILE_NAME);

        return savePath;
    }
}
