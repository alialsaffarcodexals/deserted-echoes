// ─────────────────────────────────────────────────────────────
// PortalTrigger.cs
// Deserted Echoes | IT8101 Games Development | Group 3
// Author: Ali Husain Ali Alsaffar
// Sprint: 4 | Created: May 7, 2026
// Description: Attach to any portal/gate GameObject that has a
//              Collider2D set as a Trigger. When the Player walks
//              into the trigger, loads the target scene.
//              Set the Target Scene Name in the Inspector.
// ─────────────────────────────────────────────────────────────

using UnityEngine;
using UnityEngine.SceneManagement;

public class PortalTrigger : MonoBehaviour
{
    [Header("Portal Settings")]
    [Tooltip("Exact name of the scene to load (must match Build Settings)")]
    [SerializeField] private string targetSceneName;

    [Header("Boss Lock")]
    [SerializeField] private bool requireBossDefeated;
    [SerializeField] private string requiredBossName = "Beholder1";

    private bool playerInRange = false;
    private bool hasTriggered   = false;
    private bool bossDefeated;
    private EnemyControllerBase requiredBoss;

    private void Start()
    {
        if (!requireBossDefeated)
            return;

        requiredBoss = FindRequiredBoss();
        if (requiredBoss != null)
        {
            requiredBoss.OnDefeated += HandleRequiredBossDefeated;
            return;
        }

        Debug.LogWarning($"PortalTrigger: Boss '{requiredBossName}' was not found. Portal '{name}' will stay locked.");
    }

    private void OnDestroy()
    {
        if (requiredBoss != null)
            requiredBoss.OnDefeated -= HandleRequiredBossDefeated;
    }

    private void Update()
    {
        if (playerInRange && !hasTriggered && Input.GetKeyDown(KeyCode.E))
            EnterPortal();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        playerInRange = true;
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        playerInRange = false;
    }

    private void EnterPortal()
    {
        if (requireBossDefeated && !bossDefeated)
        {
            Debug.Log($"PortalTrigger: Defeat '{requiredBossName}' before using {gameObject.name}.");
            return;
        }

        if (string.IsNullOrEmpty(targetSceneName))
        {
            Debug.LogWarning("PortalTrigger: No target scene name set on " + gameObject.name);
            return;
        }

        // Save the player's current position so returning to this scene
        // spawns them back at this portal instead of the default spawn point.
        GameObject player = GameObject.FindWithTag("Player");
        if (player != null && PortalSpawnManager.Instance != null)
            PortalSpawnManager.Instance.SetReturnPosition(
                SceneManager.GetActiveScene().name,
                player.transform.position
            );

        // Heading to Open-World: present the destination choice BEFORE teleporting.
        // The player picks here; the actual load happens only after a selection.
        if (targetSceneName == "Open-World" && PortalSpawnManager.Instance != null)
        {
            TeleportSelectionUI ui = PortalSpawnManager.FindTeleportSelectionUI();
            if (ui != null)
            {
                hasTriggered = true; // block re-trigger while the panel is open
                ui.gameObject.SetActive(true);
                ui.Show(
                    useCentral =>
                    {
                        PortalSpawnManager.Instance.SetOpenWorldSpawnChoice(useCentral);
                        Time.timeScale = 1f;
                        SceneLoader.LoadScene("Open-World");
                    },
                    () => hasTriggered = false); // closed without choosing — allow retry
                return;
            }
        }

        hasTriggered   = true;
        Time.timeScale = 1f;
        SceneLoader.LoadScene(targetSceneName);
    }

    private EnemyControllerBase FindRequiredBoss()
    {
        EnemyControllerBase[] enemies = FindObjectsByType<EnemyControllerBase>(FindObjectsSortMode.None);
        foreach (EnemyControllerBase enemy in enemies)
        {
            if (enemy != null && enemy.name == requiredBossName)
                return enemy;
        }

        foreach (EnemyControllerBase enemy in enemies)
        {
            if (enemy != null && enemy.name.Contains(requiredBossName))
                return enemy;
        }

        return null;
    }

    private void HandleRequiredBossDefeated(EnemyControllerBase defeated)
    {
        bossDefeated = true;

        if (requiredBoss != null)
            requiredBoss.OnDefeated -= HandleRequiredBossDefeated;
    }
}
