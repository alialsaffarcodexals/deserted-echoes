// ─────────────────────────────────────────────────────────────
// MainMenuUI.cs
// Deserted Echoes | IT8101 Games Development | Group 3
// Author: Prepared by Ali Husain — Implementation: Faisal Alasfoor
// Sprint: 1 | Created: April 9, 2026
// Description: Controls the Main Menu canvas. Attach this script
//              to the MainMenuCanvas GameObject in the main-menu scene.
//              Wire up each button's OnClick() in the Inspector.
// ─────────────────────────────────────────────────────────────

using UnityEngine;
using UnityEngine.UI;

public class MainMenuUI : MonoBehaviour
{
    [Header("Panels")]
    [SerializeField] private GameObject instructionsPanel;
    [SerializeField] private GameObject creditsPanel;
    [SerializeField] private GameObject settingsPanel;

    [Header("Navigation")]
    [SerializeField] private GameObject buttonList;

    [Header("UI Sound Effects")]
    [SerializeField] private AudioSource uiAudio;
    [SerializeField] private AudioClip buttonClickClip;
    [SerializeField] private AudioClip panelOpenClip;
    [SerializeField] private AudioClip panelCloseClip;

    private void Start()
    {
        CloseAllPanels();
        ShowButtonList();
    }

    // ── Button Callbacks (wire these in Inspector OnClick) ───

    /// <summary>Start Game button → loads first level.</summary>
    public void OnStartGame()
    {
        PlaySound(buttonClickClip);
        SceneLoader.LoadScene("open-world");
    }

    /// <summary>Instructions button → shows instructions panel.</summary>
    public void OnInstructions()
    {
        PlaySound(panelOpenClip);
        CloseAllPanels();
        HideButtonList();
        if (instructionsPanel != null)
            instructionsPanel.SetActive(true);
    }

    /// <summary>Credits button → shows credits panel.</summary>
    public void OnCredits()
    {
        PlaySound(panelOpenClip);
        CloseAllPanels();
        HideButtonList();
        if (creditsPanel != null)
            creditsPanel.SetActive(true);
    }

    /// <summary>Settings button → shows settings panel.</summary>
    public void OnSettings()
    {
        PlaySound(panelOpenClip);
        CloseAllPanels();
        HideButtonList();
        if (settingsPanel != null)
            settingsPanel.SetActive(true);
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

    /// <summary>Close / back button on any overlay panel.</summary>
    public void OnClosePanel()
    {
        PlaySound(panelCloseClip);
        CloseAllPanels();
        ShowButtonList();
    }

    // ── Private Helpers ──────────────────────────────────────

    private void PlaySound(AudioClip clip)
    {
        if (uiAudio != null && clip != null)
            uiAudio.PlayOneShot(clip);
    }

    private void CloseAllPanels()
    {
        if (instructionsPanel != null) instructionsPanel.SetActive(false);
        if (creditsPanel != null)      creditsPanel.SetActive(false);
        if (settingsPanel != null)     settingsPanel.SetActive(false);
    }

    private void ShowButtonList()
    {
        if (buttonList != null) buttonList.SetActive(true);
    }

    private void HideButtonList()
    {
        if (buttonList != null) buttonList.SetActive(false);
    }
}
