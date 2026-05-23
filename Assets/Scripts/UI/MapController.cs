using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class MapController : MonoBehaviour
{
    public static MapController Instance { get; private set; }
    public bool IsMapOpen => isMapOpen;
    public Texture2D DiscoveryTexture => discoveryTexture;

    [Header("UI References")]
    [SerializeField] private GameObject mapPanel;
    [SerializeField] private RectTransform playerMarker;
    [SerializeField] private RectTransform mapImage;
    [SerializeField] private RawImage fogImage;

    [Header("Level Configuration")]
    [SerializeField] private Vector2 worldMin = new Vector2(-51f, -54f);
    [SerializeField] private Vector2 worldSize = new Vector2(88f, 63f);
    [SerializeField] private Vector2 markerOffset = Vector2.zero;

    [Header("Zoom Settings")]
    [SerializeField] private float minZoom = 0.5f;
    [SerializeField] private float maxZoom = 4.0f;
    [SerializeField] private float zoomSpeed = 0.2f;

    [Header("Discovery Settings")]
    [SerializeField] private int discoveryResolution = 128;
    [SerializeField] private float revealRadius = 5f;

    private float currentZoom = 1.0f;
    private bool isMapOpen = false;
    private Transform playerTransform;
    private Texture2D discoveryTexture;
    private Color32[] discoveryPixels;

    public void Configure(Vector2 min, Vector2 size, Sprite mapSprite)
    {
        worldMin = min;
        worldSize = size;
        if (mapImage != null)
        {
            var img = mapImage.GetComponent<UnityEngine.UI.Image>();
            if (img != null) img.sprite = mapSprite;
        }
        
        InitializeDiscoveryTexture();
    }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        if (mapPanel != null) mapPanel.SetActive(false);
        
        InitializeDiscoveryTexture();
    }

    private void InitializeDiscoveryTexture()
    {
        if (discoveryTexture == null)
        {
            discoveryTexture = new Texture2D(discoveryResolution, discoveryResolution, TextureFormat.RGBA32, false);
            discoveryTexture.filterMode = FilterMode.Bilinear;
            discoveryTexture.wrapMode = TextureWrapMode.Clamp;
            
            discoveryPixels = new Color32[discoveryResolution * discoveryResolution];
            for (int i = 0; i < discoveryPixels.Length; i++)
            {
                discoveryPixels[i] = new Color32(0, 0, 0, 255); // Fully black/hidden
            }
            discoveryTexture.SetPixels32(discoveryPixels);
            discoveryTexture.Apply();
        }

        if (fogImage != null)
        {
            fogImage.texture = discoveryTexture;
        }
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
            mPressed = Input.GetKeyDown(KeyCode.M);
            escPressed = Input.GetKeyDown(KeyCode.Escape);
        }

        if (mPressed)
        {
            ToggleMap();
        }
        else if (isMapOpen && escPressed)
        {
            ToggleMap();
        }

        // Always update discovery even if map is closed
        UpdateDiscovery();

        if (isMapOpen)
        {
            HandleZoom();
            UpdatePlayerMarker();
        }
    }

    private void HandleZoom()
    {
        float scroll = 0;
        if (Mouse.current != null)
        {
            scroll = Mouse.current.scroll.ReadValue().y;
        }
        else
        {
            scroll = Input.GetAxis("Mouse ScrollWheel");
        }

        if (scroll != 0)
        {
            currentZoom = Mathf.Clamp(currentZoom + (scroll > 0 ? zoomSpeed : -zoomSpeed), minZoom, maxZoom);
            if (mapImage != null)
            {
                mapImage.localScale = new Vector3(currentZoom, currentZoom, 1f);
            }
        }
    }

    private void UpdateDiscovery()
    {
        if (playerTransform == null) return;

        Vector2 worldPos = playerTransform.position;
        float normX = (worldPos.x - worldMin.x) / worldSize.x;
        float normY = (worldPos.y - worldMin.y) / worldSize.y;

        if (normX < 0 || normX > 1 || normY < 0 || normY > 1) return;

        int centerX = (int)(normX * discoveryResolution);
        int centerY = (int)(normY * discoveryResolution);
        int radiusPx = (int)((revealRadius / worldSize.x) * discoveryResolution);
        if (radiusPx < 2) radiusPx = 2;

        bool changed = false;
        for (int y = centerY - radiusPx; y <= centerY + radiusPx; y++)
        {
            for (int x = centerX - radiusPx; x <= centerX + radiusPx; x++)
            {
                if (x >= 0 && x < discoveryResolution && y >= 0 && y < discoveryResolution)
                {
                    float dist = Vector2.Distance(new Vector2(x, y), new Vector2(centerX, centerY));
                    if (dist <= radiusPx)
                    {
                        int index = y * discoveryResolution + x;
                        if (discoveryPixels[index].a > 0)
                        {
                            // Fade out based on distance for smoother edges
                            byte newAlpha = (byte)Mathf.Min(discoveryPixels[index].a, (byte)((dist / radiusPx) * 150));
                            if (dist < radiusPx * 0.5f) newAlpha = 0;
                            
                            if (discoveryPixels[index].a != newAlpha)
                            {
                                discoveryPixels[index].a = newAlpha;
                                changed = true;
                            }
                        }
                    }
                }
            }
        }

        if (changed)
        {
            discoveryTexture.SetPixels32(discoveryPixels);
            discoveryTexture.Apply();
        }
    }

    public void ToggleMap()
    {
        if (mapPanel == null) return;

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

    private void UpdatePlayerMarker()
    {
        if (playerTransform == null || playerMarker == null || mapImage == null) return;

        Vector2 worldPos = playerTransform.position;
        float normX = (worldPos.x - worldMin.x) / worldSize.x;
        float normY = (worldPos.y - worldMin.y) / worldSize.y;

        Vector2 mapSize = mapImage.rect.size;
        float uiX = (normX - 0.5f) * mapSize.x; 
        float uiY = (normY - 0.5f) * mapSize.y;

        playerMarker.anchoredPosition = new Vector2(uiX + markerOffset.x, uiY + markerOffset.y);
        playerMarker.localRotation = Quaternion.Euler(0, 0, playerTransform.eulerAngles.z);
    }
}