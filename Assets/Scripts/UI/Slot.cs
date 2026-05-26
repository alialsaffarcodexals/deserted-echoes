using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class Slot : MonoBehaviour
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
        }
    }
}