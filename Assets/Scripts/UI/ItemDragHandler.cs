using UnityEngine;
using UnityEngine.EventSystems;

public class ItemDragHandler : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    private Transform originalParent;
    private CanvasGroup canvasGroup;
    private RectTransform rectTransform;

    void Start()
    {
        canvasGroup = GetComponent<CanvasGroup>();
        rectTransform = GetComponent<RectTransform>();
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        originalParent = transform.parent; // Save original parent
        transform.SetParent(transform.root); // Move to root to avoid being masked
        canvasGroup.blocksRaycasts = false; // Allow raycasts to pass through while dragging
        canvasGroup.alpha = 0.6f; // Make the item semi-transparent while dragging
    }

    public void OnDrag(PointerEventData eventData)
    {
        transform.position = eventData.position; // Follow the mouse
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        canvasGroup.blocksRaycasts = true;
        canvasGroup.alpha = 1f;

        Slot dropSlot = null;
        if (eventData.pointerEnter != null)
        {
            dropSlot = eventData.pointerEnter.GetComponentInParent<Slot>();
        }

        Slot originalSlot = originalParent.GetComponent<Slot>();

        // Dropped onto a chest slot? Route through the chest so its data and
        // save get updated (just reparenting the icon wouldn't persist).
        ChestItemButton chestSlot = dropSlot != null ? dropSlot.GetComponent<ChestItemButton>() : null;
        if (chestSlot != null && chestSlot.ChestUI != null)
        {
            bool stored = chestSlot.ChestUI.StoreItem(chestSlot.ItemIndex, gameObject);

            if (stored)
            {
                if (originalSlot != null)
                {
                    originalSlot.currentItem = null;
                    originalSlot.UpdateSlotVisual();
                }

                InventoryController.Current?.SaveToStore();

                HotBarController hb = FindFirstObjectByType<HotBarController>();
                if (hb != null) hb.RefreshActiveSlotItem();

                Destroy(gameObject);
            }
            else
            {
                transform.SetParent(originalParent);
                GetComponent<RectTransform>().localScale = Vector3.one;
                rectTransform.anchoredPosition = Vector2.zero;
            }

            return;
        }

        if (dropSlot != null)
        {
            if (dropSlot.currentItem != null && dropSlot.currentItem != gameObject)
            {
                GameObject itemInDropSlot = dropSlot.currentItem;

                itemInDropSlot.transform.SetParent(originalSlot.transform);
                originalSlot.currentItem = itemInDropSlot;

                RectTransform swappedRect = itemInDropSlot.GetComponent<RectTransform>();
                if (swappedRect != null) swappedRect.anchoredPosition = Vector2.zero;
            }
            else
            {
                originalSlot.currentItem = null;
            }

            transform.SetParent(dropSlot.transform);
            dropSlot.currentItem = gameObject;

            GetComponent<RectTransform>().localScale = Vector3.one;
        }
        else
        {
            transform.SetParent(originalParent);
            GetComponent<RectTransform>().localScale = Vector3.one;
        }

        rectTransform.anchoredPosition = Vector2.zero;

        if (originalSlot != null) originalSlot.UpdateSlotVisual();
        if (dropSlot != null) dropSlot.UpdateSlotVisual();

        HotBarController hotbar = FindFirstObjectByType<HotBarController>();
        if (hotbar != null)
        {
            hotbar.RefreshActiveSlotItem();
        }
    }
}