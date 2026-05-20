// ─────────────────────────────────────────────────────────────
// GameDifficulty.cs
// Deserted Echoes | IT8101 Games Development | Group 3
// Description: Global difficulty selection (Easy / Normal / Hard).
//              Saved via PlayerPrefs and read by enemies on level load.
// ─────────────────────────────────────────────────────────────

using UnityEngine;

public enum GameDifficulty
{
    Easy = 0,
    Normal = 1,
    Hard = 2
}

public static class GameDifficultySettings
{
    private const string Key = "GameDifficulty";

    public static GameDifficulty Current { get; private set; } = GameDifficulty.Normal;

    public static void Load()
    {
        Current = (GameDifficulty)PlayerPrefs.GetInt(Key, (int)GameDifficulty.Normal);
    }

    public static void Set(GameDifficulty difficulty)
    {
        Current = difficulty;
        PlayerPrefs.SetInt(Key, (int)difficulty);
        PlayerPrefs.Save();
    }

    /// <summary>Maps to enemy prefab suffix: 1 = Easy, 2 = Normal, 3 = Hard.</summary>
    public static int VariantIndex => (int)Current + 1;
}
