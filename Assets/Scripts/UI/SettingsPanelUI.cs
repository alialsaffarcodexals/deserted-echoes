// ─────────────────────────────────────────────────────────────
// SettingsPanelUI.cs
// Deserted Echoes | IT8101 Games Development | Group 3
// Author: Ali Husain Ali Alsaffar
// Sprint: 3 | Created: May 1, 2026
// Description: Controls the Settings sub-panel inside the Pause Menu.
//              Handles Master / Music / SFX volume sliders and
//              Fullscreen toggle. Sliders preview audio in real time;
//              settings are only committed on Apply. Close reverts to
//              last saved state. Main Menu button exits to main-menu scene.
// ─────────────────────────────────────────────────────────────

using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;

public class SettingsPanelUI : MonoBehaviour
{
    [Header("Audio Mixer")]
    [SerializeField] private AudioMixer audioMixer;

    [Header("Direct Audio Sources (used when no mixer is assigned)")]
    [SerializeField] private AudioSource musicSource;

    // ── Runtime-resolved references ──────────────────────────
    private FootstepSounds footstepSounds;
    private AudioSource    uiAudioSource;

    [Header("Volume Sliders")]
    [SerializeField] private Slider masterVolumeSlider;
    [SerializeField] private Slider musicVolumeSlider;
    [SerializeField] private Slider sfxVolumeSlider;

    [Header("Fullscreen Toggle")]
    [SerializeField] private Toggle fullscreenToggle;

    [Header("UI Sound Effects")]
    [SerializeField] private AudioClip panelOpenClip;
    [SerializeField] private AudioClip toggleClip;
    [SerializeField] private AudioClip buttonClickClip;

    // ─────────────────────────────────────────────────────────

    private void OnEnable()
    {
        LoadSettingsIntoUI();
        PlaySound(panelOpenClip);
    }

    private void PlaySound(AudioClip clip)
    {
        AudioSource src = GetComponentInParent<AudioSource>();
        if (src != null && clip != null)
            src.PlayOneShot(clip);
    }

    // ── Load saved settings into UI elements ─────────────────

    private void LoadSettingsIntoUI()
    {
        float master  = PlayerPrefs.GetFloat(SettingsManager.KEY_MASTER,  0.2f);
        float music   = PlayerPrefs.GetFloat(SettingsManager.KEY_MUSIC,   0.2f);
        float sfx     = PlayerPrefs.GetFloat(SettingsManager.KEY_SFX,     0.2f);
        int   fullscr = PlayerPrefs.GetInt(SettingsManager.KEY_FULLSCR,   Screen.fullScreen ? 1 : 0);

        if (masterVolumeSlider != null)  masterVolumeSlider.SetValueWithoutNotify(master);
        if (musicVolumeSlider  != null)  musicVolumeSlider.SetValueWithoutNotify(music);
        if (sfxVolumeSlider    != null)  sfxVolumeSlider.SetValueWithoutNotify(sfx);
        if (fullscreenToggle   != null)  fullscreenToggle.SetIsOnWithoutNotify(fullscr == 1);
    }

    // ── Slider Callbacks — preview only, no PlayerPrefs writes ──

    /// <summary>Called by MasterVolumeSlider OnValueChanged.</summary>
    public void OnMasterVolumeChanged(float value)
    {
        ApplyVolume("MasterVolume", value);
    }

    /// <summary>Called by MusicVolumeSlider OnValueChanged.</summary>
    public void OnMusicVolumeChanged(float value)
    {
        ApplyVolume("MusicVolume", value);
    }

    /// <summary>Called by SFXVolumeSlider OnValueChanged.</summary>
    public void OnSFXVolumeChanged(float value)
    {
        ApplyVolume("SFXVolume", value);
    }

    // ── Toggle Callback — preview only ──────────────────────

    /// <summary>Called by FullscreenToggle OnValueChanged.</summary>
    public void OnFullscreenToggleChanged(bool isOn)
    {
        PlaySound(toggleClip);
        Screen.fullScreen = isOn;
    }

    // ── Button Callbacks ─────────────────────────────────────

    /// <summary>Saves all current slider/toggle values and hides the panel.</summary>
    public void OnApply()
    {
        PlaySound(buttonClickClip);

        float master  = masterVolumeSlider  != null ? masterVolumeSlider.value  : PlayerPrefs.GetFloat(SettingsManager.KEY_MASTER, 0.2f);
        float music   = musicVolumeSlider   != null ? musicVolumeSlider.value   : PlayerPrefs.GetFloat(SettingsManager.KEY_MUSIC,  0.2f);
        float sfx     = sfxVolumeSlider     != null ? sfxVolumeSlider.value     : PlayerPrefs.GetFloat(SettingsManager.KEY_SFX,    0.2f);
        bool  fullscr = fullscreenToggle    != null ? fullscreenToggle.isOn     : Screen.fullScreen;

        if (SettingsManager.Instance != null)
        {
            SettingsManager.Instance.SetMasterVolume(master);
            SettingsManager.Instance.SetMusicVolume(music);
            SettingsManager.Instance.SetSFXVolume(sfx);
            SettingsManager.Instance.SetFullscreen(fullscr);
        }
        else
        {
            PlayerPrefs.SetFloat(SettingsManager.KEY_MASTER, master);
            PlayerPrefs.SetFloat(SettingsManager.KEY_MUSIC,  music);
            PlayerPrefs.SetFloat(SettingsManager.KEY_SFX,    sfx);
            PlayerPrefs.SetInt(SettingsManager.KEY_FULLSCR,  fullscr ? 1 : 0);
            PlayerPrefs.Save();
        }

        NotifyParentAndClose();
    }

    /// <summary>Reverts the AudioMixer to last saved state and hides the panel.</summary>
    public void OnClose()
    {
        if (SettingsManager.Instance != null)
            SettingsManager.Instance.ApplyAllSettings();

        NotifyParentAndClose();
    }

    private void NotifyParentAndClose()
    {
        MainMenuUI mainMenu = GetComponentInParent<MainMenuUI>();
        if (mainMenu != null)
            mainMenu.OnClosePanel();
        else
            gameObject.SetActive(false);
    }

    /// <summary>Returns to the main menu, resetting timescale first.</summary>
    public void OnMainMenu()
    {
        PlaySound(buttonClickClip);
        Time.timeScale = 1f;
        SceneLoader.LoadScene("main-menu");
    }

    // ── Private Helpers ──────────────────────────────────────

    /// <summary>Converts a 0–1 slider value to decibels and sets it on the mixer.</summary>
    private void ApplyVolume(string parameter, float value)
    {
        if (audioMixer == null) return;

        float dB = value > 0.0001f ? Mathf.Log10(value) * 20f : -80f;
        audioMixer.SetFloat(parameter, dB);
    }
}
