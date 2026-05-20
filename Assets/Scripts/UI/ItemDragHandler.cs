using UnityEngine;
using UnityEngine.EventSystems;

public class ItemDragHandler : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    Transform originalParent;
    CanvasGroup canvasGroup;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        canvasGroup = GetComponent<CanvasGroup>();
        
    }
    public void OnBeginDrag(PointerEventData eventData)
    {
        originalParent = transform.parent; //Save original parent
        transform.SetParent(transform.root); //Move to root to avoid being masked
        canvasGroup.blocksRaycasts = false; //Allow raycasts to pass through while dragging
        canvasGroup.alpha = 0.6f; //Make the item semi-transparent while dragging
    }

    public void OnDrag(PointerEventData eventData)
    {
        transform.position = eventData.position; //Follow the mouse
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        canvasGroup.blocksRaycasts = true; // enable raycasts
        canvasGroup.alpha = 1f; //Reset transparency

        Slot dropSlot = eventData.pointerEnter?.GetComponent<Slot>(); //Check if we dropped on a slot
        Slot originalSlot = originalParent.GetComponent<Slot>(); //Get the original slot


        if (dropSlot == null) {

            GameObject item = eventData.pointerEnter;
            if (item != null) { 
            dropSlot = item.GetComponentInParent<Slot>();
            }

        }



        if(dropSlot != null)
        {
            if(dropSlot.currentItem != null)
            {
                //Slot is occupied, swap items
                dropSlot.currentItem.transform.SetParent(originalSlot.transform); //Move existing item back to original slot
                originalSlot.currentItem = dropSlot.currentItem; //Update original slot reference
                dropSlot.currentItem.GetComponent<RectTransform>().anchoredPosition = Vector2.zero; //Reset position
            }
            else
            {
                //Slot is empty, just move the item
                originalSlot.currentItem = null; //Clear original slot reference
            }

            //Move dragged item to new slot
            transform.SetParent(dropSlot.transform);
            dropSlot.currentItem = gameObject; //Update new slot referenc
        }
        else
        {
            //Not dropped on a slot, return to original position
            transform.SetParent(originalParent);
        }

        GetComponent<RectTransform>().anchoredPosition = Vector2.zero; //Reset position
    }
    
}
