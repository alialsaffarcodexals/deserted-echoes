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

    private void Start()
    {
        // Ensure all overlay panels are hidden at start
        CloseAllPanels();
    }

    // ── Button Callbacks (wire these in Inspector OnClick) ───

    /// <summary>Start Game button → loads first level.</summary>
    public void OnStartGame()
    {
        SceneLoader.LoadScene("open-world");
    }

    /// <summary>Instructions button → shows instructions panel.</summary>
    public void OnInstructions()
    {
        CloseAllPanels();
        if (instructionsPanel != null)
            instructionsPanel.SetActive(true);
    }

    /// <summary>Credits button → shows credits panel.</summary>
    public void OnCredits()
    {
        CloseAllPanels();
        if (creditsPanel != null)
            creditsPanel.SetActive(true);
    }

    /// <summary>Settings button → shows settings panel.</summary>
    public void OnSettings()
    {
        CloseAllPanels();
        if (settingsPanel != null)
            settingsPanel.SetActive(true);
    }

    /// <summary>Quit button → exits the application.</summary>
    public void OnQuit()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    /// <summary>Close / back button on any overlay panel.</summary>
    public void OnClosePanel()
    {
        CloseAllPanels();
    }

    // ── Private Helpers ──────────────────────────────────────

    private void CloseAllPanels()
    {
        if (instructionsPanel != null) instructionsPanel.SetActive(false);
        if (creditsPanel != null)      creditsPanel.SetActive(false);
        if (settingsPanel != null)     settingsPanel.SetActive(false);
    }
}
