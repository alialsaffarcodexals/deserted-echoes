using UnityEngine;
using UnityEngine.EventSystems;

public class ChestItemButton : MonoBehaviour, IPointerClickHandler
{
    private int itemIndex;
    private ChestUIController chestUI;

    public void Setup(int index, ChestUIController controller)
    {
        itemIndex = index;
        chestUI = controller;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (chestUI == null) return;

        chestUI.TakeItem(itemIndex);

        Destroy(gameObject);
    }
}