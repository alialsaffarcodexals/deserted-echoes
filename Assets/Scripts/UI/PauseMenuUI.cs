// ─────────────────────────────────────────────────────────────
// PauseMenuUI.cs
// Deserted Echoes | IT8101 Games Development | Group 3
// Author: Ali Husain Ali Alsaffar
// Sprint: 2 | Created: April 21, 2026
// Description: Controls the Pause Menu canvas. Attach this script
//              to the PauseMenuCanvas GameObject in any level scene.
//              Press ESC in-game to toggle the pause menu.
//              Wire up each button's OnClick() in the Inspector.
// ─────────────────────────────────────────────────────────────

using UnityEngine;
using UnityEngine.UI;

public class PauseMenuUI : MonoBehaviour
{
    [Header("Pause Panel")]
    [SerializeField] private GameObject pausePanel;

    [Header("Sub Panels")]
    [SerializeField] private GameObject settingsPanel;

    [Header("UI Sound Effects")]
    [SerializeField] private AudioSource uiAudio;
    [SerializeField] private AudioClip buttonClickClip;
    [SerializeField] private AudioClip menuOpenClip;
    [SerializeField] private AudioClip menuCloseClip;

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (pausePanel != null && pausePanel.activeSelf)
                OnResume();
            else
                OnPause();
        }
    }

    // ── Button Callbacks (wire these in Inspector OnClick) ───

    /// <summary>Pause the game and show the pause panel.</summary>
    public void OnPause()
    {
        if (pausePanel != null)
            pausePanel.SetActive(true);

        PlaySound(menuOpenClip);
        Time.timeScale = 0f;

        if (GameManager.Instance != null)
            GameManager.Instance.PauseGame();
    }

    /// <summary>Resume button → hides pause panel and resumes game.</summary>
    public void OnResume()
    {
        PlaySound(menuCloseClip);
        CloseAllPanels();

        Time.timeScale = 1f;

        if (GameManager.Instance != null)
            GameManager.Instance.ResumeGame();
    }

    /// <summary>Settings button → shows settings sub-panel on top of pause panel.</summary>
    public void OnSettings()
    {
        PlaySound(buttonClickClip);
        if (settingsPanel != null) settingsPanel.SetActive(true);
    }

    /// <summary>Close settings sub-panel and return to pause panel.</summary>
    public void OnCloseSettings()
    {
        PlaySound(buttonClickClip);
        if (settingsPanel != null) settingsPanel.SetActive(false);
    }

    /// <summary>Main Menu button → resumes time then loads main menu.</summary>
    public void OnMainMenu()
    {
        PlaySound(buttonClickClip);
        Time.timeScale = 1f;

        if (GameManager.Instance != null)
            GameManager.Instance.ResumeGame();

        SceneLoader.LoadScene("main-menu");
    }

    /// <summary>Quit button → exits the application.</summary>
    public void OnQuit()
    {
        PlaySound(buttonClickClip);
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    // ── Private Helpers ──────────────────────────────────────

    private void PlaySound(AudioClip clip)
    {
        if (uiAudio != null && clip != null)
            uiAudio.PlayOneShot(clip);
    }

    private void CloseAllPanels()
    {
        if (pausePanel != null)    pausePanel.SetActive(false);
        if (settingsPanel != null) settingsPanel.SetActive(false);
    }
}
