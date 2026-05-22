using UnityEngine;
using UnityEngine.UI;

public class MinimapController : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private RectTransform mapContent; // The large map image
    [SerializeField] private RectTransform playerMarker; // Stationary in center of mask

    [Header("Level Configuration")]
    [SerializeField] private Vector2 worldMin = new Vector2(-51f, -54f);
    [SerializeField] private Vector2 worldSize = new Vector2(88f, 63f);

    private Transform playerTransform;

    public void Configure(Vector2 min, Vector2 size, Sprite mapSprite)
    {
        worldMin = min;
        worldSize = size;
        if (mapContent != null)
        {
            var img = mapContent.GetComponent<UnityEngine.UI.Image>();
            if (img != null) img.sprite = mapSprite;
        }
    }

    private void Start()
    {
        playerTransform = GameObject.FindGameObjectWithTag("Player")?.transform;
    }

    private void Update()
    {
        if (playerTransform == null)
        {
            playerTransform = GameObject.FindGameObjectWithTag("Player")?.transform;
            if (playerTransform == null) return;
        }

        UpdateMapPosition();
    }

    private void UpdateMapPosition()
    {
        if (mapContent == null) return;

        Vector2 worldPos = playerTransform.position;

        // Calculate normalized position (0 to 1)
        float normX = (worldPos.x - worldMin.x) / worldSize.x;
        float normY = (worldPos.y - worldMin.y) / worldSize.y;

        // The mapContent should be shifted so that the player's position is at the center (0,0 of the parent mask)
        // mapContent size should be significantly larger than the mask for scrolling effect.
        Vector2 mapSize = mapContent.rect.size;

        // The center of the map image represents the center of the world bounds (conceptually)
        // But our worldMin/worldSize define the boundaries.
        // AnchoredPosition (0,0) of mapContent is its pivot. 
        // If pivot is (0.5, 0.5), then at norm (0.5, 0.5), anchoredPosition should be (0,0).
        
        float uiX = -(normX - 0.5f) * mapSize.x;
        float uiY = -(normY - 0.5f) * mapSize.y;

        mapContent.anchoredPosition = new Vector2(uiX, uiY);

        // Update player marker rotation if desired
        if (playerMarker != null)
        {
            playerMarker.localRotation = Quaternion.Euler(0, 0, -playerTransform.eulerAngles.z);
        }
    }
}