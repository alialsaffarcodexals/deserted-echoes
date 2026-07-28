using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Deserted Echoes/Loot Table", fileName = "NewLootTable")]
public class LootTable : ScriptableObject
{
    [System.Serializable]
    public class LootEntry
    {
        public GameObject itemPrefab;

        [Tooltip("Relative chance of this item being picked. Higher = more common. E.g. common=10, rare=1.")]
        [Min(0f)] public float weight = 1f;
    }

    [Header("Possible Items")]
    [SerializeField] private List<LootEntry> entries = new List<LootEntry>();

    [Header("Roll Settings")]
    [Tooltip("How many items a chest gets (random between min and max, inclusive).")]
    [Min(0)] [SerializeField] private int minItems = 2;
    [Min(0)] [SerializeField] private int maxItems = 4;

    [Tooltip("If off, the same item can only be rolled once per chest.")]
    [SerializeField] private bool allowDuplicates = true;

    // Rolls a fresh set of items for one chest. Call once per chest, then
    // persist the result — never re-roll a chest that already has a save entry.
    public GameObject[] RollLoot()
    {
        int count = Random.Range(minItems, maxItems + 1);
        List<GameObject> results = new List<GameObject>(count);

        // Work on a copy so no-duplicates mode can remove picked entries.
        List<LootEntry> pool = new List<LootEntry>();
        foreach (LootEntry entry in entries)
        {
            if (entry != null && entry.itemPrefab != null && entry.weight > 0f)
                pool.Add(entry);
        }

        for (int i = 0; i < count && pool.Count > 0; i++)
        {
            LootEntry picked = PickWeighted(pool);
            if (picked == null) break;

            results.Add(picked.itemPrefab);

            if (!allowDuplicates)
                pool.Remove(picked);
        }

        return results.ToArray();
    }

    private LootEntry PickWeighted(List<LootEntry> pool)
    {
        float totalWeight = 0f;
        foreach (LootEntry entry in pool)
            totalWeight += entry.weight;

        if (totalWeight <= 0f) return null;

        float roll = Random.Range(0f, totalWeight);
        foreach (LootEntry entry in pool)
        {
            roll -= entry.weight;
            if (roll <= 0f)
                return entry;
        }

        return pool[pool.Count - 1];
    }

    // Used when restoring a saved roll: maps a saved prefab name back to its prefab.
    public GameObject FindPrefabByName(string prefabName)
    {
        if (string.IsNullOrEmpty(prefabName)) return null;

        foreach (LootEntry entry in entries)
        {
            if (entry != null && entry.itemPrefab != null && entry.itemPrefab.name == prefabName)
                return entry.itemPrefab;
        }

        return null;
    }
}
