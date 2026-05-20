// ─────────────────────────────────────────────────────────────
// EnemyDifficultyApplier.cs
// Deserted Echoes | IT8101 Games Development | Group 3
// Description: Swaps placed enemies to the prefab variant that matches
//              the player's chosen difficulty (visual + stats).
// ─────────────────────────────────────────────────────────────

using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.SceneManagement;

public class EnemyDifficultyApplier : MonoBehaviour
{
    [Serializable]
    public class VariantEntry
    {
        public string enemyKey;
        public GameObject[] variants = new GameObject[3];
    }

    private static readonly Regex NamePattern = new Regex(@"^(.+?)([123])(?:\s|\(|$)", RegexOptions.Compiled);

    [SerializeField] private List<VariantEntry> entries = new List<VariantEntry>();

    private static EnemyDifficultyApplier instance;
    private Dictionary<string, GameObject[]> lookup;

    private void Awake()
    {
        instance = this;
        BuildLookup();
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void Start()
    {
        ApplyToActiveScene();
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        ApplyToActiveScene();
    }

    public static void ApplyToActiveScene()
    {
        GameDifficultySettings.Load();

        if (instance == null)
            instance = FindFirstObjectByType<EnemyDifficultyApplier>();

        if (instance == null)
        {
            Debug.LogWarning("EnemyDifficultyApplier: No applier found in the scene.");
            return;
        }

        instance.Apply();
    }

    private void Apply()
    {
        BuildLookup();

        if (lookup == null || lookup.Count == 0)
        {
            Debug.LogWarning("EnemyDifficultyApplier: Variant list is empty. Run Tools > Deserted Echoes > Setup Difficulty System.");
            return;
        }

        string sceneName = SceneManager.GetActiveScene().name;
        if (sceneName == "main-menu" || sceneName == "game-over")
            return;

        EnemyControllerBase[] enemies = FindObjectsByType<EnemyControllerBase>(FindObjectsSortMode.None);
        int variantIndex = GameDifficultySettings.VariantIndex;

        foreach (EnemyControllerBase enemy in enemies)
        {
            if (!TryGetEnemyKey(enemy, out string enemyKey, out int currentVariant))
                continue;

            if (currentVariant == variantIndex)
                continue;

            if (!TryGetPrefab(enemyKey, variantIndex, out GameObject prefab))
                continue;

            ReplaceEnemy(enemy, prefab);
        }
    }

    public bool TryGetPrefab(string enemyKey, int variantIndex, out GameObject prefab)
    {
        prefab = null;
        if (lookup == null || string.IsNullOrEmpty(enemyKey))
            return false;

        if (!lookup.TryGetValue(enemyKey, out GameObject[] variants))
            return false;

        int arrayIndex = Mathf.Clamp(variantIndex - 1, 0, 2);
        prefab = variants[arrayIndex];
        return prefab != null;
    }

    public void SetEntries(List<VariantEntry> newEntries)
    {
        entries = newEntries ?? new List<VariantEntry>();
        lookup = null;
        BuildLookup();
    }

    private void BuildLookup()
    {
        if (lookup != null)
            return;

        lookup = new Dictionary<string, GameObject[]>(StringComparer.Ordinal);
        foreach (VariantEntry entry in entries)
        {
            if (string.IsNullOrWhiteSpace(entry.enemyKey) || entry.variants == null)
                continue;

            lookup[entry.enemyKey.Trim()] = entry.variants;
        }
    }

    private static void ReplaceEnemy(EnemyControllerBase enemy, GameObject prefab)
    {
        Transform transform = enemy.transform;
        Vector3 position = transform.position;
        Quaternion rotation = transform.rotation;
        Transform parent = transform.parent;
        string objectName = enemy.gameObject.name;

        Destroy(enemy.gameObject);
        GameObject instance = Instantiate(prefab, position, rotation, parent);
        instance.name = objectName;
    }

    public static bool TryGetEnemyKey(EnemyControllerBase enemy, out string enemyKey, out int currentVariant)
    {
        enemyKey = null;
        currentVariant = 0;

        if (enemy == null)
            return false;

        string objectName = enemy.gameObject.name;
        Match match = NamePattern.Match(objectName);
        if (match.Success)
        {
            enemyKey = match.Groups[1].Value;
            currentVariant = int.Parse(match.Groups[2].Value);
            return true;
        }

        enemyKey = enemy.GetType().Name.Replace("Controller", string.Empty);
        currentVariant = 1;
        return !string.IsNullOrEmpty(enemyKey);
    }
}
