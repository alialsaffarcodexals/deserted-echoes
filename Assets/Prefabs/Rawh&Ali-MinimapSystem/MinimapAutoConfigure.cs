using UnityEngine;
using UnityEngine.Tilemaps;

/// Auto configures the MinimapSystem prefab for any level.
/// Finds the largest tilemap in the scene and sets the fog area to cover it.
public class MinimapAutoConfigure : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private MinimapController minimapController;

    [Header("Auto-Configure Settings")]
    [Tooltip("If true, finds the largest tilemap in the scene and configures fog to cover it.")]
    [SerializeField] private bool autoFindTilemap = true;

    [Tooltip("Extra padding around the level bounds (in world units).")]
    [SerializeField] private float padding = 10f;

    [Header("Manual Override (used if autoFindTilemap is false)")]
    [SerializeField] private Vector2 manualFogCenter = Vector2.zero;
    [SerializeField] private float manualFogSize = 120f;

    private void Awake()
    {
        if (minimapController == null)
            minimapController = GetComponentInChildren<MinimapController>();

        if (minimapController == null)
        {
            Debug.LogWarning("[MinimapAutoConfigure] No MinimapController found in children. Skipping auto-configure.");
            return;
        }

        if (autoFindTilemap)
            AutoConfigureFromTilemap();
        else
            ApplyManual();
    }

    private void AutoConfigureFromTilemap()
    {
        Tilemap[] tilemaps = FindObjectsByType<Tilemap>(FindObjectsSortMode.None);
        if (tilemaps == null || tilemaps.Length == 0)
        {
            Debug.LogWarning("[MinimapAutoConfigure] No tilemaps found. Using manual fog values.");
            ApplyManual();
            return;
        }

        // Find combined bounds of all tilemaps
        Bounds combined = new Bounds();
        bool initialized = false;

        foreach (var tilemap in tilemaps)
        {
            tilemap.CompressBounds();
            BoundsInt b = tilemap.cellBounds;
            if (b.size.x == 0 || b.size.y == 0) continue;

            Vector3 min = tilemap.CellToWorld(new Vector3Int(b.xMin, b.yMin, 0));
            Vector3 max = tilemap.CellToWorld(new Vector3Int(b.xMax, b.yMax, 0));
            Bounds tmBounds = new Bounds((min + max) * 0.5f, max - min);

            if (!initialized) { combined = tmBounds; initialized = true; }
            else combined.Encapsulate(tmBounds);
        }

        if (!initialized)
        {
            ApplyManual();
            return;
        }

        Vector2 center = combined.center;
        float size = Mathf.Max(combined.size.x, combined.size.y) + (padding * 2f);

        minimapController.SetFogArea(center, size);
        Debug.Log($"[MinimapAutoConfigure] Configured fog: center={center}, size={size}");
    }

    private void ApplyManual()
    {
        if (minimapController != null)
            minimapController.SetFogArea(manualFogCenter, manualFogSize);
    }
}