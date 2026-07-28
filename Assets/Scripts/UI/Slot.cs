using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class Slot : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public GameObject currentItem;

    [Header("UI Display")]
    [SerializeField] private TextMeshProUGUI slotNumberText;
    [SerializeField] private Image itemIconImage; // Drag your Slot's Child Image component here

    void Start()
    {
        // Update the visual representation on start
        UpdateSlotVisual();
    }

 
    public void InitializeSlotNumber(int displayIndex)
    {
        if (slotNumberText != null)
        {
            slotNumberText.text = displayIndex.ToString();
            slotNumberText.gameObject.SetActive(true);
        }
    }

    public void UpdateSlotVisual()
    {
        if (itemIconImage == null) return;

        // Check if there is a real item assigned to this slot data slot
        if (currentItem != null)
        {
            // Look for the Image component on the spawned item asset
            Image itemImageComponent = currentItem.GetComponentInChildren<Image>();

            if (itemImageComponent != null)
            {
                // Synchronize the Slot's display icon to match the Item's icon
                itemIconImage.sprite = itemImageComponent.sprite;
                itemIconImage.enabled = true;

                // Turn off the item's native image component if it's blocking raycasts 
                // or overlapping duplicate graphics over your slot container frame.
                itemImageComponent.enabled = false;
            }
        }
        else
        {
            // No item? Hide the icon display completely
            itemIconImage.sprite = null;
            itemIconImage.enabled = false;

            // If the cursor was hovering this slot when the item left it,
            // don't leave a stale tooltip on screen.
            ItemTooltip.HideFor(this);
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (currentItem != null)
            ItemTooltip.Show(ItemInfo.GetDisplayName(currentItem), this);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        ItemTooltip.HideFor(this);
    }
}