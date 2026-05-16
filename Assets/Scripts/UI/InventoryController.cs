using UnityEngine;
using UnityEngine.InputSystem;

public class InventoryController : MonoBehaviour
{
    public GameObject inventoryPanel;
    public GameObject slotPrefab;
    public int slotCount;
    public GameObject[] itemPrefabs;

    void Start()
    {
        if (inventoryPanel == null)
        {
            Debug.LogError("InventoryController: Inventory Panel is not assigned in the Inspector!");
            return;
        }

        if (slotPrefab == null)
        {
            // No slot prefab assigned — TAB toggle still works, slots just won't spawn
            return;
        }

        inventoryPanel.SetActive(false); // Hide the inventory panel at the start

        for (int i = 0; i < slotCount; i++)
        {
            // Instantiate the slot as a child of the panel
            GameObject newSlotObj = Instantiate(slotPrefab, inventoryPanel.transform);

            // Try to get the Slot component
            Slot slot = newSlotObj.GetComponent<Slot>();

            if (slot == null)
            {
                Debug.LogError($"InventoryController: The Slot Prefab is missing the 'Slot' script component at index {i}!");
                continue;
            }

            // 3. Fill slot with item if one exists in the array
            if (i < itemPrefabs.Length && itemPrefabs[i] != null)
            {
                GameObject item = Instantiate(itemPrefabs[i], newSlotObj.transform);

                // Ensure the item is centered in the slot
                RectTransform itemRect = item.GetComponent<RectTransform>();
                if (itemRect != null)
                {
                    itemRect.anchoredPosition = Vector2.zero;
                }

                slot.currentItem = item;
            }
        }
    }

    void Update()
    {
        if (Keyboard.current != null && Keyboard.current.tabKey.wasPressedThisFrame)
        {
            inventoryPanel.SetActive(!inventoryPanel.activeSelf);
        }
    }
}
