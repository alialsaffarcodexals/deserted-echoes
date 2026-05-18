// ─────────────────────────────────────────────────────────────
// DifficultySystemSetup.cs  (Editor-only — lives in Assets/Editor/)
// Deserted Echoes | IT8101 Games Development | Group 3
// Run once: Tools ▶ Deserted Echoes ▶ Setup Difficulty System
// ─────────────────────────────────────────────────────────────

using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

public static class DifficultySystemSetup
{
    private const string EnemiesRoot = "Assets/Prefabs/Enemies";

    [MenuItem("Tools/Deserted Echoes/Setup Difficulty System")]
    public static void Run()
    {
        if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
            return;

        EditorSceneManager.OpenScene("Assets/Scenes/main-menu.unity");

        GameManager gameManager = Object.FindObjectOfType<GameManager>();
        if (gameManager == null)
        {
            Debug.LogError("[DifficultySystemSetup] GameManager not found in main-menu scene.");
            return;
        }

        EnemyDifficultyApplier applier = gameManager.GetComponent<EnemyDifficultyApplier>();
        if (applier == null)
            applier = gameManager.gameObject.AddComponent<EnemyDifficultyApplier>();

        applier.SetEntries(BuildEntries());
        EditorUtility.SetDirty(applier);
        EditorSceneManager.MarkSceneDirty(gameManager.gameObject.scene);
        EditorSceneManager.SaveOpenScenes();
        Debug.Log("[DifficultySystemSetup] Done — difficulty system ready on GameManager.");
    }

    private static List<EnemyDifficultyApplier.VariantEntry> BuildEntries()
    {
        List<EnemyDifficultyApplier.VariantEntry> entries = new List<EnemyDifficultyApplier.VariantEntry>();
        HashSet<string> processedKeys = new HashSet<string>();

        string[] variantOneGuids = AssetDatabase.FindAssets("t:Prefab", new[] { EnemiesRoot });
        foreach (string guid in variantOneGuids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            string fileName = System.IO.Path.GetFileNameWithoutExtension(path);
            if (!fileName.EndsWith("1"))
                continue;

            string enemyKey = fileName.Substring(0, fileName.Length - 1);
            if (!processedKeys.Add(enemyKey))
                continue;

            string folder = System.IO.Path.GetDirectoryName(path)?.Replace('\\', '/');
            if (string.IsNullOrEmpty(folder))
                continue;

            EnemyDifficultyApplier.VariantEntry entry = new EnemyDifficultyApplier.VariantEntry
            {
                enemyKey = enemyKey,
                variants = new GameObject[3]
            };

            for (int variant = 1; variant <= 3; variant++)
            {
                string variantPath = $"{folder}/{enemyKey}{variant}.prefab";
                entry.variants[variant - 1] = AssetDatabase.LoadAssetAtPath<GameObject>(variantPath);
            }

            entries.Add(entry);
        }

        return entries;
    }
}
