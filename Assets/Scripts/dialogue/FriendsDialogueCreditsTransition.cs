using UnityEngine;
using UnityEngine.SceneManagement;

public class FriendsDialogueCreditsTransition : MonoBehaviour
{
    private const string FriendsSceneName = "Friends";
    private const string CreditsSceneName = "Credits";

    [SerializeField] private float fadeOutDuration = 3f;
    [SerializeField] private float fadeInDuration = 0.5f;

    private bool hasTriggered;
    private bool subscribed;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void Initialize()
    {
        SceneManager.sceneLoaded -= HandleSceneLoaded;
        SceneManager.sceneLoaded += HandleSceneLoaded;
        EnsureForScene(SceneManager.GetActiveScene());
    }

    private static void HandleSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        EnsureForScene(scene);
    }

    private static void EnsureForScene(Scene scene)
    {
        if (scene.name != FriendsSceneName)
            return;

        if (Object.FindAnyObjectByType<FriendsDialogueCreditsTransition>() != null)
            return;

        GameObject transition = new GameObject("FriendsDialogueCreditsTransition");
        SceneManager.MoveGameObjectToScene(transition, scene);
        transition.AddComponent<FriendsDialogueCreditsTransition>();
    }

    private void OnEnable()
    {
        TrySubscribe();
    }

    private void Update()
    {
        if (!subscribed)
            TrySubscribe();
    }

    private void OnDisable()
    {
        if (DialogueManager.Instance != null)
            DialogueManager.Instance.ConversationEnded -= HandleConversationEnded;

        subscribed = false;
    }

    private void TrySubscribe()
    {
        if (DialogueManager.Instance == null)
            return;

        DialogueManager.Instance.ConversationEnded -= HandleConversationEnded;
        DialogueManager.Instance.ConversationEnded += HandleConversationEnded;
        subscribed = true;
    }

    private void HandleConversationEnded(Conversation conversation)
    {
        if (hasTriggered || SceneManager.GetActiveScene().name != FriendsSceneName)
            return;

        hasTriggered = true;

        if (SaveManager.Instance != null)
        {
            SaveManager.Instance.MarkFriendsSceneSeen();
            SaveManager.Instance.SaveGame();
        }

        if (SceneTransition.Instance != null)
        {
            SceneTransition.Instance.TransitionToScene(
                CreditsSceneName,
                fadeOutDuration,
                fadeInDuration,
                false);
            return;
        }

        SceneLoader.LoadScene(CreditsSceneName);
    }
}
