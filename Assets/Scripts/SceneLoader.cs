// ─────────────────────────────────────────────────────────────
// SceneLoader.cs
// Deserted Echoes | IT8101 Games Development | Group 3
// Author: Ali Husain Ali Alsaffar (202301152)
// Sprint: 1 | Created: April 9, 2026
// Description: Static utility for loading scenes. All scene
//              transitions in the project go through this class.
//              Do NOT attach to a GameObject — call statically:
//              SceneLoader.LoadScene("main-menu");
//
//              NOTE: In Sprint 3, a fade/loading-screen transition
//              will be added here. All callers will get it for free.
// ─────────────────────────────────────────────────────────────

using UnityEngine;
using UnityEngine.SceneManagement;

public static class SceneLoader
{
    /// <summary>Load a scene by name. Use kebab-case names per CONVENTIONS.md.</summary>
    public static void LoadScene(string sceneName)
    {
        Debug.Log($"SceneLoader: Loading scene '{sceneName}'");
        // Sprint 3: add fade-out coroutine here before loading
        SceneManager.LoadScene(sceneName);
    }

    /// <summary>Reload the currently active scene.</summary>
    public static void ReloadCurrentScene()
    {
        string current = SceneManager.GetActiveScene().name;
        Debug.Log($"SceneLoader: Reloading scene '{current}'");
        SceneManager.LoadScene(current);
    }

    /// <summary>Load scene by build index (useful for sequential level progression).</summary>
    public static void LoadNextScene()
    {
        int next = SceneManager.GetActiveScene().buildIndex + 1;
        Debug.Log($"SceneLoader: Loading next scene (index {next})");
        SceneManager.LoadScene(next);
    }
}
