// ─────────────────────────────────────────────────────────────
// Level01Setup.cs  (Editor-only — lives in Assets/Editor/)
// Deserted Echoes | IT8101 Games Development | Group 3
// Run once: Tools ▶ Deserted Echoes ▶ Setup Level-01
// ─────────────────────────────────────────────────────────────
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class Level01Setup
{
    [MenuItem("Tools/Deserted Echoes/Setup Level-01 (Enemies + HUD)")]
    public static void Run()
    {
        Scene scene = SceneManager.GetActiveScene();
        if (!scene.name.Equals("level-01", System.StringComparison.OrdinalIgnoreCase))
        {
            Debug.LogError("[Level01Setup] Open level-01 first, then run this tool.");
            return;
        }

        int enemyLayer = LayerMask.NameToLayer("Enemy");
        if (enemyLayer < 0) enemyLayer = 6;

        // ── HUD Canvas ────────────────────────────────────────
        Place("Assets/Prefabs/UI Items/Canvas.prefab",
              "Canvas", Vector3.zero, -1, false);

        // ── Mobs ──────────────────────────────────────────────
        Place("Assets/Prefabs/Enemies/Mobs/Goblin/Goblin1.prefab",
              "Goblin1",    new Vector3(14f, -45.23f, 0f), enemyLayer);

        Place("Assets/Prefabs/Enemies/Mobs/Skeleton/Skeleton1.prefab",
              "Skeleton1",  new Vector3(7f,  -45.23f, 0f), enemyLayer);

        Place("Assets/Prefabs/Enemies/Mobs/GiantRat/GiantRat1.prefab",
              "GiantRat1",  new Vector3(11f, -50f,    0f), enemyLayer);

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        Debug.Log("[Level01Setup] Done — scene saved.");
    }

    static void Place(string prefabPath, string goName, Vector3 pos, int layer, bool checkDuplicate = true)
    {
        if (checkDuplicate && GameObject.Find(goName) != null)
        {
            Debug.Log($"[Level01Setup] {goName} already exists — skipped.");
            return;
        }

        GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
        if (prefab == null)
        {
            Debug.LogError($"[Level01Setup] Prefab not found: {prefabPath}");
            return;
        }

        GameObject go = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
        go.transform.position = pos;
        if (layer >= 0) go.layer = layer;
        Undo.RegisterCreatedObjectUndo(go, $"Add {goName}");
        Debug.Log($"[Level01Setup] Placed {goName} at {pos}");
    }
}
