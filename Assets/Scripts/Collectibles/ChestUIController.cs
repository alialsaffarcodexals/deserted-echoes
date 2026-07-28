using UnityEngine;
using UnityEngine.InputSystem;

public class ChestUIController : MonoBehaviour
{
    [Header("Chest UI")]
    [SerializeField] private GameObject chestPanel;
    [SerializeField] private GameObject slotPrefab;
    [SerializeField] private float inventoryOffsetXWhileChestOpen = 427f;

    private InventoryController inventoryController;
    private ChestInteractable currentChest;
    private RectTransform inventoryPanelRect;
    private Vector2 inventoryPanelDefaultPosition;
    private Slot[] uiSlots;

    // Lets InventoryController know not to also toggle the inventory panel
    // when Tab is used to close the chest in the same frame.
    public static bool AnyChestOpen { get; private set; }

    public Transform ChestPanelTransform => chestPanel != null ? chestPanel.transform : null;

    private void LateUpdate()
    {
        if (currentChest == null) return;

        if (Keyboard.current != null && Keyboard.current.tabKey.wasPressedThisFrame)
        {
            currentChest.NotifyClosedExternally();
            CloseChest();
        }
    }

    private void Start()
    {
        inventoryController = FindFirstObjectByType<InventoryController>();

        if (inventoryController != null && inventoryController.inventoryPanel != null)
        {
            inventoryPanelRect = inventoryController.inventoryPanel.GetComponent<RectTransform>();
            inventoryPanelDefaultPosition = inventoryPanelRect.anchoredPosition;
        }

        if (chestPanel != null)
            chestPanel.SetActive(false);
    }

    public void OpenChest(ChestInteractable chest)
    {
        if (chestPanel == null || slotPrefab == null) return;

        currentChest = chest;
        AnyChestOpen = true;
        chestPanel.SetActive(true);

        if (inventoryController != null && inventoryController.inventoryPanel != null)
        {
            inventoryController.inventoryPanel.SetActive(true);

            if (inventoryPanelRect != null)
                inventoryPanelRect.anchoredPosition = new Vector2(inventoryOffsetXWhileChestOpen, inventoryPanelDefaultPosition.y);
        }

        RebuildSlots();
    }

    // Builds one UI slot per chest array index, INCLUDING empty ones, so the
    // slot frames stay visible after items are taken and can receive drops.
    private void RebuildSlots()
    {
        foreach (Transform child in chestPanel.transform)
        {
            Destroy(child.gameObject);
        }

        GameObject[] chestItems = currentChest.GetChestItems();
        uiSlots = new Slot[chestItems.Length];

        for (int i = 0; i < chestItems.Length; i++)
        {
            GameObject slotObj = Instantiate(slotPrefab, chestPanel.transform, false);
            Slot slot = slotObj.GetComponent<Slot>();
            uiSlots[i] = slot;

            // Added to every slot (even empty ones): handles click-to-take and
            // marks this as a chest slot so inventory drags can drop into it.
            ChestItemButton button = slotObj.AddComponent<ChestItemButton>();
            button.Setup(i, this);

            if (chestItems[i] == null) continue;

            GameObject item = Instantiate(chestItems[i], slotObj.transform, false);

            // Chest items use ChestItemDragHandler instead of the inventory one.
            ItemDragHandler dragHandler = item.GetComponent<ItemDragHandler>();
            if (dragHandler != null)
            {
                Destroy(dragHandler);
            }

            // for item positions in the chest
            RectTransform itemRect = item.GetComponent<RectTransform>();

            if (itemRect != null)
            {
                itemRect.anchorMin = Vector2.zero;
                itemRect.anchorMax = Vector2.one;
                itemRect.sizeDelta = Vector2.zero;
                itemRect.anchoredPosition = Vector2.zero;
                itemRect.localScale = Vector3.one;
            }
            else
            {
                item.transform.localPosition = Vector3.zero;
                item.transform.localScale = Vector3.one;
            }

            if (slot != null)
            {
                slot.currentItem = item;
                slot.UpdateSlotVisual();
            }

            ChestItemDragHandler dragger = item.AddComponent<ChestItemDragHandler>();
            dragger.Setup(i, this, slot);
        }
    }

    public bool TakeItem(int itemIndex, Slot targetSlot = null)
    {
        if (currentChest == null) return false;

        GameObject itemPrefab = currentChest.GetItemAt(itemIndex);

        if (itemPrefab == null) return false;

        if (inventoryController == null)
            inventoryController = FindFirstObjectByType<InventoryController>();

        if (inventoryController == null) return false;

        bool added = targetSlot != null
            ? inventoryController.PlaceItemInSlot(itemPrefab, targetSlot)
            : inventoryController.AddItem(itemPrefab);

        if (added)
        {
            currentChest.RemoveItemAt(itemIndex);
            ClearUISlot(itemIndex);
            Debug.Log("ChestUIController: Item moved to inventory.");
        }

        return added;
    }

    // Puts an item (dragged from inventory/hotbar) back into an empty chest slot.
    // Returns false if the slot is occupied or the item can't be identified.
    public bool StoreItem(int slotIndex, GameObject draggedItem)
    {
        if (currentChest == null || draggedItem == null) return false;
        if (currentChest.GetItemAt(slotIndex) != null) return false;

        // Dragged UI items are clones, e.g. "BreadPrefab Variant(Clone)" —
        // strip the suffix to get back to the prefab name.
        string prefabName = draggedItem.name.Replace("(Clone)", "").Trim();
        GameObject prefab = currentChest.ResolvePrefab(prefabName);

        if (prefab == null)
        {
            Debug.LogWarning($"ChestUIController: couldn't resolve '{prefabName}' to store in chest.");
            return false;
        }

        currentChest.SetItemAt(slotIndex, prefab);
        RebuildSlots();

        Debug.Log("ChestUIController: Item stored in chest.");
        return true;
    }

    // Rearranges items inside the chest itself. Works for both moving into an
    // empty slot and swapping with an occupied one.
    public bool MoveWithinChest(int fromIndex, int toIndex)
    {
        if (currentChest == null) return false;
        if (fromIndex == toIndex) return false;
        if (currentChest.GetItemAt(fromIndex) == null) return false;

        currentChest.SwapItemsAt(fromIndex, toIndex);
        RebuildSlots();

        return true;
    }

    // Empties one chest UI slot but keeps the slot frame visible.
    private void ClearUISlot(int index)
    {
        if (uiSlots == null || index < 0 || index >= uiSlots.Length) return;

        Slot slot = uiSlots[index];
        if (slot == null) return;

        if (slot.currentItem != null)
            Destroy(slot.currentItem);

        slot.currentItem = null;
        slot.UpdateSlotVisual();
    }

    public void CloseChest()
    {
        currentChest = null;
        AnyChestOpen = false;

        if (chestPanel != null)
            chestPanel.SetActive(false);

        if (inventoryController != null && inventoryController.inventoryPanel != null)
        {
            inventoryController.inventoryPanel.SetActive(false);

            if (inventoryPanelRect != null)
                inventoryPanelRect.anchoredPosition = inventoryPanelDefaultPosition;
        }
    }
}