// ------------------------------------------------------------
// InventoryItemPickup.cs
// Deserted Echoes | IT8101 Games Development | Group 3
// Author: Khizar Azhar
// Sprint: 8 | Created: 25/05/2026
// Description: Adds dropped world items into player inventory instead of consuming them immediately
// ------------------------------------------------------------

using UnityEngine;

public class InventoryItemPickup : MonoBehaviour
{
    [Header("Inventory Settings")]
    [SerializeField] private GameObject inventoryItemPrefab;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        InventoryController inventory = FindFirstObjectByType<InventoryController>();

        if (inventory == null)
        {
            Debug.LogWarning("InventoryItemPickup: InventoryController not found.");
            return;
        }

        if (inventory.AddItem(inventoryItemPrefab))
        {
            Debug.Log("InventoryItemPickup: Item added to inventory.");
            Destroy(gameObject);
        }
    }
}