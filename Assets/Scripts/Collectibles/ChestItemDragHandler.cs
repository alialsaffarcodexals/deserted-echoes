using UnityEngine;
using UnityEngine.EventSystems;

public class ChestItemDragHandler : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    private int itemIndex;
    private ChestUIController chestUI;
    private Slot ownerSlot;
    private Transform originalParent;
    private Vector2 originalAnchoredPosition;
    private CanvasGroup canvasGroup;
    private RectTransform rectTransform;

    public void Setup(int index, ChestUIController controller, Slot owner)
    {
        itemIndex = index;
        chestUI = controller;
        ownerSlot = owner;
    }

    void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null)
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        originalParent = transform.parent;
        originalAnchoredPosition = rectTransform.anchoredPosition;
        transform.SetParent(transform.root);
        canvasGroup.blocksRaycasts = false;
        canvasGroup.alpha = 0.6f;
    }

    public void OnDrag(PointerEventData eventData)
    {
        transform.position = eventData.position;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        canvasGroup.blocksRaycasts = true;
        canvasGroup.alpha = 1f;

        Slot dropSlot = null;
        if (eventData.pointerEnter != null)
            dropSlot = eventData.pointerEnter.GetComponentInParent<Slot>();

        if (dropSlot == null || chestUI == null)
        {
            SnapBack();
            return;
        }

        bool droppedBackInChest = chestUI.ChestPanelTransform != null
            && dropSlot.transform.IsChildOf(chestUI.ChestPanelTransform);

        if (droppedBackInChest)
        {
            // Rearranging inside the chest: move/swap into the target slot.
            ChestItemButton targetSlot = dropSlot.GetComponent<ChestItemButton>();

            if (targetSlot != null && chestUI.MoveWithinChest(itemIndex, targetSlot.ItemIndex))
            {
                // RebuildSlots made fresh icons; this dragged one was reparented
                // out of the panel during the drag, so remove it by hand.
                Destroy(gameObject);
                return;
            }

            SnapBack();
            return;
        }

        // Dropped on an inventory/hotbar slot. TakeItem clears the chest slot
        // and destroys this icon on success, so only failure needs handling.
        if (!chestUI.TakeItem(itemIndex, dropSlot))
            SnapBack();
    }

    private void SnapBack()
    {
        transform.SetParent(originalParent);
        rectTransform.anchoredPosition = originalAnchoredPosition;
    }
}
