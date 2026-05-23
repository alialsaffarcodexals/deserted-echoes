// ─────────────────────────────────────────────────────────────
// SettingsPanelUI.cs
// Deserted Echoes | IT8101 Games Development | Group 3
// Author: Ali Husain Ali Alsaffar
// Sprint: 3 | Created: May 1, 2026
// Description: Controls the Settings sub-panel inside the Pause Menu.
//              Handles Master / Music / SFX volume sliders and
//              Fullscreen toggle. Settings are saved via PlayerPrefs
//              and restored automatically on startup.
// ─────────────────────────────────────────────────────────────

using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Audio;

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

    // ─────────────────────────────────────────────────────────

    private void OnEnable()
    {
        LoadSettingsIntoUI();
        PlayOpenSound();
    }

    private void PlayOpenSound()
    {
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
        // Apply settings through SettingsManager to ensure everything is synchronized
        if (SettingsManager.Instance != null)
        {
            SettingsManager.Instance.ApplyAllSettings();
        }

        // Fetch current values from PlayerPrefs using standard keys
        float master  = PlayerPrefs.GetFloat(SettingsManager.KEY_MASTER,  0.2f);
        float music   = PlayerPrefs.GetFloat(SettingsManager.KEY_MUSIC,   0.2f);
        float sfx     = PlayerPrefs.GetFloat(SettingsManager.KEY_SFX,     0.2f);
        int   fullscr = PlayerPrefs.GetInt(SettingsManager.KEY_FULLSCR,   Screen.fullScreen ? 1 : 0);

        // Update UI components silently without triggering their onValueChanged callbacks
        if (masterVolumeSlider != null)  masterVolumeSlider.SetValueWithoutNotify(master);
        if (musicVolumeSlider  != null)  musicVolumeSlider.SetValueWithoutNotify(music);
        if (sfxVolumeSlider    != null)  sfxVolumeSlider.SetValueWithoutNotify(sfx);
        if (fullscreenToggle   != null)  fullscreenToggle.SetIsOnWithoutNotify(fullscr == 1);
    }

    // ── Slider Callbacks (wire to OnValueChanged in Inspector) ──

    /// <summary>Called by MasterVolumeSlider OnValueChanged.</summary>
    public void OnMasterVolumeChanged(float value)
    {
        if (SettingsManager.Instance != null)
        {
            SettingsManager.Instance.SetMasterVolume(value);
        }
        else
        {
            PlayerPrefs.SetFloat(SettingsManager.KEY_MASTER, value);
            PlayerPrefs.Save();
        }
    }

    /// <summary>Called by MusicVolumeSlider OnValueChanged.</summary>
    public void OnMusicVolumeChanged(float value)
    {
        if (SettingsManager.Instance != null)
        {
            SettingsManager.Instance.SetMusicVolume(value);
        }
        else
        {
            PlayerPrefs.SetFloat(SettingsManager.KEY_MUSIC, value);
            PlayerPrefs.Save();
        }
    }

    /// <summary>Called by SFXVolumeSlider OnValueChanged.</summary>
    public void OnSFXVolumeChanged(float value)
    {
        if (SettingsManager.Instance != null)
        {
            SettingsManager.Instance.SetSFXVolume(value);
        }
        else
        {
            PlayerPrefs.SetFloat(SettingsManager.KEY_SFX, value);
            PlayerPrefs.Save();
        }
    }

    // ── Toggle Callback (wire to OnValueChanged in Inspector) ──

    /// <summary>Called by FullscreenToggle OnValueChanged.</summary>
    public void OnFullscreenToggleChanged(bool isOn)
    {
        PlaySound(toggleClip);
        if (SettingsManager.Instance != null)
        {
            SettingsManager.Instance.SetFullscreen(isOn);
        }
        else
        {
            Screen.fullScreen = isOn;
            PlayerPrefs.SetInt(SettingsManager.KEY_FULLSCR, isOn ? 1 : 0);
            PlayerPrefs.Save();
        }
    }

    // ── Private Helpers ──────────────────────────────────────

    /// <summary>Converts a 0–1 slider value to decibels and sets it on the mixer.</summary>
    private void ApplyVolume(string parameter, float value)
    {
        if (audioMixer == null) return;

        // Clamp to avoid log(0) — minimum slider value maps to -80 dB (silence)
        float dB = value > 0.0001f ? Mathf.Log10(value) * 20f : -80f;
        audioMixer.SetFloat(parameter, dB);
    }
}
