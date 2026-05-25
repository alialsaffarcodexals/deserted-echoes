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

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PortalSpawnManager : MonoBehaviour
{
    public static PortalSpawnManager Instance { get; private set; }

    private readonly Dictionary<string, Vector2> returnPositions = new Dictionary<string, Vector2>();

    // Set by a PortalTrigger when the player picks a destination on the panel
    // before teleporting to Open-World; consumed on the next Open-World load.
    // null = no pending choice, true = Central_Teleport, false = normal (last position).
    private bool? pendingOpenWorldUseCentral = null;

    // Fallback central position used only if the Central_Teleport object is missing.
    private static readonly Vector2 DefaultCentralPosition = new Vector2(-14.98f, 35.38f);

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

    /// <summary>
    /// Records the player's pre-teleport destination choice for Open-World, made on
    /// the selection panel in the source scene. true = spawn at Central_Teleport,
    /// false = spawn at the player's last Open-World position. Applied on the next
    /// Open-World load.
    /// </summary>
    public void SetOpenWorldSpawnChoice(bool useCentral)
    {
        pendingOpenWorldUseCentral = useCentral;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        bool hasReturn = returnPositions.TryGetValue(scene.name, out Vector2 pos);

        // Entering Open-World with a destination already chosen on the panel.
        if (scene.name == "Open-World" && pendingOpenWorldUseCentral.HasValue)
        {
            bool useCentral = pendingOpenWorldUseCentral.Value;
            pendingOpenWorldUseCentral = null;

            if (useCentral)
            {
                GameObject centralObj = GameObject.Find("Central_Teleport");
                Vector2 centralPos = centralObj != null
                    ? (Vector2)centralObj.transform.position
                    : DefaultCentralPosition;
                ApplySpawn(centralPos);
            }
            else if (hasReturn)
            {
                // Normal teleport: drop the player at their last Open-World position.
                ApplySpawn(pos);
            }
            return;
        }

        // Returning to any other scene: reposition immediately to avoid a
        // 1-frame jitter or camera damping slide.
        if (hasReturn) ApplySpawn(pos);
    }

    // Locates the selection UI even when its GameObject is inactive (the panel
    // starts disabled), so callers can activate and drive it.
    public static TeleportSelectionUI FindTeleportSelectionUI()
    {
        if (TeleportSelectionUI.Instance != null) return TeleportSelectionUI.Instance;

        var found = Object.FindObjectsByType<TeleportSelectionUI>(
            FindObjectsInactive.Include, FindObjectsSortMode.None);
        return found.Length > 0 ? found[0] : null;
    }

    private void ApplySpawn(Vector2 pos)
    {
        GameObject player = GameObject.FindWithTag("Player");
        if (player == null) return;

        player.transform.position = pos;

        // Instantly snap CameraFollow2D so it doesn't slide from the old position.
        CameraFollow2D cam = Object.FindAnyObjectByType<CameraFollow2D>();
        if (cam != null)
            cam.Warp(pos);
    }
}
