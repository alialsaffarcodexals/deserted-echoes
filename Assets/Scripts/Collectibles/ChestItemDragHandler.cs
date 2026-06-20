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

        bool droppedBackInChest = dropSlot != null && chestUI != null && chestUI.ChestPanelTransform != null
            && dropSlot.transform.IsChildOf(chestUI.ChestPanelTransform);

        bool moved = false;
        if (dropSlot != null && !droppedBackInChest && chestUI != null)
            moved = chestUI.TakeItem(itemIndex, dropSlot);

        if (moved)
        {
            if (ownerSlot != null)
            {
                ownerSlot.currentItem = null;
                ownerSlot.UpdateSlotVisual();
            }
            Destroy(gameObject);
        }
        else
        {
            transform.SetParent(originalParent);
            rectTransform.anchoredPosition = originalAnchoredPosition;
        }
    }
}
