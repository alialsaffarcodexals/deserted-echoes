using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;



public class HotBarController : MonoBehaviour
{
    // changes here
    private PlayerController playerController;
    //-----


    [Header("Hotbar Configuration")]
    [Tooltip("Set how many static slots are generated inside the hotbar from the Inspector.")]
    public int hotbarSlotCount = 7;

    [Header("Hotbar Panel Assignment")]
    [SerializeField] private GameObject hotbarPanel;
    [SerializeField] private GameObject slotPrefab;

    [Header("Selection Styling (Grayed Out Effect)")]
    [SerializeField] private Color activeColor = new Color(0.3f, 0.3f, 0.3f, 0.6f);
    [SerializeField] private Color inactiveColor = new Color(1f, 1f, 1f, 0f);

    private Slot[] hotbarSlots;
    private Image[] selectionHighlights;

    private int currentSelectedIndex = 0;
    public int CurrentSelectedIndex => currentSelectedIndex;

    void Start()
    {

        if (hotbarPanel == null || slotPrefab == null)
        {
            Debug.LogError("HotbarController: Missing references in Inspector!");
            return;
        }

        hotbarSlots = new Slot[hotbarSlotCount];
        selectionHighlights = new Image[hotbarSlotCount];

        for (int i = 0; i < hotbarSlotCount; i++)
        {
            GameObject newSlotObj = Instantiate(slotPrefab, hotbarPanel.transform, false);
            hotbarSlots[i] = newSlotObj.GetComponent<Slot>();

            if (hotbarSlots[i] != null)
            {
                hotbarSlots[i].InitializeSlotNumber(i + 1);
            }

            // Automatically links to the slot's overlay or background panel graphic
            Transform highlightTransform = newSlotObj.transform.Find("Highlight");
            if (highlightTransform != null)
            {
                selectionHighlights[i] = highlightTransform.GetComponent<Image>();
            }
            else
            {
                selectionHighlights[i] = newSlotObj.GetComponentInChildren<Image>();
            }
        }

        UpdateSelectionUI();
        
        // changes here 

        GameObject playerObject = GameObject.FindGameObjectWithTag("Player");

        if (playerObject != null)
        {
            playerController = playerObject.GetComponent<PlayerController>();
        }

        // -- end here ---
    }

    void Update()
    {
        HandleKeyboardInput();
        HandleScrollInput();
        // changes here 
        HandleItemUse();
        // --end here --
    }

    private void HandleKeyboardInput()
    {
        if (Keyboard.current == null) return;

        for (int i = 0; i < hotbarSlotCount; i++)
        {
            Key targetKey = (Key)((int)Key.Digit1 + i);
            if (i < 9 && Keyboard.current[targetKey].wasPressedThisFrame)
            {
                ChangeActiveSlot(i);
                break;
            }
        }
    }

    private void HandleScrollInput()
    {
        if (Mouse.current == null) return;

        float scrollValue = Mouse.current.scroll.ReadValue().y;
        if (scrollValue == 0f) return;

        int nextIndex = currentSelectedIndex;

        if (scrollValue > 0f)
        {
            nextIndex--;
            if (nextIndex < 0) nextIndex = hotbarSlotCount - 1;
        }
        else if (scrollValue < 0f)
        {
            nextIndex++;
            if (nextIndex > hotbarSlotCount - 1) nextIndex = 0;
        }

        ChangeActiveSlot(nextIndex);
    }

    private void ChangeActiveSlot(int newIndex)
    {
        if (newIndex < 0 || newIndex >= hotbarSlotCount) return;
        currentSelectedIndex = newIndex;
        UpdateSelectionUI();
        OnItemChanged();
    }

    public void RefreshActiveSlotItem()
    {
        OnItemChanged();
    }

    private void UpdateSelectionUI()
    {
        // Loops through all generated slots: Only the currentSelectedIndex gets the gray activeColor tint
        for (int i = 0; i < hotbarSlotCount; i++)
        {
            if (selectionHighlights[i] != null)
            {
                selectionHighlights[i].color = (i == currentSelectedIndex) ? activeColor : inactiveColor;
            }
        }
    }

    private void OnItemChanged()
    {
        if (hotbarSlots[currentSelectedIndex] != null && hotbarSlots[currentSelectedIndex].currentItem != null)
        {
            Debug.Log($"Equipped: {hotbarSlots[currentSelectedIndex].currentItem.name} (Slot {currentSelectedIndex + 1})");
        }
        else
        {
            Debug.Log($"Slot {currentSelectedIndex + 1} is empty.");
        }
    }

    public Slot[] GetGeneratedHotbarSlots()
    {
        return hotbarSlots;
    }
    // changes here 
    private void HandleItemUse()
    {
        if (Keyboard.current == null || !Keyboard.current.eKey.wasPressedThisFrame)
            return;

        Slot selectedSlot = hotbarSlots[currentSelectedIndex];

        if (selectedSlot == null || selectedSlot.currentItem == null)
        {
            Debug.Log("HotbarController: No item selected.");
            return;
        }

        IUsableItem usableItem = selectedSlot.currentItem.GetComponent<IUsableItem>();

        if (usableItem == null)
        {
            Debug.Log("HotbarController: Selected item is not usable.");
            return;
        }

        // Re-find player if lost (e.g. scene reload).
        if (playerController == null)
        {
            GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
            if (playerObject != null)
                playerController = playerObject.GetComponent<PlayerController>();
        }

        if (playerController == null)
        {
            Debug.LogWarning("HotbarController: PlayerController not found, cannot use item.");
            return;
        }

        usableItem.Use(playerController.gameObject);

        Destroy(selectedSlot.currentItem);
        selectedSlot.currentItem = null;
        selectedSlot.UpdateSlotVisual();

        // Persist the updated inventory so it survives scene transitions.
        InventoryController.Current?.SaveToStore();

        Debug.Log("HotbarController: Item used.");
    }
    // end here

}