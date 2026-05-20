// ------------------------------------------------------------
// SwordCollectible.cs
// Deserted Echoes | IT8101 Games Development | Group 3
// Author: Khizar Azhar
// Sprint: 7 | Created: 20/05/2026
// Description: Handles sword pickup and equips weapon damage without stacking
// ------------------------------------------------------------

using UnityEngine;

public class SwordCollectible : MonoBehaviour, ICollectible
{
    [Header("Sword Settings")]
    [SerializeField] private int bonusDamage = 10;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        Collect(other.gameObject);
    }

    public void Collect(GameObject collector)
    {
        PlayerController playerController = collector.GetComponent<PlayerController>();

        if (playerController == null)
        {
            Debug.LogWarning("SwordCollectible: PlayerController not found on player.");
            return;
        }

        playerController.EquipWeapon(bonusDamage);

        Debug.Log($"SwordCollectible: Equipped sword with +{bonusDamage} damage.");

        Destroy(gameObject);
    }
}