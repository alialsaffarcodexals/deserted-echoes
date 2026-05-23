using UnityEngine;
using UnityEngine.UI;

public class MinimapController : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private RectTransform mapContent;
    [SerializeField] private RectTransform playerMarker;

    [Header("Level Configuration")]
    [SerializeField] private Vector2 worldMin = new Vector2(-51f, -54f);
    [SerializeField] private Vector2 worldSize = new Vector2(88f, 63f);

    [Header("Discovery Settings")]
    [SerializeField] private int discoveryResolution = 128;
    [SerializeField] private float revealWidth = 5f;
    [SerializeField] private float revealHeight = 5f;
    [SerializeField] private Vector2 fogRevealOffset = Vector2.zero;

    private Transform playerTransform;
    private RawImage fogOverlay;
    private Texture2D discoveryTexture;
    private Color32[] discoveryPixels;

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
        InitializeDiscoveryTexture();
        CreateFogOverlay();
    }

    private void InitializeDiscoveryTexture()
    {
        discoveryTexture = new Texture2D(discoveryResolution, discoveryResolution, TextureFormat.RGBA32, false);
        discoveryTexture.filterMode = FilterMode.Bilinear;
        discoveryTexture.wrapMode = TextureWrapMode.Clamp;

        discoveryPixels = new Color32[discoveryResolution * discoveryResolution];
        for (int i = 0; i < discoveryPixels.Length; i++)
            discoveryPixels[i] = new Color32(0, 0, 0, 255);

        discoveryTexture.SetPixels32(discoveryPixels);
        discoveryTexture.Apply();
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

        fogOverlay.texture = discoveryTexture;
    }

    private void Update()
    {
        if (playerTransform == null)
        {
            playerTransform = GameObject.FindGameObjectWithTag("Player")?.transform;
            if (playerTransform == null) return;
        }

        UpdateDiscovery();
        UpdateMapPosition();
    }

    private void UpdateDiscovery()
    {
        if (discoveryTexture == null || discoveryPixels == null) return;

        Vector2 worldPos = playerTransform.position;
        float normX = (worldPos.x - worldMin.x) / worldSize.x;
        float normY = (worldPos.y - worldMin.y) / worldSize.y;

        if (normX < 0 || normX > 1 || normY < 0 || normY > 1) return;

        Vector2 mapSize = mapContent != null ? mapContent.rect.size : new Vector2(1, 1);
        float offsetNormX = mapSize.x > 0 ? fogRevealOffset.x / mapSize.x : 0f;
        float offsetNormY = mapSize.y > 0 ? fogRevealOffset.y / mapSize.y : 0f;

        int centerX = (int)((normX + offsetNormX) * discoveryResolution);
        int centerY = (int)((normY + offsetNormY) * discoveryResolution);
        int radiusX = Mathf.Max(2, (int)((revealWidth  / worldSize.x) * discoveryResolution));
        int radiusY = Mathf.Max(2, (int)((revealHeight / worldSize.y) * discoveryResolution));

        bool changed = false;
        for (int y = centerY - radiusY; y <= centerY + radiusY; y++)
        {
            for (int x = centerX - radiusX; x <= centerX + radiusX; x++)
            {
                if (x < 0 || x >= discoveryResolution || y < 0 || y >= discoveryResolution) continue;

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

        if (changed)
        {
            discoveryTexture.SetPixels32(discoveryPixels);
            discoveryTexture.Apply();
        }
    }

    private void UpdateMapPosition()
    {
        if (mapContent == null) return;

        Vector2 worldPos = playerTransform.position;
        float normX = (worldPos.x - worldMin.x) / worldSize.x;
        float normY = (worldPos.y - worldMin.y) / worldSize.y;

        Vector2 mapSize = mapContent.rect.size;
        float uiX = -(normX - 0.5f) * mapSize.x;
        float uiY = -(normY - 0.5f) * mapSize.y;

        mapContent.anchoredPosition = new Vector2(uiX, uiY);

        if (playerMarker != null)
            playerMarker.localRotation = Quaternion.Euler(0, 0, -playerTransform.eulerAngles.z);
    }
}
