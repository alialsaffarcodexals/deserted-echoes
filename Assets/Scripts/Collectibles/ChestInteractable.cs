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
    [SerializeField] private GameObject[] chestItems;
    
    [Header("Save Settings")]
    [SerializeField] private string chestID;

    private bool playerNearby = false;
    private bool isChestOpen = false;
    private ChestUIController chestUIController;
    

    private void Start()
    {
        chestUIController = FindFirstObjectByType<ChestUIController>();
        LoadChestData();

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
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
            playerNearby = true;
    }



    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
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
    }

    private void LoadChestData()
    {
        if (SaveManager.Instance == null)
            return;

        SaveData saveData = SaveManager.Instance.CurrentSaveData;

        ChestSaveData chestData = saveData.chestSaveData.Find(c => c.chestID == chestID);

        if (chestData == null)
            return;

        for (int i = 0; i < chestItems.Length; i++)
        {
            bool itemStillExists = chestData.remainingItems.Contains(chestItems[i].name);

            if (!itemStillExists)
            {
                chestItems[i] = null;
            }
        }
    }

}