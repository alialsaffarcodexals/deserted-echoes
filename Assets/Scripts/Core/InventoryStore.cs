using System.Collections.Generic;
using UnityEngine;

// Session-memory inventory store — survives scene reloads exactly like FogStore.
// Cleared on New Game; restored automatically by InventoryController.Start().
public static class InventoryStore
{
    private static readonly Dictionary<string, GameObject> registry = new Dictionary<string, GameObject>();

    private static string[] hotbarData;
    private static string[] inventoryData;

    public static bool IsInitialized => hotbarData != null;

    // Register a prefab so it can be looked up by name when restoring.
    public static void Register(GameObject prefab)
    {
        if (prefab == null) return;
        string key = CleanName(prefab.name);
        if (!string.IsNullOrEmpty(key) && !registry.ContainsKey(key))
            registry[key] = prefab;
    }

    public static void Save(Slot[] hotbar, Slot[] inventory)
    {
        hotbarData    = SlotsToNames(hotbar);
        inventoryData = SlotsToNames(inventory);
    }

    public static string[] GetHotbarData()    => hotbarData;
    public static string[] GetInventoryData() => inventoryData;

    public static GameObject FindPrefab(string name)
    {
        if (string.IsNullOrEmpty(name)) return null;
        registry.TryGetValue(name, out GameObject prefab);
        return prefab;
    }

    public static void Clear()
    {
        hotbarData    = null;
        inventoryData = null;
    }

    // Strip "(Clone)" suffix Unity appends to instantiated objects.
    public static string CleanName(string name)
    {
        if (name == null) return null;
        int idx = name.IndexOf("(Clone)");
        return idx >= 0 ? name.Substring(0, idx).Trim() : name.Trim();
    }

    private static string[] SlotsToNames(Slot[] slots)
    {
        if (slots == null) return new string[0];
        string[] names = new string[slots.Length];
        for (int i = 0; i < slots.Length; i++)
        {
            names[i] = (slots[i] != null && slots[i].currentItem != null)
                ? CleanName(slots[i].currentItem.name)
                : null;
        }
        return names;
    }
}
