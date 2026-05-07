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

public class PortalTrigger : MonoBehaviour
{
    [Header("Portal Settings")]
    [Tooltip("Exact name of the scene to load (must match Build Settings)")]
    [SerializeField] private string targetSceneName;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        EnterPortal();
    }

    private void EnterPortal()
    {
        if (string.IsNullOrEmpty(targetSceneName))
        {
            Debug.LogWarning("PortalTrigger: No target scene name set on " + gameObject.name);
            return;
        }

        Time.timeScale = 1f;
        SceneLoader.LoadScene(targetSceneName);
    }
}
