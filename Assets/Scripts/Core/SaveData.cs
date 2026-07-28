using System.Collections.Generic;

[System.Serializable]
public class SaveData
{
    // Player Stats
    public int currentHealth;
    public int maxHealth;
    public int level;
    public int experience;
    
    // Position
    public float playerPositionX;
    public float playerPositionY;
    
    // Additional stats for future expansion
    public int attackDamage;
    public float attackSpeed;
    public int coins;
    public int killedEnemies;
    
    // Scene Info
    public string lastSceneName;
    public long saveTimestamp;
    public bool hasSeenFriendsScene;

    // Dialogue
    public List<string> seenDialogueKeys;

    // Fog of war discovery state — persists across scene reloads; cleared on New Game
    public List<SceneFogData> sceneFogData;
    // change here
    public List<ChestSaveData> chestSaveData;
    // end here
    public SaveData()
    {
        // Default values
        currentHealth = 100;
        maxHealth = 100;
        level = 1;
        experience = 0;
        playerPositionX = 0;
        playerPositionY = 0;
        attackDamage = 25;
        attackSpeed = 0.5f;
        coins = 0;
        killedEnemies = 0;
        lastSceneName = "Open-World";
        saveTimestamp = System.DateTime.Now.Ticks;
        hasSeenFriendsScene = false;
        seenDialogueKeys = new List<string>();
        sceneFogData = new List<SceneFogData>();
        // change here
        chestSaveData = new List<ChestSaveData>();
        // end here
    }
}

[System.Serializable]
public class ChestSaveData
{
    public string chestID;

    // Legacy name-only list, kept so saves made before slot-based storage still load.
    public List<string> remainingItems = new List<string>();

    // Per-slot item names; empty string = slot is empty/taken. Slot-indexed so two
    // items with the same name (possible with loot table rolls) don't get confused.
    public List<string> slotItems = new List<string>();

    // True once this chest's contents have been rolled/recorded, so loot chests
    // never re-roll on a later visit.
    public bool initialized = false;
}
