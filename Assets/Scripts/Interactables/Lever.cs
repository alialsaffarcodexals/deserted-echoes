using UnityEngine;
using UnityEngine.Tilemaps;

public class Lever : MonoBehaviour
{
    [Header("Lever Tilemap")]
    [Tooltip("The Tilemap layer that contains the lever tile")]
    [SerializeField] private Tilemap leverTilemap;

    [Header("Lever Tiles")]
    [Tooltip("The tile currently painted (lever OFF state)")]
    [SerializeField] private TileBase leverOffTile;
    [Tooltip("The tile to swap to when activated (lever ON state)")]
    [SerializeField] private TileBase leverOnTile;

    [Header("Walls to Remove")]
    [Tooltip("First Tilemap to hide when the lever is pulled")]
    [SerializeField] private Tilemap wallTilemap1;
    [Tooltip("Second Tilemap to hide when the lever is pulled")]
    [SerializeField] private Tilemap wallTilemap2;

    [Header("Interaction")]
    [SerializeField] private float interactRadius = 1.5f;

    private bool isActivated = false;
    private Transform playerTransform;
    private Vector3Int leverCellPosition;

    private void Start()
    {
        PlayerController player = FindAnyObjectByType<PlayerController>();
        if (player != null)
            playerTransform = player.transform;

        // Snap this GameObject's position to the centre of the lever cell
        if (leverTilemap != null)
        {
            leverCellPosition = leverTilemap.WorldToCell(transform.position);
            Debug.Log($"[Lever] Cell position resolved to {leverCellPosition}");
        }
    }

    private void Update()
    {
        if (isActivated || playerTransform == null)
            return;

        float dist = Vector2.Distance(transform.position, playerTransform.position);
        if (dist <= interactRadius && Input.GetKeyDown(KeyCode.E))
            Activate();
    }

    private void Activate()
    {
        isActivated = true;

        // Swap the tile on the tilemap from OFF → ON
        if (leverTilemap != null && leverOnTile != null)
            leverTilemap.SetTile(leverCellPosition, leverOnTile);

        // Hide the assigned wall tilemaps
        if (wallTilemap1 != null)
            wallTilemap1.gameObject.SetActive(false);
        if (wallTilemap2 != null)
            wallTilemap2.gameObject.SetActive(false);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, interactRadius);
    }
}
