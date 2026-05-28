using UnityEngine;
using UnityEngine.SceneManagement;

public static class LevelEnemyWinConditionInstaller
{
    private const string OpenWorldSceneName = "Open-World";
    private const string FriendsSceneName = "Friends";
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
        winCondition.AddComponent<LevelEnemyWinCondition>().Configure(GetTargetSceneName(scene.name), LoadDelay);
    }

    private static bool ShouldTrackScene(string sceneName)
    {
        return sceneName == "level-11" || sceneName == "level-12";
    }

    private static string GetTargetSceneName(string sceneName)
    {
        return sceneName == "level-12" ? FriendsSceneName : OpenWorldSceneName;
    }
}
