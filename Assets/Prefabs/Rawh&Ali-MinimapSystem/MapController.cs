using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class MapController : MonoBehaviour
{
    public static MapController Instance { get; private set; }
    public bool IsMapOpen => isMapOpen;

    [Header("UI References")]
    [SerializeField] private GameObject mapPanel;
    [SerializeField] private RectTransform mapImageRect; // the RawImage rect for the big map

    [Header("Camera")]
    [SerializeField] private Camera bigMapCamera;
    [SerializeField] private float heightOffset = -10f;

    [Header("Zoom Settings")]
    [SerializeField] private float minZoom = 10f;
    [SerializeField] private float maxZoom = 60f;
    [SerializeField] private float zoomSpeed = 2f;
    [SerializeField] private float startZoom = 30f;

    [Header("Panning Settings")]
    [SerializeField] private float panningSpeed = 20f;

    private bool isMapOpen = false;
    private Transform playerTransform;
    private Vector2 panOffset = Vector2.zero;
    private RawImage bigMapFogOverlay;

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
        if (bigMapCamera != null)
            bigMapCamera.orthographicSize = startZoom;

        CreateBigMapFogOverlay();
    }

    private void CreateBigMapFogOverlay()
    {
        if (mapImageRect == null) return;

        var go = new GameObject("BigMapFog");
        go.transform.SetParent(mapImageRect, false);

        bigMapFogOverlay = go.AddComponent<RawImage>();
        var rt = bigMapFogOverlay.rectTransform;
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
        bigMapFogOverlay.raycastTarget = false;

        // Start fully transparent until we have a fog texture, prevents white flash
        bigMapFogOverlay.color = new Color(1, 1, 1, 0);
        bigMapFogOverlay.texture = null;
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

        if (mPressed) ToggleMap();
        else if (isMapOpen && escPressed) ToggleMap();

        if (isMapOpen)
        {
            HandlePanning();
            HandleZoom();
            UpdateBigMapCamera();
            UpdateBigMapFogUV();
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
            panOffset += move.normalized * panningSpeed * Time.unscaledDeltaTime;
    }

    private void HandleZoom()
    {
        float scroll = 0;
        if (Mouse.current != null)
            scroll = Mouse.current.scroll.ReadValue().y;
        else
            scroll = Input.GetAxis("Mouse ScrollWheel");

        if (Mathf.Abs(scroll) > 0.01f && bigMapCamera != null)
        {
            // Use sign so big scroll deltas don't jump too far
            float direction = scroll > 0 ? -1f : 1f;
            bigMapCamera.orthographicSize = Mathf.Clamp(
                bigMapCamera.orthographicSize + (direction * zoomSpeed), minZoom, maxZoom);
        }
    }

    private void UpdateBigMapCamera()
    {
        if (bigMapCamera == null || playerTransform == null) return;

        Vector3 playerPos = playerTransform.position;
        bigMapCamera.transform.position = new Vector3(
            playerPos.x + panOffset.x,
            playerPos.y + panOffset.y,
            heightOffset);
    }

    private void UpdateBigMapFogUV()
    {
        if (bigMapFogOverlay == null || bigMapCamera == null || MinimapController.Instance == null) return;

        // Lazy-assign the fog texture once MinimapController is ready
        if (bigMapFogOverlay.texture == null && MinimapController.Instance.FogTexture != null)
        {
            bigMapFogOverlay.texture = MinimapController.Instance.FogTexture;
            bigMapFogOverlay.color = Color.white; // make it visible again
        }

        if (bigMapFogOverlay.texture == null) return;

        Vector2 camPos = bigMapCamera.transform.position;
        float camHalfHeight = bigMapCamera.orthographicSize;
        float camHalfWidth = camHalfHeight * bigMapCamera.aspect;

        float fogSize = MinimapController.Instance.FogWorldSize;
        Vector2 fogCenter = MinimapController.Instance.FogWorldCenter;
        float halfSize = fogSize * 0.5f;

        float uMin = (camPos.x - camHalfWidth - (fogCenter.x - halfSize)) / fogSize;
        float vMin = (camPos.y - camHalfHeight - (fogCenter.y - halfSize)) / fogSize;
        float uSize = (camHalfWidth * 2f) / fogSize;
        float vSize = (camHalfHeight * 2f) / fogSize;

        bigMapFogOverlay.uvRect = new Rect(uMin, vMin, uSize, vSize);
    }

    public void ToggleMap()
    {
        if (mapPanel == null) return;

        isMapOpen = !isMapOpen;
        mapPanel.SetActive(isMapOpen);

        if (isMapOpen)
        {
            Time.timeScale = 0f;
            panOffset = Vector2.zero;
            UpdateBigMapCamera();
        }
        else
        {
            Time.timeScale = 1f;
        }
    }
}