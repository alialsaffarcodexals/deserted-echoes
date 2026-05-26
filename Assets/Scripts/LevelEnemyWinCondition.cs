using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelEnemyWinCondition : MonoBehaviour
{
    [Header("Win Condition")]
    [SerializeField] private string targetSceneName = "Open-World";
    [SerializeField] private float loadDelay = 1.5f;
    [SerializeField] private bool triggerWhenNoEnemiesAtStart;

    private readonly HashSet<EnemyControllerBase> trackedEnemies = new HashSet<EnemyControllerBase>();
    private readonly HashSet<GameObject> remainingEnemyObjects = new HashSet<GameObject>();
    private bool hasTrackedEnemies;
    private bool hasWon;

    public void Configure(string sceneName, float delay)
    {
        targetSceneName = sceneName;
        loadDelay = delay;
    }

    private void Start()
    {
        RefreshEnemies();

        hasTrackedEnemies = remainingEnemyObjects.Count > 0;

        if (remainingEnemyObjects.Count == 0 && triggerWhenNoEnemiesAtStart)
            CompleteLevel();
    }

    private void Update()
    {
        if (hasWon)
            return;

        PruneDestroyedEnemies();

        if (remainingEnemyObjects.Count == 0 && hasTrackedEnemies)
            CompleteLevel();
    }

    private void OnDestroy()
    {
        foreach (EnemyControllerBase enemy in trackedEnemies)
        {
            if (enemy != null)
                enemy.OnDefeated -= HandleEnemyDefeated;
        }

        trackedEnemies.Clear();
        remainingEnemyObjects.Clear();
    }

    private void RefreshEnemies()
    {
        foreach (EnemyControllerBase enemy in trackedEnemies)
        {
            if (enemy != null)
                enemy.OnDefeated -= HandleEnemyDefeated;
        }

        trackedEnemies.Clear();
        remainingEnemyObjects.Clear();

        EnemyControllerBase[] enemies = FindObjectsByType<EnemyControllerBase>(FindObjectsSortMode.None);
        foreach (EnemyControllerBase enemy in enemies)
        {
            if (enemy == null)
                continue;

            trackedEnemies.Add(enemy);
            remainingEnemyObjects.Add(enemy.gameObject);
            enemy.OnDefeated += HandleEnemyDefeated;
        }

        EnemyFollowAndDisappear[] simpleEnemies = FindObjectsByType<EnemyFollowAndDisappear>(FindObjectsSortMode.None);
        foreach (EnemyFollowAndDisappear enemy in simpleEnemies)
        {
            if (enemy != null)
                remainingEnemyObjects.Add(enemy.gameObject);
        }

        Debug.Log($"LevelEnemyWinCondition: Tracking {remainingEnemyObjects.Count} enemies in {SceneManager.GetActiveScene().name}.");
    }

    private void HandleEnemyDefeated(EnemyControllerBase enemy)
    {
        if (hasWon || enemy == null)
            return;

        enemy.OnDefeated -= HandleEnemyDefeated;
        trackedEnemies.Remove(enemy);
        remainingEnemyObjects.Remove(enemy.gameObject);

        Debug.Log($"LevelEnemyWinCondition: {remainingEnemyObjects.Count} enemies remaining.");

        if (remainingEnemyObjects.Count == 0)
            CompleteLevel();
    }

    private void PruneDestroyedEnemies()
    {
        remainingEnemyObjects.RemoveWhere(enemyObject => enemyObject == null);
    }

    private void CompleteLevel()
    {
        if (hasWon)
            return;

        hasWon = true;
        StartCoroutine(LoadTargetSceneAfterDelay());
    }

    private IEnumerator LoadTargetSceneAfterDelay()
    {
        if (loadDelay > 0f)
            yield return new WaitForSecondsRealtime(loadDelay);

        if (string.IsNullOrWhiteSpace(targetSceneName))
        {
            SceneLoader.LoadNextScene();
            yield break;
        }

        SceneLoader.LoadScene(targetSceneName);
    }
}
