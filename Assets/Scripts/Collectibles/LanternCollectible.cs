// ------------------------------------------------------------
// LanternCollectible.cs
// Deserted Echoes | IT8101 Games Development | Group 3
// Author: Khizar Azhar
// Sprint: 7 | Created: 20/05/2026
// Description: Handles lantern pickup and enables player light during night
//              Setup: Add a child object to the Player prefab named LanternLight.
//              Add a Light2D component to LanternLight, set Light Type to Spot,
//              Intensity to 0, Outer Radius to 4, and disable LanternLight by default.
// ------------------------------------------------------------

using UnityEngine;
using UnityEngine.Rendering.Universal;

public class LanternCollectible : MonoBehaviour, ICollectible
{
    [Header("Lantern Settings")]
    [SerializeField] private float lanternIntensity = 1.5f;
    [SerializeField] private float lanternRadius = 4f;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        Collect(other.gameObject);
    }

    public void Collect(GameObject collector)
    {
        Light2D playerLight = collector.GetComponentInChildren<Light2D>(true);

        if (playerLight == null)
        {
            Debug.LogWarning("LanternCollectible: No Light2D found on player.");
            return;
        }

        playerLight.intensity = lanternIntensity;
        playerLight.pointLightOuterRadius = lanternRadius;

        bool shouldEnableLantern = DayNightCycle.Instance == null || !DayNightCycle.Instance.IsDay;
        playerLight.gameObject.SetActive(shouldEnableLantern);

        Debug.Log("LanternCollectible: Lantern collected and player light enabled.");

        Destroy(gameObject);
    }
}