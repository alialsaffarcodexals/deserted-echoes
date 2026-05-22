using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class MapController : MonoBehaviour
{
    public static MapController Instance { get; private set; }
    public bool IsMapOpen => isMapOpen;

    [Header("UI References")]
    [SerializeField] private GameObject mapPanel;
    [SerializeField] private RectTransform playerMarker;
    [SerializeField] private RectTransform mapImage;

    [Header("Level Configuration (Level-01)")]
    [SerializeField] private Vector2 worldMin = new Vector2(-51f, -54f);
    [SerializeField] private Vector2 worldSize = new Vector2(88f, 63f);

    private bool isMapOpen = false;
    private Transform playerTransform;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        if (mapPanel != null) mapPanel.SetActive(false);
    }

    private void Start()
    {
        playerTransform = GameObject.FindGameObjectWithTag("Player")?.transform;
    }

    private void Update()
    {
        bool mPressed = false;
        bool escPressed = false;

        if (Keyboard.current != null)
        {
            mPressed = Keyboard.current.mKey.wasPressedThisFrame;
            escPressed = Keyboard.current.escapeKey.wasPressedThisFrame;
        }
        else
        {
            // Legacy fallback
            mPressed = Input.GetKeyDown(KeyCode.M);
            escPressed = Input.GetKeyDown(KeyCode.Escape);
        }

        if (mPressed)
        {
            Debug.Log("Map Toggle Input Detected.");
            ToggleMap();
        }
        else if (isMapOpen && escPressed)
        {
            Debug.Log("Map Close Input Detected (ESC).");
            ToggleMap();
        }

        if (isMapOpen)
        {
            UpdatePlayerMarker();
        }
    }

    public void ToggleMap()
    {
        if (mapPanel == null)
        {
            Debug.LogError("MapController: mapPanel is not assigned!");
            return;
        }

        isMapOpen = !isMapOpen;
        mapPanel.SetActive(isMapOpen);

        if (isMapOpen)
        {
            Time.timeScale = 0f;
            UpdatePlayerMarker();
            Debug.Log("Map Opened.");
        }
        else
        {
            Time.timeScale = 1f;
            Debug.Log("Map Closed.");
        }
    }

    private void UpdatePlayerMarker()
    {
        if (playerTransform == null)
        {
            playerTransform = GameObject.FindGameObjectWithTag("Player")?.transform;
            if (playerTransform == null) return;
        }

        if (playerMarker == null || mapImage == null) return;

        Vector2 worldPos = playerTransform.position;
        
        // Calculate normalized position (0 to 1)
        float normX = (worldPos.x - worldMin.x) / worldSize.x;
        float normY = (worldPos.y - worldMin.y) / worldSize.y;

        // Map to UI size
        Vector2 mapSize = mapImage.rect.size;
        float uiX = (normX - 0.5f) * mapSize.x; // Subtract 0.5 because anchoredPosition is center-based
        float uiY = (normY - 0.5f) * mapSize.y;

        playerMarker.anchoredPosition = new Vector2(uiX, uiY);
        
        // Update rotation
        playerMarker.localRotation = Quaternion.Euler(0, 0, playerTransform.eulerAngles.z);
    }
}