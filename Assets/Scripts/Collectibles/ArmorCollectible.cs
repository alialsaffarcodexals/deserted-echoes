// ------------------------------------------------------------
// ArmorCollectible.cs
// Deserted Echoes | IT8101 Games Development | Group 3
// Author: Khizar Azhar
// Sprint: 7 | Created: 20/05/2026
// Description: Handles armor pickup and reduces incoming player damage
// ------------------------------------------------------------

using UnityEngine;

public class ArmorCollectible : MonoBehaviour, ICollectible
{
    [Header("Armor Settings")]
    [SerializeField] private int defenseBonus = 5;

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
            Debug.LogWarning("ArmorCollectible: PlayerController not found on player.");
            return;
        }

        playerController.EquipArmor(defenseBonus);

        Debug.Log($"ArmorCollectible: Equipped armor with {defenseBonus} defense.");

        Destroy(gameObject);
    }
}