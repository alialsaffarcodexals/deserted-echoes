// ─────────────────────────────────────────────────────────────
// Level02Setup.cs  (Editor-only — lives in Assets/Editor/)
// Deserted Echoes | IT8101 Games Development | Group 3
// Run once: Tools ▶ Deserted Echoes ▶ Setup Level-02
// ─────────────────────────────────────────────────────────────
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class Level02Setup
{
    [MenuItem("Tools/Deserted Echoes/Setup Level-02 (Enemies + HUD)")]
    public static void Run()
    {
        Scene scene = SceneManager.GetActiveScene();
        if (!scene.name.Equals("level-02", System.StringComparison.OrdinalIgnoreCase))
        {
            Debug.LogError("[Level02Setup] Open level-02 first, then run this tool.");
            return;
        }

        int enemyLayer = LayerMask.NameToLayer("Enemy");
        if (enemyLayer < 0) enemyLayer = 6;

        // Player spawn in level-02 is approx (102.23, 19.38)

        // ── HUD Canvas ────────────────────────────────────────
        Place("Assets/Prefabs/UI Items/Canvas.prefab",
              "Canvas", Vector3.zero, -1);

        // ── Mobs ──────────────────────────────────────────────
        Place("Assets/Prefabs/Enemies/Mobs/Zombie/Zombie1.prefab",
              "Zombie1",  new Vector3(107f, 19.38f, 0f), enemyLayer);

        Place("Assets/Prefabs/Enemies/Mobs/Orc/Orc1.prefab",
              "Orc1",     new Vector3(102f, 14f,    0f), enemyLayer);

        // ── Boss ──────────────────────────────────────────────
        Place("Assets/Prefabs/Enemies/Bosses/Gnoll/Gnoll1.prefab",
              "Gnoll1",   new Vector3(115f, 19.38f, 0f), enemyLayer);

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        Debug.Log("[Level02Setup] Done — scene saved.");
    }

    static void Place(string prefabPath, string goName, Vector3 pos, int layer)
    {
        if (GameObject.Find(goName) != null)
        {
            Debug.Log($"[Level02Setup] {goName} already exists — skipped.");
            return;
        }

        GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
        if (prefab == null)
        {
            Debug.LogError($"[Level02Setup] Prefab not found: {prefabPath}");
            return;
        }

        GameObject go = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
        go.transform.position = pos;
        if (layer >= 0) go.layer = layer;
        Undo.RegisterCreatedObjectUndo(go, $"Add {goName}");
        Debug.Log($"[Level02Setup] Placed {goName} at {pos}");
    }
}
