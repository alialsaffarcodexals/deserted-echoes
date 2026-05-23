// ─────────────────────────────────────────────────────────────
// GameOverUI.cs
// Deserted Echoes | IT8101 Games Development | Group 3
// Description: Full-screen game over overlay. Add GameOverCanvas prefab
//              to level scenes (same as PauseMenuCanvas). Shown when the
//              player dies, after the death animation delay.
// ─────────────────────────────────────────────────────────────

using System.Collections;
using UnityEngine;

public class GameOverUI : MonoBehaviour
{
    public static GameOverUI Instance { get; private set; }

    [Header("Overlay")]
    [SerializeField] private GameObject overlayPanel;

    [Header("Timing")]
    [SerializeField] private float showDelay = 1.5f;

    [Header("UI Sound Effects")]
    [SerializeField] private AudioSource uiAudio;
    [SerializeField] private AudioClip overlayOpenClip;

    private bool isShowing;
    private Coroutine showRoutine;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        if (overlayPanel != null)
            overlayPanel.SetActive(false);
    }

    private void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }

    /// <summary>Show overlay after a delay (lets death animation play).</summary>
    public void ShowAfterDelay(float delay = -1f)
    {
        if (isShowing)
            return;

        if (showRoutine != null)
            StopCoroutine(showRoutine);

        float wait = delay >= 0f ? delay : showDelay;
        showRoutine = StartCoroutine(ShowAfterDelayRoutine(wait));
    }

    /// <summary>Show overlay immediately.</summary>
    public void Show()
    {
        if (isShowing)
            return;

        isShowing = true;

        if (overlayPanel != null)
            overlayPanel.SetActive(true);

        PlaySound(overlayOpenClip);

        if (GameManager.Instance != null)
            GameManager.Instance.PauseGame();
        else
            Time.timeScale = 0f;
    }

    public void OnRetry()
    {
        ResumeTime();
        SceneLoader.ReloadCurrentScene();
    }

    public void OnMainMenu()
    {
        ResumeTime();

        if (GameManager.Instance != null)
            GameManager.Instance.ResetGame();

        SceneLoader.LoadScene("main-menu");
    }

    private IEnumerator ShowAfterDelayRoutine(float delay)
    {
        if (delay > 0f)
            yield return new WaitForSeconds(delay);

        showRoutine = null;
        Show();
    }

    private void ResumeTime()
    {
        if (GameManager.Instance != null)
            GameManager.Instance.ResumeGame();
        else
            Time.timeScale = 1f;
    }

    private void PlaySound(AudioClip clip)
    {
        if (uiAudio != null && clip != null)
            uiAudio.PlayOneShot(clip);
    }
}
