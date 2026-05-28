using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MinimapController : MonoBehaviour
{
    public static MinimapController Instance { get; private set; }
    public Texture2D FogTexture => fogTexture;
    public float FogWorldSize => fogWorldSize;
    public Vector2 FogWorldCenter => fogWorldCenter;

    [Header("Camera")]
    [SerializeField] private Camera minimapCamera;
    [SerializeField] private float heightOffset = -10f;

    [Header("Fog Settings")]
    [SerializeField] private RectTransform mapContent;
    [SerializeField] private int fogResolution = 256;
    [SerializeField] private float fogWorldSize = 120f;
    [SerializeField] private Vector2 fogWorldCenter = Vector2.zero;
    [SerializeField] private float revealRadius = 6f;

    public void SetFogArea(Vector2 center, float size)
    {
        fogWorldCenter = center;
        fogWorldSize = size;
    }

    private Transform playerTransform;
    private RawImage fogOverlay;
    private Texture2D fogTexture;
    private Color32[] fogPixels;

    private void Awake()
    {
        if (Instance == null) Instance = this;
    }

    private void OnDestroy()
    {
        if (Instance == this) Instance = null;
        if (fogPixels == null) return;
        byte[] alphas = new byte[fogPixels.Length];
        for (int i = 0; i < fogPixels.Length; i++)
            alphas[i] = fogPixels[i].a;
        FogStore.Set(SceneManager.GetActiveScene().name, alphas);
    }

    private void Start()
    {
        playerTransform = GameObject.FindGameObjectWithTag("Player")?.transform;
        InitializeFog();
        CreateFogOverlay();
    }

    private void InitializeFog()
    {
        fogTexture = new Texture2D(fogResolution, fogResolution, TextureFormat.RGBA32, false);
        fogTexture.filterMode = FilterMode.Bilinear;
        fogTexture.wrapMode = TextureWrapMode.Clamp;

        fogPixels = new Color32[fogResolution * fogResolution];
        for (int i = 0; i < fogPixels.Length; i++)
            fogPixels[i] = new Color32(0, 0, 0, 255);

        fogTexture.SetPixels32(fogPixels);
        fogTexture.Apply();

        byte[] saved = FogStore.Get(SceneManager.GetActiveScene().name);
        if (saved != null && saved.Length == fogPixels.Length)
        {
            for (int i = 0; i < fogPixels.Length; i++)
                fogPixels[i].a = saved[i];
            fogTexture.SetPixels32(fogPixels);
            fogTexture.Apply();
        }
    }

    private void CreateFogOverlay()
    {
        if (mapContent == null) return;

        var go = new GameObject("MinimapFog");
        go.transform.SetParent(mapContent, false);

        fogOverlay = go.AddComponent<RawImage>();
        var rt = fogOverlay.rectTransform;
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
        fogOverlay.raycastTarget = false;
        fogOverlay.texture = fogTexture;
    }

    private void LateUpdate()
    {
        if (playerTransform == null)
        {
            playerTransform = GameObject.FindGameObjectWithTag("Player")?.transform;
            if (playerTransform == null) return;
        }

        if (minimapCamera != null)
        {
            Vector3 playerPos = playerTransform.position;
            minimapCamera.transform.position = new Vector3(playerPos.x, playerPos.y, heightOffset);
        }

        UpdateFogReveal();
        UpdateFogOverlayUV();
    }

    private void UpdateFogReveal()
    {
        if (fogTexture == null) return;

        Vector2 worldPos = playerTransform.position;
        float halfSize = fogWorldSize * 0.5f;
        float normX = (worldPos.x - (fogWorldCenter.x - halfSize)) / fogWorldSize;
        float normY = (worldPos.y - (fogWorldCenter.y - halfSize)) / fogWorldSize;

        if (normX < 0 || normX > 1 || normY < 0 || normY > 1) return;

        int centerX = (int)(normX * fogResolution);
        int centerY = (int)(normY * fogResolution);
        int radius = Mathf.Max(2, (int)((revealRadius / fogWorldSize) * fogResolution));

        bool changed = false;
        for (int y = centerY - radius; y <= centerY + radius; y++)
        {
            for (int x = centerX - radius; x <= centerX + radius; x++)
            {
                if (x < 0 || x >= fogResolution || y < 0 || y >= fogResolution) continue;

                float dx = (x - centerX) / (float)radius;
                float dy = (y - centerY) / (float)radius;
                float dist = Mathf.Sqrt(dx * dx + dy * dy);
                if (dist > 1f) continue;

                int index = y * fogResolution + x;
                byte newAlpha = 0;

                if (fogPixels[index].a > newAlpha)
                {
                    fogPixels[index].a = newAlpha;
                    changed = true;
                }
            }
        }

        if (changed)
        {
            fogTexture.SetPixels32(fogPixels);
            fogTexture.Apply();
        }
    }

    private void UpdateFogOverlayUV()
    {
        if (fogOverlay == null || minimapCamera == null) return;

        Vector2 camPos = minimapCamera.transform.position;
        float camHalfHeight = minimapCamera.orthographicSize;
        float camHalfWidth = camHalfHeight * minimapCamera.aspect;

        float halfSize = fogWorldSize * 0.5f;
        float uMin = (camPos.x - camHalfWidth - (fogWorldCenter.x - halfSize)) / fogWorldSize;
        float vMin = (camPos.y - camHalfHeight - (fogWorldCenter.y - halfSize)) / fogWorldSize;
        float uSize = (camHalfWidth * 2f) / fogWorldSize;
        float vSize = (camHalfHeight * 2f) / fogWorldSize;

        fogOverlay.uvRect = new Rect(uMin, vMin, uSize, vSize);
    }
}