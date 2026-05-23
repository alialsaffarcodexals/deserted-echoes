// ─────────────────────────────────────────────────────────────
// PortalSpawnManager.cs
// Deserted Echoes | IT8101 Games Development | Group 3
// Author: Ali Husain Ali Alsaffar
// Sprint: 5 | Created: May 22, 2026
// Description: Persistent singleton that remembers where the player
//              exited each scene via a portal. When that scene is
//              loaded again the player is repositioned to the saved
//              exit point, so returning through a portal always
//              lands the player back at the gate they came from.
// ─────────────────────────────────────────────────────────────

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PortalSpawnManager : MonoBehaviour
{
    public static PortalSpawnManager Instance { get; private set; }

    private readonly Dictionary<string, Vector2> returnPositions = new Dictionary<string, Vector2>();

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void AutoBootstrap()
    {
        if (Instance != null) return;
        GameObject go = new GameObject("PortalSpawnManager");
        go.AddComponent<PortalSpawnManager>();
    }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    /// <summary>
    /// Called by PortalTrigger before leaving a scene.
    /// Stores the player's exit position so they can return to it later.
    /// </summary>
    public void SetReturnPosition(string sceneName, Vector2 position)
    {
        returnPositions[sceneName] = position;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (!returnPositions.TryGetValue(scene.name, out Vector2 pos)) return;
        StartCoroutine(ApplySpawnNextFrame(pos));
    }

    // Waits one frame so the player GameObject is fully initialized before moving it.
    private IEnumerator ApplySpawnNextFrame(Vector2 pos)
    {
        yield return null;
        GameObject player = GameObject.FindWithTag("Player");
        if (player != null)
            player.transform.position = pos;
    }
}
