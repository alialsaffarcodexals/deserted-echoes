using UnityEngine;
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