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
        ResolvePanelReferences();
        CloseAllPanels();
        ShowButtonList();
    }

    private void Update()
    {
        if (instructionsPanel != null && instructionsPanel.activeSelf)
        {
            if (Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.M))
                OnClosePanel();
        }
    }

    // ── Button Callbacks (wire these in Inspector OnClick) ───

    /// <summary>Start Game button → shows difficulty selection.</summary>
    public void OnStartGame()
    {
        PlaySound(panelOpenClip);
        OpenPanel(difficultyPanel, "DifficultyPanel");
    }

    public void OnSelectEasy() => OnSelectDifficulty(GameDifficulty.Easy);

    public void OnSelectNormal() => OnSelectDifficulty(GameDifficulty.Normal);

    public void OnSelectHard() => OnSelectDifficulty(GameDifficulty.Hard);

    /// <summary>Called by Easy / Normal / Hard buttons.</summary>
    public void OnSelectDifficulty(GameDifficulty difficulty)
    {
        PlaySound(buttonClickClip);
        GameDifficultySettings.Set(difficulty);

        SaveManager saveManager = EnsureSaveManagerExists();
        saveManager.CreateNewGame(firstLevelScene);

        if (PortalSpawnManager.Instance != null)
            PortalSpawnManager.Instance.ClearReturnPositions();

        OpeningCinematic.TargetScene = firstLevelScene;
        SceneLoader.LoadScene("Opening");
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

        SaveManager saveManager = EnsureSaveManagerExists();
        if (!saveManager.HasSaveFile())
        {
            Debug.LogWarning("No save file found. Load game cancelled.");
            return;
        }

        saveManager.LoadGame();
        string savedScene = saveManager.CurrentSaveData.lastSceneName;
        SceneLoader.LoadScene(string.IsNullOrWhiteSpace(savedScene) ? firstLevelScene : savedScene);
    }

    /// <summary>Instructions button → shows instructions panel.</summary>
    public void OnInstructions()
    {
        PlaySound(panelOpenClip);
        OpenPanel(instructionsPanel, "InstructionsPanel");
    }

    /// <summary>Credits button → plays the credits video fullscreen.</summary>
    public void OnCredits()
    {
        PlaySound(panelOpenClip);
        HideButtonList();
        CreditsVideoPlayer.Play(ShowButtonList);
    }

    /// <summary>Settings button → shows settings panel.</summary>
    public void OnSettings()
    {
        PlaySound(panelOpenClip);
        OpenPanel(settingsPanel, "SettingsPanel");
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

    private void OpenPanel(GameObject panel, string panelName)
    {
        if (panel == null)
            panel = FindPanel(panelName);

        if (panel == null)
        {
            Debug.LogWarning($"MainMenuUI: {panelName} is not assigned or found in the scene.");
            CloseAllPanels();
            ShowButtonList();
            return;
        }

        CloseAllPanels();

        if (IsPanelInsideButtonList(panel))
            panel.transform.SetParent(transform, false);

        panel.SetActive(true);
        panel.transform.SetAsLastSibling();
        HideButtonList();
    }

    private GameObject FindPanel(string panelName)
    {
        Transform[] children = GetComponentsInChildren<Transform>(true);
        foreach (Transform child in children)
        {
            if (child.name == panelName)
                return child.gameObject;
        }

        return null;
    }

    private void ResolvePanelReferences()
    {
        if (instructionsPanel == null) instructionsPanel = FindPanel("InstructionsPanel");
        if (creditsPanel == null)      creditsPanel = FindPanel("CreditsPanel");
        if (settingsPanel == null)     settingsPanel = FindPanel("SettingsPanel");
        if (difficultyPanel == null)   difficultyPanel = FindPanel("DifficultyPanel");
    }

    private bool IsPanelInsideButtonList(GameObject panel)
    {
        return buttonList != null && panel.transform.IsChildOf(buttonList.transform);
    }

    private SaveManager EnsureSaveManagerExists()
    {
        if (SaveManager.Instance != null)
            return SaveManager.Instance;

        GameObject saveManagerObj = new GameObject("SaveManager");
        return saveManagerObj.AddComponent<SaveManager>();
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
