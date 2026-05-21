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

public class MainMenuUI : MonoBehaviour
{
    [Header("Panels")]
    [SerializeField] private GameObject instructionsPanel;
    [SerializeField] private GameObject creditsPanel;
    [SerializeField] private GameObject settingsPanel;
    [SerializeField] private GameObject difficultyPanel;

    [Header("Navigation")]
    [SerializeField] private GameObject buttonList;

    [Header("Scene")]
    [SerializeField] private string firstLevelScene = "Open-World";

    [Header("UI Sound Effects")]
    [SerializeField] private AudioSource uiAudio;
    [SerializeField] private AudioClip buttonClickClip;
    [SerializeField] private AudioClip panelOpenClip;
    [SerializeField] private AudioClip panelCloseClip;

    private void Start()
    {
        GameDifficultySettings.Load();
        CloseAllPanels();
        ShowButtonList();
    }

    // ── Button Callbacks (wire these in Inspector OnClick) ───

    /// <summary>Start Game button → shows difficulty selection.</summary>
    public void OnStartGame()
    {
        PlaySound(panelOpenClip);
        CloseAllPanels();
        HideButtonList();
        if (difficultyPanel != null)
            difficultyPanel.SetActive(true);
    }

    public void OnSelectEasy() => OnSelectDifficulty(GameDifficulty.Easy);

    public void OnSelectNormal() => OnSelectDifficulty(GameDifficulty.Normal);

    public void OnSelectHard() => OnSelectDifficulty(GameDifficulty.Hard);

    /// <summary>Called by Easy / Normal / Hard buttons.</summary>
    public void OnSelectDifficulty(GameDifficulty difficulty)
    {
        PlaySound(buttonClickClip);
        GameDifficultySettings.Set(difficulty);

        if (GameManager.Instance != null)
            GameManager.Instance.StartNewGame(firstLevelScene);
        else
            SceneLoader.LoadScene(firstLevelScene);
    }

    /// <summary>Load Game button → loads the last saved scene and player data.</summary>
    public void OnLoadGame()
    {
        PlaySound(buttonClickClip);

        if (GameManager.Instance != null)
        {
            GameManager.Instance.LoadSavedGame(firstLevelScene);
            return;
        }

        if (SaveManager.Instance != null)
        {
            SaveManager.Instance.LoadGame();
            string savedScene = SaveManager.Instance.CurrentSaveData.lastSceneName;
            SceneLoader.LoadScene(string.IsNullOrWhiteSpace(savedScene) ? firstLevelScene : savedScene);
            return;
        }

        SceneLoader.LoadScene(firstLevelScene);
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
        if (difficultyPanel != null)   difficultyPanel.SetActive(false);
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
