/*using UnityEngine;
using UnityEngine.InputSystem;

public class ChestInteractable : MonoBehaviour
{
    [Header("Chest Settings")]
    [SerializeField] private GameObject[] itemDropPrefabs;
    [SerializeField] private int numberOfDrops = 2;
    [SerializeField] private float dropRadius = 0.7f;

    private bool playerNearby = false;
    private bool opened = false;

    private void Update()
    {
        if (!playerNearby || opened) return;

        if (Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame)
        {
            OpenChest();
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
            playerNearby = true;
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
            playerNearby = false;
    }

    private void OpenChest()
    {
        opened = true;

        for (int i = 0; i < numberOfDrops; i++)
        {
            if (itemDropPrefabs == null || itemDropPrefabs.Length == 0)
                break;

            GameObject itemPrefab = itemDropPrefabs[Random.Range(0, itemDropPrefabs.Length)];

            if (itemPrefab == null)
                continue;

            Vector2 offset = Random.insideUnitCircle * dropRadius;
            Vector3 dropPosition = transform.position + new Vector3(offset.x, offset.y, 0f);

            Instantiate(itemPrefab, dropPosition, Quaternion.identity);
        }

        Debug.Log("ChestInteractable: Chest opened and items dropped.");

        Destroy(gameObject);
    }
}
*/
using UnityEngine;
using UnityEngine.InputSystem;

public class ChestInteractable : MonoBehaviour
{
    [Header("Chest Items")]
    [Tooltip("Hand-placed items. Ignored if a Loot Table is assigned below.")]
    [SerializeField] private GameObject[] chestItems;

    [Header("Random Loot (optional)")]
    [Tooltip("If assigned, this chest rolls its contents from the table on first load instead of using the hand-placed list above. The roll is saved, so it never re-rolls.")]
    [SerializeField] private LootTable lootTable;

    [Header("Chest Size")]
    [Tooltip("Total slots the chest UI shows, even when mostly empty. 16 = 4x4 grid.")]
    [SerializeField] private int chestCapacity = 16;

    [Header("Save Settings")]
    [SerializeField] private string chestID;

    private bool playerNearby = false;
    private bool isChestOpen = false;
    private ChestUIController chestUIController;

    // Copy of the inspector-assigned items, kept so saved names can be
    // resolved back to prefabs on load (slots in chestItems get nulled as
    // items are taken).
    private GameObject[] originalItems;

    private void Awake()
    {
        // Auto-ID: if no chestID was set in the Inspector, build a stable one
        // from the scene name + this chest's position. Position doesn't change
        // between sessions, so saves keep matching. Two chests would only
        // collide if they sat on the exact same spot in the same scene.
        if (string.IsNullOrEmpty(chestID))
        {
            Vector3 p = transform.position;
            chestID = $"{gameObject.scene.name}_chest_{p.x:F1}_{p.y:F1}";
        }
    }

    private void Start()
    {
        chestUIController = FindFirstObjectByType<ChestUIController>();

        EnsureCapacity();

        originalItems = (GameObject[])chestItems.Clone();

        LoadChestData();

        // Saves from before the capacity setting may restore a shorter array;
        // pad it back out so the UI always shows the full grid.
        EnsureCapacity();
    }

    // Grows chestItems to chestCapacity (never shrinks, so no items are lost).
    private void EnsureCapacity()
    {
        if (chestItems == null)
        {
            chestItems = new GameObject[chestCapacity];
            return;
        }

        if (chestItems.Length >= chestCapacity)
            return;

        GameObject[] expanded = new GameObject[chestCapacity];
        for (int i = 0; i < chestItems.Length; i++)
        {
            expanded[i] = chestItems[i];
        }

        chestItems = expanded;
    }
    /*
    private void Update()
    {
        
        if (!playerNearby) return;

        if (Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame)
        {
            if (chestUIController != null)
            {
                chestUIController.OpenChest(chestItems);
            }
        }
        


    }
        */

    private void Update()
    {
        if (!playerNearby) return;

        if (Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame)
        {
            Debug.Log("E pressed on chest");

            if (chestUIController == null) return;

            if (isChestOpen)
            {
                chestUIController.CloseChest();
                isChestOpen = false;
            }
            else
            {
                chestUIController.OpenChest(this);
                isChestOpen = true;
            }
            
        }

    }

    public void NotifyClosedExternally()
    {
        isChestOpen = false;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
            playerNearby = true;
    }



    private void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag("Player"))
            return;

        playerNearby = false;

        if (chestUIController != null)
        {
            chestUIController.CloseChest();
        }

        isChestOpen = false;
    }

    private void SaveChestData()
    {
        if (SaveManager.Instance == null)
            return;

        SaveData saveData = SaveManager.Instance.CurrentSaveData;

        ChestSaveData existingChest = saveData.chestSaveData.Find(c => c.chestID == chestID);

        if (existingChest == null)
        {
            existingChest = new ChestSaveData();
            existingChest.chestID = chestID;

            saveData.chestSaveData.Add(existingChest);
        }

        existingChest.initialized = true;

        // Slot-indexed storage: empty string marks a taken/empty slot, so two
        // items with the same name stay independent.
        existingChest.slotItems.Clear();
        foreach (GameObject item in chestItems)
        {
            existingChest.slotItems.Add(item != null ? item.name : "");
        }

        // Keep the legacy list in sync too.
        existingChest.remainingItems.Clear();
        foreach (GameObject item in chestItems)
        {
            if (item != null)
            {
                existingChest.remainingItems.Add(item.name);
            }
        }
    }

    public GameObject[] GetChestItems()
    {
        return chestItems;
    }

    public GameObject GetItemAt(int index)
    {
        if (index < 0 || index >= chestItems.Length)
            return null;

        return chestItems[index];
    }

    public void RemoveItemAt(int index)
    {
        if (index < 0 || index >= chestItems.Length)
            return;

        chestItems[index] = null;

        SaveChestData();
        SaveManager.Instance?.SaveGame();
    }

    // Puts an item into a chest slot (used when dragging items back into the chest).
    public void SetItemAt(int index, GameObject itemPrefab)
    {
        if (index < 0 || index >= chestItems.Length)
            return;

        chestItems[index] = itemPrefab;

        SaveChestData();
        SaveManager.Instance?.SaveGame();
    }

    // Swaps two slots, for rearranging items inside the chest.
    public void SwapItemsAt(int indexA, int indexB)
    {
        if (indexA < 0 || indexA >= chestItems.Length) return;
        if (indexB < 0 || indexB >= chestItems.Length) return;
        if (indexA == indexB) return;

        GameObject temp = chestItems[indexA];
        chestItems[indexA] = chestItems[indexB];
        chestItems[indexB] = temp;

        SaveChestData();
        SaveManager.Instance?.SaveGame();
    }

    private void LoadChestData()
    {
        ChestSaveData chestData = null;

        if (SaveManager.Instance != null)
        {
            SaveData saveData = SaveManager.Instance.CurrentSaveData;
            chestData = saveData.chestSaveData.Find(c => c.chestID == chestID);
        }

        // First time this chest is ever loaded (no save entry at all): if it
        // has a loot table, roll its contents once and save the roll so it's
        // locked in permanently. Manual chests just keep their inspector items.
        if (chestData == null)
        {
            if (lootTable != null)
            {
                GameObject[] rolled = lootTable.RollLoot();

                // Scatter the rolled items into random positions across the
                // full grid instead of packing them into the first slots.
                chestItems = new GameObject[Mathf.Max(chestCapacity, rolled.Length)];

                System.Collections.Generic.List<int> freeSlots = new System.Collections.Generic.List<int>();
                for (int i = 0; i < chestItems.Length; i++)
                    freeSlots.Add(i);

                foreach (GameObject rolledItem in rolled)
                {
                    if (freeSlots.Count == 0) break;

                    int pick = Random.Range(0, freeSlots.Count);
                    chestItems[freeSlots[pick]] = rolledItem;
                    freeSlots.RemoveAt(pick);
                }

                originalItems = (GameObject[])chestItems.Clone();
                SaveChestData();
            }
            return;
        }

        // Slot-based restore (current format).
        if (chestData.slotItems.Count > 0)
        {
            GameObject[] restored = new GameObject[chestData.slotItems.Count];

            for (int i = 0; i < chestData.slotItems.Count; i++)
            {
                restored[i] = ResolvePrefab(chestData.slotItems[i]);
            }

            chestItems = restored;
            return;
        }

        // Legacy restore (name-only list from older saves).
        for (int i = 0; i < chestItems.Length; i++)
        {
            if (chestItems[i] == null) continue;

            bool itemStillExists = chestData.remainingItems.Contains(chestItems[i].name);

            if (!itemStillExists)
            {
                chestItems[i] = null;
            }
        }
    }

    // Maps a prefab name back to a prefab reference: loot table first, then the
    // original inspector-assigned items, then the inventory's registered prefabs
    // (covers items dragged in from the player's inventory that this chest
    // never originally contained).
    public GameObject ResolvePrefab(string prefabName)
    {
        if (string.IsNullOrEmpty(prefabName))
            return null;

        if (lootTable != null)
        {
            GameObject fromTable = lootTable.FindPrefabByName(prefabName);
            if (fromTable != null)
                return fromTable;
        }

        if (originalItems != null)
        {
            foreach (GameObject item in originalItems)
            {
                if (item != null && item.name == prefabName)
                    return item;
            }
        }

        GameObject fromStore = InventoryStore.FindPrefab(prefabName);
        if (fromStore != null)
            return fromStore;

        Debug.LogWarning($"ChestInteractable '{chestID}': couldn't resolve item '{prefabName}' to a prefab.");
        return null;
    }

}