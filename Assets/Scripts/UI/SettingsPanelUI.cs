// ─────────────────────────────────────────────────────────────
// SettingsPanelUI.cs
// Deserted Echoes | IT8101 Games Development | Group 3
// Author: Ali Husain Ali Alsaffar
// Sprint: 3 | Created: May 1, 2026
// Description: Controls the Settings sub-panel inside the Pause Menu.
//              Handles Master / Music / SFX volume sliders and
//              Fullscreen toggle. Settings are saved via PlayerPrefs
//              and restored automatically on startup.
//              Wire up all fields in the Inspector.
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

    [Header("Volume Sliders")]
    [SerializeField] private Slider masterVolumeSlider;
    [SerializeField] private Slider musicVolumeSlider;
    [SerializeField] private Slider sfxVolumeSlider;

    [Header("Fullscreen Toggle")]
    [SerializeField] private Toggle fullscreenToggle;

    [Header("UI Sound Effects")]
    [SerializeField] private AudioClip panelOpenClip;
    [SerializeField] private AudioClip toggleClip;

    // ── PlayerPrefs Keys ─────────────────────────────────────
    private const string KEY_MASTER  = "MasterVolume";
    private const string KEY_MUSIC   = "MusicVolume";
    private const string KEY_SFX     = "SFXVolume";
    private const string KEY_FULLSCR = "Fullscreen";

    // ── Audio Mixer Exposed Parameter Names ──────────────────
    private const string PARAM_MASTER = "MasterVolume";
    private const string PARAM_MUSIC  = "MusicVolume";
    private const string PARAM_SFX    = "SFXVolume";

    // ─────────────────────────────────────────────────────────

    private void OnEnable()
    {
        ResolveMusicSource();
        LoadSettings();
        PlayOpenSound();
    }

    private void ResolveMusicSource()
    {
        if (musicSource != null) return;
        GameObject go = GameObject.Find("negev_desert_music");
        if (go != null) musicSource = go.GetComponent<AudioSource>();
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

    // ── Load & Apply saved settings ──────────────────────────

    private void LoadSettings()
    {
        float master  = PlayerPrefs.GetFloat(KEY_MASTER,  0.75f);
        float music   = PlayerPrefs.GetFloat(KEY_MUSIC,   0.75f);
        float sfx     = PlayerPrefs.GetFloat(KEY_SFX,     0.75f);
        int   fullscr = PlayerPrefs.GetInt(KEY_FULLSCR,   Screen.fullScreen ? 1 : 0);

        // Update sliders without triggering callbacks (set value silently)
        if (masterVolumeSlider != null)  masterVolumeSlider.SetValueWithoutNotify(master);
        if (musicVolumeSlider  != null)  musicVolumeSlider.SetValueWithoutNotify(music);
        if (sfxVolumeSlider    != null)  sfxVolumeSlider.SetValueWithoutNotify(sfx);
        if (fullscreenToggle   != null)  fullscreenToggle.SetIsOnWithoutNotify(fullscr == 1);

        // Apply to Audio Mixer
        ApplyVolume(PARAM_MASTER, master);
        ApplyVolume(PARAM_MUSIC,  music);
        ApplyVolume(PARAM_SFX,    sfx);

        // Apply directly to audio sources if no mixer
        if (musicSource != null) musicSource.volume = music;
        Screen.fullScreen = fullscr == 1;
    }

    // ── Slider Callbacks (wire to OnValueChanged in Inspector) ──

    /// <summary>Called by MasterVolumeSlider OnValueChanged.</summary>
    public void OnMasterVolumeChanged(float value)
    {
        ApplyVolume(PARAM_MASTER, value);
        PlayerPrefs.SetFloat(KEY_MASTER, value);
    }

    /// <summary>Called by MusicVolumeSlider OnValueChanged.</summary>
    public void OnMusicVolumeChanged(float value)
    {
        ApplyVolume(PARAM_MUSIC, value);
        if (musicSource != null) musicSource.volume = value;
        PlayerPrefs.SetFloat(KEY_MUSIC, value);
    }

    /// <summary>Called by SFXVolumeSlider OnValueChanged.</summary>
    public void OnSFXVolumeChanged(float value)
    {
        ApplyVolume(PARAM_SFX, value);
        PlayerPrefs.SetFloat(KEY_SFX, value);
    }

    // ── Toggle Callback (wire to OnValueChanged in Inspector) ──

    /// <summary>Called by FullscreenToggle OnValueChanged.</summary>
    public void OnFullscreenToggleChanged(bool isOn)
    {
        PlaySound(toggleClip);
        Screen.fullScreen = isOn;
        PlayerPrefs.SetInt(KEY_FULLSCR, isOn ? 1 : 0);
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
