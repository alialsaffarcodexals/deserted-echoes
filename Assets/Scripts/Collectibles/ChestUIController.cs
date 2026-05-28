using UnityEngine;

public class ChestUIController : MonoBehaviour
{
    [Header("Chest UI")]
    [SerializeField] private GameObject chestPanel;
    [SerializeField] private GameObject slotPrefab;

    private InventoryController inventoryController;
    private ChestInteractable currentChest;

    private void Start()
    {
        inventoryController = FindFirstObjectByType<InventoryController>();

        if (chestPanel != null)
            chestPanel.SetActive(false);
    }

    public void OpenChest(ChestInteractable chest)
    {
        if (chestPanel == null || slotPrefab == null) return;

        currentChest = chest;
        chestPanel.SetActive(true);

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

            ChestItemButton button = item.AddComponent<ChestItemButton>();
            button.Setup(i, this);
        }
    }

    public void TakeItem(int itemIndex)
    {
        if (currentChest == null) return;

        GameObject itemPrefab = currentChest.GetItemAt(itemIndex);

        if (itemPrefab == null) return;

        if (inventoryController == null)
            inventoryController = FindFirstObjectByType<InventoryController>();

        if (inventoryController != null && inventoryController.AddItem(itemPrefab))
        {
            currentChest.RemoveItemAt(itemIndex);

            Debug.Log("ChestUIController: Item moved to inventory.");
        }
    }

    public void CloseChest()
    {
        if (chestPanel != null)
            chestPanel.SetActive(false);
    }
}