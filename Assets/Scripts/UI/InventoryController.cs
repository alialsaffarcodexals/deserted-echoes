using UnityEngine;
using UnityEngine.InputSystem;

public class InventoryController : MonoBehaviour
{
    public static InventoryController Current { get; private set; }

    [Header("Panels & Prefabs")]
    public GameObject inventoryPanel;
    public GameObject slotPrefab;
    [Tooltip("Controls exactly how many slots are generated inside the main inventory panel.")]
    public int inventorySlotCount = 16;
    public GameObject[] itemPrefabs;

    private Slot[] staticHotbarSlots;
    private Slot[] inventorySlots;

    private void Awake()
    {
        Current = this;
    }

    private void OnDestroy()
    {
        if (Current == this) Current = null;
    }

    void Start()
    {
        if (inventoryPanel == null || slotPrefab == null)
        {
            Debug.LogError("InventoryController: Missing references in the Inspector!");
            return;
        }

        inventoryPanel.SetActive(false);

        // Register all known prefabs so InventoryStore can restore them by name.
        if (itemPrefabs != null)
        {
            foreach (GameObject p in itemPrefabs)
                InventoryStore.Register(p);
        }

        // 1. Fetch hotbar slots from HotBarController
        HotBarController hotbar = FindFirstObjectByType<HotBarController>();
        int hotbarCount = 0;
        if (hotbar != null)
        {
            staticHotbarSlots = hotbar.GetGeneratedHotbarSlots();
            hotbarCount = staticHotbarSlots != null ? staticHotbarSlots.Length : 0;
        }

        // 2. Generate inventory panel slots
        inventorySlots = new Slot[inventorySlotCount];
        for (int i = 0; i < inventorySlotCount; i++)
        {
            GameObject newSlotObj = Instantiate(slotPrefab, inventoryPanel.transform, false);
            Slot slot = newSlotObj.GetComponent<Slot>();
            if (slot != null)
                slot.InitializeSlotNumber(i + 1);
            inventorySlots[i] = slot;
        }

        // 3. Populate slots — restore from store if we have saved data, else use defaults.
        if (InventoryStore.IsInitialized)
        {
            RestoreFromStore();
        }
        else
        {
            PopulateDefaults(hotbarCount);
        }

        SaveToStore();
    }

    // ── Populate from inspector defaults (first run) ─────────────────────────

    private void PopulateDefaults(int hotbarCount)
    {
        int globalItemIndex = 0;

        if (staticHotbarSlots != null)
        {
            for (int i = 0; i < hotbarCount; i++)
            {
                if (globalItemIndex < itemPrefabs.Length && itemPrefabs[globalItemIndex] != null)
                    SpawnItemInSlot(itemPrefabs[globalItemIndex], staticHotbarSlots[i]);
                globalItemIndex++;
            }
        }

        for (int i = 0; i < inventorySlotCount; i++)
        {
            if (inventorySlots[i] != null && globalItemIndex < itemPrefabs.Length && itemPrefabs[globalItemIndex] != null)
                SpawnItemInSlot(itemPrefabs[globalItemIndex], inventorySlots[i]);
            globalItemIndex++;
        }
    }

    // ── Restore from InventoryStore ──────────────────────────────────────────

    private void RestoreFromStore()
    {
        string[] hotbarData    = InventoryStore.GetHotbarData();
        string[] inventoryData = InventoryStore.GetInventoryData();

        if (staticHotbarSlots != null && hotbarData != null)
        {
            for (int i = 0; i < staticHotbarSlots.Length; i++)
            {
                if (staticHotbarSlots[i] == null) continue;
                string name = i < hotbarData.Length ? hotbarData[i] : null;
                if (!string.IsNullOrEmpty(name))
                {
                    GameObject prefab = InventoryStore.FindPrefab(name);
                    if (prefab != null) SpawnItemInSlot(prefab, staticHotbarSlots[i]);
                }
            }
        }

        if (inventorySlots != null && inventoryData != null)
        {
            for (int i = 0; i < inventorySlots.Length; i++)
            {
                if (inventorySlots[i] == null) continue;
                string name = i < inventoryData.Length ? inventoryData[i] : null;
                if (!string.IsNullOrEmpty(name))
                {
                    GameObject prefab = InventoryStore.FindPrefab(name);
                    if (prefab != null) SpawnItemInSlot(prefab, inventorySlots[i]);
                }
            }
        }
    }

    // ── Public API ───────────────────────────────────────────────────────────

    public bool AddItem(GameObject itemPrefab)
    {
        if (itemPrefab == null)
        {
            Debug.LogWarning("InventoryController: Tried to add a null item.");
            return false;
        }

        InventoryStore.Register(itemPrefab);

        if (staticHotbarSlots != null)
        {
            foreach (Slot slot in staticHotbarSlots)
            {
                if (slot != null && slot.currentItem == null)
                {
                    SpawnItemInSlot(itemPrefab, slot);
                    SaveToStore();
                    return true;
                }
            }
        }

        if (inventorySlots != null)
        {
            foreach (Slot slot in inventorySlots)
            {
                if (slot != null && slot.currentItem == null)
                {
                    SpawnItemInSlot(itemPrefab, slot);
                    SaveToStore();
                    return true;
                }
            }
        }

        Debug.Log("InventoryController: Inventory is full.");
        return false;
    }

    // Snapshot current slot state into InventoryStore.
    public void SaveToStore()
    {
        InventoryStore.Save(staticHotbarSlots, inventorySlots);
    }

    // ── Helpers ──────────────────────────────────────────────────────────────

    private void SpawnItemInSlot(GameObject itemPrefab, Slot targetSlot)
    {
        GameObject item = Instantiate(itemPrefab, targetSlot.transform, false);

        RectTransform itemRect = item.GetComponent<RectTransform>();
        if (itemRect != null)
        {
            itemRect.anchorMin        = Vector2.zero;
            itemRect.anchorMax        = Vector2.one;
            itemRect.sizeDelta        = Vector2.zero;
            itemRect.anchoredPosition = Vector2.zero;
            itemRect.localScale       = Vector3.one;
        }

        targetSlot.currentItem = item;
    }

    void Update()
    {
        if (Keyboard.current != null && Keyboard.current.tabKey.wasPressedThisFrame)
            inventoryPanel.SetActive(!inventoryPanel.activeSelf);
    }
}
