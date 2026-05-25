using System;
using System.Collections.Generic;

// In-memory fog-of-war discovery store, keyed by scene name.
//
// This is static on purpose: static state lives for the whole play
// session and survives scene reloads/teleports even when no
// SaveManager exists (e.g. running a level scene directly in the
// editor without going through the main menu). SaveManager syncs this
// to/from disk at its save points so discovery also persists across
// sessions for the Continue flow. New Game clears it.
public static class FogStore
{
    private static readonly Dictionary<string, byte[]> data = new Dictionary<string, byte[]>();

    public static byte[] Get(string sceneName)
    {
        if (string.IsNullOrEmpty(sceneName)) return null;
        return data.TryGetValue(sceneName, out byte[] bytes) ? bytes : null;
    }

    public static void Set(string sceneName, byte[] alphas)
    {
        if (string.IsNullOrEmpty(sceneName) || alphas == null) return;
        data[sceneName] = alphas;
    }

    public static void Clear()
    {
        data.Clear();
    }

    // Replace the in-memory store with fog loaded from a save file.
    public static void ImportFrom(List<SceneFogData> saved)
    {
        data.Clear();
        if (saved == null) return;
        foreach (SceneFogData entry in saved)
        {
            if (entry == null || string.IsNullOrEmpty(entry.sceneName) || string.IsNullOrEmpty(entry.fogBase64))
                continue;
            data[entry.sceneName] = Convert.FromBase64String(entry.fogBase64);
        }
    }

    // Snapshot the in-memory store for serialization into a save file.
    public static List<SceneFogData> ExportTo()
    {
        List<SceneFogData> list = new List<SceneFogData>(data.Count);
        foreach (KeyValuePair<string, byte[]> kv in data)
        {
            list.Add(new SceneFogData
            {
                sceneName = kv.Key,
                fogBase64 = Convert.ToBase64String(kv.Value)
            });
        }
        return list;
    }
}
