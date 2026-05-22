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

    private InputAction mapAction;
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
        // Use the new input system to find the Map action
        var playerInput = FindFirstObjectByType<PlayerInput>();
        if (playerInput != null)
        {
            mapAction = playerInput.actions.FindAction("Map");
            if (mapAction != null)
            {
                mapAction.performed += OnMapAction;
            }
        }

        playerTransform = GameObject.FindGameObjectWithTag("Player")?.transform;
    }

    private void OnDestroy()
    {
        if (mapAction != null)
        {
            mapAction.performed -= OnMapAction;
        }
    }

    private void OnMapAction(InputAction.CallbackContext context)
    {
        ToggleMap();
    }

    public void ToggleMap()
    {
        isMapOpen = !isMapOpen;
        mapPanel.SetActive(isMapOpen);

        if (isMapOpen)
        {
            Time.timeScale = 0f;
            UpdatePlayerMarker();
        }
        else
        {
            Time.timeScale = 1f;
        }
    }

    private void Update()
    {
        if (isMapOpen)
        {
            UpdatePlayerMarker();
            
            // Allow ESC to close even if PauseMenu might consume it (if map is open, we close it first)
            if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
            {
                // ToggleMap will set isMapOpen to false
                // But we need to make sure we don't double toggle if ESC is bound to Map action too
                // Actually if it's bound to Map action, OnMapAction already handled it.
            }
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