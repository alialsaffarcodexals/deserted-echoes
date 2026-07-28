using UnityEngine;
using UnityEngine.EventSystems;

// Lives on every chest UI slot. Handles click-to-take, and doubles as the
// marker that identifies a slot as a chest slot (with its index) so items
// dragged from the inventory/hotbar can be dropped back into the chest.
public class ChestItemButton : MonoBehaviour, IPointerClickHandler
{
    private int itemIndex;
    private ChestUIController chestUI;

    public int ItemIndex => itemIndex;
    public ChestUIController ChestUI => chestUI;

    public void Setup(int index, ChestUIController controller)
    {
        itemIndex = index;
        chestUI = controller;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (chestUI == null) return;

        // TakeItem clears the slot visual on success; the slot frame itself
        // stays so the chest keeps its full grid and can accept items back.
        chestUI.TakeItem(itemIndex);
    }
}
