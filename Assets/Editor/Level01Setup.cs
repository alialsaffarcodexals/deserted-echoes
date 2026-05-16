// ─────────────────────────────────────────────────────────────
// Level01Setup.cs  (Editor-only — lives in Assets/Editor/)
// Deserted Echoes | IT8101 Games Development | Group 3
// Run once: Tools ▶ Deserted Echoes ▶ Setup Level-01 (Enemies + HUD)
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
        // ── 1. Make sure level-01 is the active scene ─────────
        Scene scene = SceneManager.GetActiveScene();
        if (!scene.name.Equals("level-01", System.StringComparison.OrdinalIgnoreCase))
        {
            Debug.LogError("[Level01Setup] Please open the level-01 scene first, then run this tool.");
            return;
        }

        // ── 2. Skip if already set up ─────────────────────────
        bool hasGoblin  = GameObject.Find("Goblin1")  != null;
        bool hasCanvas  = GameObject.Find("Canvas")   != null;

        if (hasGoblin && hasCanvas)
        {
            Debug.Log("[Level01Setup] Already set up — Goblin1 and Canvas are both present. Nothing to do.");
            return;
        }

        // ── 3. Load prefabs ───────────────────────────────────
        const string goblinPath = "Assets/Prefabs/Enemies/Mobs/Goblin/Goblin1.prefab";
        const string canvasPath = "Assets/Prefabs/UI Items/Canvas.prefab";

        GameObject goblinPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(goblinPath);
        GameObject canvasPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(canvasPath);

        if (goblinPrefab == null)
        {
            Debug.LogError($"[Level01Setup] Cannot find prefab at: {goblinPath}");
            return;
        }
        if (canvasPrefab == null)
        {
            Debug.LogError($"[Level01Setup] Cannot find prefab at: {canvasPath}");
            return;
        }

        // ── 4. Instantiate Goblin1 (3 units east of player) ───
        if (!hasGoblin)
        {
            GameObject goblin = (GameObject)PrefabUtility.InstantiatePrefab(goblinPrefab);
            goblin.transform.position = new Vector3(14f, -45.23f, 0f);
            goblin.layer = LayerMask.NameToLayer("Enemy");
            if (goblin.layer < 0)
            {
                goblin.layer = 6; // fallback to layer index 6
                Debug.LogWarning("[Level01Setup] 'Enemy' layer not found by name — using layer 6.");
            }
            Undo.RegisterCreatedObjectUndo(goblin, "Add Goblin1");
            Debug.Log($"[Level01Setup] Goblin1 instantiated at {goblin.transform.position}, layer {goblin.layer}.");
        }

        // ── 5. Instantiate Canvas (HUD) ───────────────────────
        if (!hasCanvas)
        {
            GameObject canvas = (GameObject)PrefabUtility.InstantiatePrefab(canvasPrefab);
            Undo.RegisterCreatedObjectUndo(canvas, "Add HUD Canvas");
            Debug.Log("[Level01Setup] HUD Canvas instantiated.");
        }

        // ── 6. Mark scene dirty and save ─────────────────────
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        Debug.Log("[Level01Setup] Scene saved. Setup complete!");
    }
}
