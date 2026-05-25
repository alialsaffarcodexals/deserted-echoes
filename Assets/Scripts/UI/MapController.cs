using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

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
    [SerializeField] private float markerSize = 30f; 
    [SerializeField] private Vector2 mapImageOffset = Vector2.zero;

    [Header("Zoom Settings")]
    [SerializeField] private float minZoom = 0.5f;
    [SerializeField] private float maxZoom = 4.0f;
    [SerializeField] private float zoomSpeed = 0.2f;

    [Header("Map Open Settings")]
    [SerializeField] private float startZoom = 1f;
    [SerializeField] private Vector2 startPosition = Vector2.zero;

    [Header("Panning Settings")]
    [SerializeField] private float panningSpeed = 500f;

    [Header("Discovery Settings")]
    [SerializeField] private int discoveryResolution = 128;
    [SerializeField] private float revealWidth = 5f;
    [SerializeField] private float revealHeight = 5f;
    [SerializeField] private Vector2 fogRevealOffset = Vector2.zero;

    private float currentZoom = 1.0f;
    private Vector2 currentPan = Vector2.zero;
    private bool isMapOpen = false;
    private bool hasBeenOpened = false;
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
                discoveryPixels[i] = new Color32(0, 0, 0, 255);

            LoadFogState();

            discoveryTexture.SetPixels32(discoveryPixels);
            discoveryTexture.Apply();
        }

        if (fogImage != null)
            fogImage.texture = discoveryTexture;
    }

    private void LoadFogState()
    {
        if (SaveManager.Instance == null || discoveryPixels == null) return;
        string scene = SceneManager.GetActiveScene().name;
        byte[] alphas = SaveManager.Instance.GetFogAlphas(scene);
        if (alphas == null || alphas.Length != discoveryPixels.Length) return;
        for (int i = 0; i < discoveryPixels.Length; i++)
            discoveryPixels[i].a = alphas[i];
    }

    private void SaveFogState()
    {
        if (Instance != this || SaveManager.Instance == null || discoveryPixels == null) return;
        string scene = SceneManager.GetActiveScene().name;
        byte[] alphas = new byte[discoveryPixels.Length];
        for (int i = 0; i < discoveryPixels.Length; i++)
            alphas[i] = discoveryPixels[i].a;
        SaveManager.Instance.SetFogAlphas(scene, alphas);
    }

    private void OnApplicationQuit()
    {
        SaveFogState();
    }

    private void OnDestroy()
    {
        SaveFogState();
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
            HandlePanning();
            HandleZoom();
            UpdatePlayerMarker();
        }
    }

    private void HandlePanning()
    {
        Vector2 move = Vector2.zero;
        if (Keyboard.current != null)
        {
            if (Keyboard.current.upArrowKey.isPressed) move.y += 1;
            if (Keyboard.current.downArrowKey.isPressed) move.y -= 1;
            if (Keyboard.current.leftArrowKey.isPressed) move.x -= 1;
            if (Keyboard.current.rightArrowKey.isPressed) move.x += 1;
        }
        else
        {
            if (Input.GetKey(KeyCode.UpArrow)) move.y += 1;
            if (Input.GetKey(KeyCode.DownArrow)) move.y -= 1;
            if (Input.GetKey(KeyCode.LeftArrow)) move.x -= 1;
            if (Input.GetKey(KeyCode.RightArrow)) move.x += 1;
        }

        if (move != Vector2.zero)
        {
            // Pan the map in the opposite direction of the arrow key to move the "view" in that direction
            currentPan -= move.normalized * panningSpeed * Time.unscaledDeltaTime;
            ClampPan();
        }
    }

    private void ClampPan()
    {
        if (mapImage == null || mapPanel == null) return;

        RectTransform panelRect = mapPanel.GetComponent<RectTransform>();
        Vector2 panelSize = panelRect.rect.size;
        Vector2 mapVisualSize = mapImage.rect.size * currentZoom;

        float maxPanX = Mathf.Max(0, (mapVisualSize.x - panelSize.x) / 2f);
        float maxPanY = Mathf.Max(0, (mapVisualSize.y - panelSize.y) / 2f);

        currentPan.x = Mathf.Clamp(currentPan.x, -maxPanX, maxPanX);
        currentPan.y = Mathf.Clamp(currentPan.y, -maxPanY, maxPanY);
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
            ClampPan(); // Ensure pan stays within bounds after zoom
        }
    }

    private void UpdateDiscovery()
    {
        if (playerTransform == null) return;

        Vector2 worldPos = playerTransform.position;
        float normX = (worldPos.x - worldMin.x) / worldSize.x;
        float normY = (worldPos.y - worldMin.y) / worldSize.y;

        if (normX < 0 || normX > 1 || normY < 0 || normY > 1) return;

        Vector2 mapSize = mapImage != null ? mapImage.rect.size : new Vector2(1, 1);
        float offsetNormX = mapSize.x > 0 ? (markerOffset.x + fogRevealOffset.x) / mapSize.x : 0f;
        float offsetNormY = mapSize.y > 0 ? (markerOffset.y + fogRevealOffset.y) / mapSize.y : 0f;
        int centerX = (int)((normX + offsetNormX) * discoveryResolution);
        int centerY = (int)((normY + offsetNormY) * discoveryResolution);
        int radiusX = Mathf.Max(2, (int)((revealWidth  / worldSize.x) * discoveryResolution));
        int radiusY = Mathf.Max(2, (int)((revealHeight / worldSize.y) * discoveryResolution));

        bool changed = false;
        for (int y = centerY - radiusY; y <= centerY + radiusY; y++)
        {
            for (int x = centerX - radiusX; x <= centerX + radiusX; x++)
            {
                if (x >= 0 && x < discoveryResolution && y >= 0 && y < discoveryResolution)
                {
                    float dx = (x - centerX) / (float)radiusX;
                    float dy = (y - centerY) / (float)radiusY;
                    float ellipseDist = Mathf.Sqrt(dx * dx + dy * dy);
                    if (ellipseDist <= 1f)
                    {
                        int index = y * discoveryResolution + x;
                        if (discoveryPixels[index].a > 0)
                        {
                            byte newAlpha = (byte)Mathf.Min(discoveryPixels[index].a, (byte)(ellipseDist * 150f));
                            if (ellipseDist < 0.5f) newAlpha = 0;
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
            if (!hasBeenOpened)
            {
                currentZoom = Mathf.Clamp(startZoom, minZoom, maxZoom);
                if (mapImage != null)
                    mapImage.localScale = new Vector3(currentZoom, currentZoom, 1f);
                if (startPosition != Vector2.zero)
                {
                    currentPan = startPosition;
                    ClampPan();
                }
                else
                {
                    CenterOnPlayer();
                }
                hasBeenOpened = true;
            }
            UpdatePlayerMarker();
        }
        else
        {
            Time.timeScale = 1f;
        }
    }

    private void CenterOnPlayer()
    {
        if (playerTransform == null || mapImage == null) return;

        Vector2 worldPos = playerTransform.position;
        float normX = (worldPos.x - worldMin.x) / worldSize.x;
        float normY = (worldPos.y - worldMin.y) / worldSize.y;

        Vector2 mapSize = mapImage.rect.size;
        float uiX = (normX - 0.5f) * mapSize.x; 
        float uiY = (normY - 0.5f) * mapSize.y;

        currentPan = -new Vector2(uiX + markerOffset.x, uiY + markerOffset.y) * currentZoom;
        ClampPan();
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

        mapImage.anchoredPosition = currentPan + mapImageOffset;
        playerMarker.anchoredPosition = new Vector2(uiX + markerOffset.x, uiY + markerOffset.y);
        playerMarker.sizeDelta = new Vector2(markerSize, markerSize);
        playerMarker.localRotation = Quaternion.Euler(0, 0, playerTransform.eulerAngles.z);
    }
}


