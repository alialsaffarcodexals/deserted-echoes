// ------------------------------------------------------------
// HealthPotionCollectible.cs
// Deserted Echoes | IT8101 Games Development | Group 3
// Author: Khizar Azhar
// Sprint: 7 | Created: 18/05/2026
// Description: Handles health potion pickups that restore a percentage of max health
// ------------------------------------------------------------

using UnityEngine;

public class HealthPotionCollectible : MonoBehaviour, ICollectible
{
    [Header("Potion Settings")]
    [SerializeField] private float healPercentage = 0.05f;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        Collect(other.gameObject);
    }

    public void Collect(GameObject collector)
    {
        SurvivalSystem survival = collector.GetComponent<SurvivalSystem>();

        if (survival == null)
        {
            Debug.LogWarning("HealthPotionCollectible: SurvivalSystem not found on player.");
            return;
        }

        float healAmount = survival.maxHealth * healPercentage;
        survival.Heal(healAmount);

        Debug.Log($"HealthPotionCollectible: Restored {healPercentage * 100}% health.");

        Destroy(gameObject);
    }
}