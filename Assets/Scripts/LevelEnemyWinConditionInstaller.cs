using UnityEngine;
using UnityEngine.SceneManagement;

public static class LevelEnemyWinConditionInstaller
{
    private const string TargetSceneName = "Open-World";
    private const float LoadDelay = 1.5f;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void Initialize()
    {
        SceneManager.sceneLoaded -= HandleSceneLoaded;
        SceneManager.sceneLoaded += HandleSceneLoaded;
        InstallForScene(SceneManager.GetActiveScene());
    }

    private static void HandleSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        InstallForScene(scene);
    }

    private static void InstallForScene(Scene scene)
    {
        if (!ShouldTrackScene(scene.name))
            return;

        if (Object.FindAnyObjectByType<LevelEnemyWinCondition>() != null)
            return;

        GameObject winCondition = new GameObject("LevelEnemyWinCondition");
        SceneManager.MoveGameObjectToScene(winCondition, scene);
        winCondition.AddComponent<LevelEnemyWinCondition>().Configure(TargetSceneName, LoadDelay);
    }

    private static bool ShouldTrackScene(string sceneName)
    {
        return sceneName == "level-11" || sceneName == "level-12";
    }
}
