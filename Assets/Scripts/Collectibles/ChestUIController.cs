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

        foreach (Transform child in chestPanel.transform)
        {
            Destroy(child.gameObject);
        }

        GameObject[] chestItems = currentChest.GetChestItems();

        for (int i = 0; i < chestItems.Length; i++)
        {
            if (chestItems[i] == null) continue;

            GameObject slotObj = Instantiate(slotPrefab, chestPanel.transform, false);
            Slot slot = slotObj.GetComponent<Slot>();

            GameObject item = Instantiate(chestItems[i], slotObj.transform, false);

            //changes here


            ItemDragHandler dragHandler = item.GetComponent<ItemDragHandler>();

            if (dragHandler != null)
            {
                Destroy(dragHandler);
            }


            //end here


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

            // change here
            //ChestItemButton button = item.AddComponent<ChestItemButton>();

            ChestItemButton button = slotObj.AddComponent<ChestItemButton>();

            // end here


            button.Setup(i, this);

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
            Debug.Log("ChestUIController: Item moved to inventory.");
        }

        return added;
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