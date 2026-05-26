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

    // Dialogue
    public List<string> seenDialogueKeys;

    // Fog of war discovery state — persists across scene reloads; cleared on New Game
    public List<SceneFogData> sceneFogData;

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
        seenDialogueKeys = new List<string>();
        sceneFogData = new List<SceneFogData>();
    }
}
