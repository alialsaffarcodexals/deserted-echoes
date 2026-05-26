using UnityEngine;

public class InventoryManagent : MonoBehaviour
{
    [Header("Item Blueprints")]
    [Tooltip("Drop your item UI prefabs here from the Project Assets window.")]
    [SerializeField] private GameObject[] itemPrefabs;

    void Start()
    {
        // Wait until the end of the frame or give Hotbar a moment to initialize slots
        Invoke(nameof(PopulateInitialInventory), 0.1f);
    }

    void PopulateInitialInventory()
    {
        // Find the hotbar in the scene
        HotBarController hotbar = FindFirstObjectByType<HotBarController>();
        if (hotbar == null) return;

        // Get the slots that the Hotbar generated automatically
        Slot[] slots = hotbar.GetGeneratedHotbarSlots();

        // Loop through and spawn items into the slots
        for (int i = 0; i < slots.Length; i++)
        {
            // Stop if we run out of prefabs to give out
            if (i >= itemPrefabs.Length) break;
            if (itemPrefabs[i] == null) continue;

            // 1. Create a physical instance of the item UI prefab inside the slot
            GameObject spawnedItem = Instantiate(itemPrefabs[i], slots[i].transform, false);

            // 2. Reset its UI positioning so it centers neatly inside the slot boundary
            RectTransform rect = spawnedItem.GetComponent<RectTransform>();
            if (rect != null)
            {
                rect.anchoredPosition = Vector2.zero;
                rect.localScale = Vector3.one;
            }

            // 3. Link the data reference in the slot script
            slots[i].currentItem = spawnedItem;

            // 4. Tell the slot to refresh its visual image
            slots[i].UpdateSlotVisual();
        }
    }
}