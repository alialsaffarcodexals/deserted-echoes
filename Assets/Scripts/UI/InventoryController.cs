using UnityEngine;
using UnityEngine.InputSystem;

public class InventoryController : MonoBehaviour
{
    [Header("Panels & Prefabs")]
    public GameObject inventoryPanel;
    public GameObject slotPrefab;
    [Tooltip("Controls exactly how many slots are generated inside the main inventory panel.")]
    public int inventorySlotCount = 16;
    public GameObject[] itemPrefabs;

    private Slot[] staticHotbarSlots;

    void Start()
    {
        if (inventoryPanel == null || slotPrefab == null)
        {
            Debug.LogError("InventoryController: Missing references in the Inspector!");
            return;
        }

        inventoryPanel.SetActive(false); // Hide inventory panel at start

        // 1. Fetch the slots dynamically created by the HotBarController
        HotBarController hotbar = FindFirstObjectByType<HotBarController>();
        int hotbarCount = 0;
        if (hotbar != null)
        {
            staticHotbarSlots = hotbar.GetGeneratedHotbarSlots();
            hotbarCount = staticHotbarSlots.Length;
        }

        // Keep track of our current position in the itemPrefabs array globally
        int globalItemIndex = 0;

        // 2. PHASE 1: Populate existing Hotbar Slots with starting items
        if (staticHotbarSlots != null)
        {
            for (int i = 0; i < hotbarCount; i++)
            {
                if (globalItemIndex < itemPrefabs.Length && itemPrefabs[globalItemIndex] != null)
                {
                    SpawnItemInSlot(itemPrefabs[globalItemIndex], staticHotbarSlots[i]);
                }
                globalItemIndex++;
            }
        }

        // 3. PHASE 2: Generate Inventory Panel Slots and populate remaining items
        for (int i = 0; i < inventorySlotCount; i++)
        {
            GameObject newSlotObj = Instantiate(slotPrefab, inventoryPanel.transform, false);
            Slot slot = newSlotObj.GetComponent<Slot>();

            if (slot != null)
            {
                // Numbers inventory slots locally from 1 to inventorySlotCount
                slot.InitializeSlotNumber(i + 1);

                // If there are still items left in your array, spawn them here
                if (globalItemIndex < itemPrefabs.Length && itemPrefabs[globalItemIndex] != null)
                {
                    SpawnItemInSlot(itemPrefabs[globalItemIndex], slot);
                }
            }
            globalItemIndex++;
        }
    }

    // Helper method to handle UI scaling transformations safely
    private void SpawnItemInSlot(GameObject itemPrefab, Slot targetSlot)
    {
        GameObject item = Instantiate(itemPrefab, targetSlot.transform, false);

        RectTransform itemRect = item.GetComponent<RectTransform>();
        if (itemRect != null)
        {
            itemRect.anchorMin = Vector2.zero;
            itemRect.anchorMax = Vector2.one;
            itemRect.sizeDelta = Vector2.zero;
            itemRect.anchoredPosition = Vector2.zero;
            itemRect.localScale = Vector3.one;
        }

        targetSlot.currentItem = item;
    }

    void Update()
    {
        if (Keyboard.current != null && Keyboard.current.tabKey.wasPressedThisFrame)
        {
            inventoryPanel.SetActive(!inventoryPanel.activeSelf);
        }
    }
}