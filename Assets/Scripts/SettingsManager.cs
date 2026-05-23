// ─────────────────────────────────────────────────────────────
// SettingsManager.cs
// Deserted Echoes | IT8101 Games Development | Group 3
// Author: Ali Husain Ali Alsaffar
// Sprint: 5 | Created: May 20, 2026
// Description: Persistent singleton that manages global settings across all scenes.
//              Automatically auto-bootstraps at startup and applies saved
//              PlayerPrefs volumes and fullscreen settings when a scene loads.
// ─────────────────────────────────────────────────────────────

using System.Collections;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;

public class SettingsManager : MonoBehaviour
{
    // ── Singleton ────────────────────────────────────────────
    public static SettingsManager Instance { get; private set; }

    [Header("Audio Mixer")]
    [SerializeField] private AudioMixer audioMixer;
    [SerializeField] private AudioMixerGroup musicGroup;
    [SerializeField] private AudioMixerGroup sfxGroup;

    public AudioMixerGroup MusicGroup => musicGroup;
    public AudioMixerGroup SFXGroup => sfxGroup;

    // ── PlayerPrefs Keys ─────────────────────────────────────
public const string KEY_MASTER  = "MasterVolume";
    public const string KEY_MUSIC   = "MusicVolume";
    public const string KEY_SFX     = "SFXVolume";
    public const string KEY_FULLSCR = "Fullscreen";

    // ── Audio Mixer Exposed Parameter Names ──────────────────
    private const string PARAM_MASTER = "MasterVolume";
    private const string PARAM_MUSIC  = "MusicVolume";
    private const string PARAM_SFX    = "SFXVolume";

    // ── Auto-bootstrap before any scene loads ─────────────────
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void AutoBootstrap()
    {
        if (Instance != null) return;
        GameObject go = new GameObject("SettingsManager");
        go.AddComponent<SettingsManager>();
    }

    // ── Lifecycle ──────────────────────────────────────────────
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        ResolveMixer();
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void Start()
    {
        StartCoroutine(ApplySettingsNextFrame());
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        StartCoroutine(ApplySettingsNextFrame());
    }

    // Waits one frame so the audio engine has fully initialized all PlayOnAwake
    // sources before we change their volume — calling ApplyAllSettings() during
    // sceneLoaded itself is too early and gets overridden by the audio frame.
    private IEnumerator ApplySettingsNextFrame()
    {
        yield return null;
        ApplyAllSettings();
    }

    // ── Resolve references ───────────────────────────────────
    public void ResolveMixer()
    {
        if (audioMixer == null)
        {
            foreach (var m in Resources.FindObjectsOfTypeAll<AudioMixer>())
            {
                if (m.name == "MainAudioMixer")
                {
                    audioMixer = m;
                    break;
                }
            }
        }

        if (audioMixer != null)
        {
            if (musicGroup == null)
            {
                AudioMixerGroup[] groups = audioMixer.FindMatchingGroups("Music");
                if (groups.Length > 0) musicGroup = groups[0];
            }
            if (sfxGroup == null)
            {
                AudioMixerGroup[] groups = audioMixer.FindMatchingGroups("SFX");
                if (groups.Length > 0) sfxGroup = groups[0];
            }
        }
    }

    // ── Load & Apply Settings ────────────────────────────────

    /// <summary>Loads settings from PlayerPrefs and applies them globally.</summary>
    public void ApplyAllSettings()
    {
        ResolveMixer();

        // Load volumes (defaults to 0.2f = 20%)
        float master  = PlayerPrefs.GetFloat(KEY_MASTER,  0.2f);
        float music   = PlayerPrefs.GetFloat(KEY_MUSIC,   0.2f);
        float sfx     = PlayerPrefs.GetFloat(KEY_SFX,     0.2f);
        int   fullscr = PlayerPrefs.GetInt(KEY_FULLSCR,   Screen.fullScreen ? 1 : 0);

        // 1. Apply to Audio Mixer
        ApplyVolume(PARAM_MASTER, master);
        ApplyVolume(PARAM_MUSIC,  music);
        ApplyVolume(PARAM_SFX,    sfx);

        // 2. Route and apply music volume to all AudioSources in the scene
        foreach (AudioSource src in FindObjectsOfType<AudioSource>())
        {
            // Auto-route to Mixer Group if not set
            if (src.outputAudioMixerGroup == null)
            {
                // Heuristic: Loop + 2D = Music, everything else = SFX
                if (src.loop && src.spatialBlend == 0f)
                    src.outputAudioMixerGroup = musicGroup;
                else
                    src.outputAudioMixerGroup = sfxGroup;
            }

            // Fallback: apply volume directly on music sources
            if (src.outputAudioMixerGroup == musicGroup)
            {
                src.volume = music * master;
            }
        }

        FootstepSounds footstep = Object.FindAnyObjectByType<FootstepSounds>();
        if (footstep != null)
            footstep.SetVolume(sfx * master);

        SettingsPanelUI settingsPanel = Object.FindAnyObjectByType<SettingsPanelUI>();
        if (settingsPanel != null)
        {
            AudioSource uiAS = settingsPanel.GetComponentInParent<AudioSource>();
            if (uiAS != null)
                uiAS.volume = music * master;
        }

        // 3. Apply Fullscreen display setting
        Screen.fullScreen = fullscr == 1;
    }

    /// <summary>Converts a 0–1 value to decibels and sets it on the mixer.</summary>
    private void ApplyVolume(string parameter, float value)
    {
        if (audioMixer == null) return;

        // Clamp to avoid log(0) — minimum slider value maps to -80 dB (silence)
        float dB = value > 0.0001f ? Mathf.Log10(value) * 20f : -80f;
        audioMixer.SetFloat(parameter, dB);
    }

    /// <summary>Applies only the AudioMixer parameters from saved settings.
    /// Call this before a scene activates so audio starts at the correct level.</summary>
    public void ApplyMixerSettings()
    {
        ResolveMixer();
        float master = PlayerPrefs.GetFloat(KEY_MASTER, 0.2f);
        float music  = PlayerPrefs.GetFloat(KEY_MUSIC,  0.2f);
        float sfx    = PlayerPrefs.GetFloat(KEY_SFX,    0.2f);
        ApplyVolume(PARAM_MASTER, master);
        ApplyVolume(PARAM_MUSIC,  music);
        ApplyVolume(PARAM_SFX,    sfx);
    }

    // ── Public Setters ───────────────────────────────────────

    public void SetMasterVolume(float value)
    {
        PlayerPrefs.SetFloat(KEY_MASTER, value);
        PlayerPrefs.Save();
        ApplyAllSettings();
    }

    public void SetMusicVolume(float value)
    {
        PlayerPrefs.SetFloat(KEY_MUSIC, value);
        PlayerPrefs.Save();
        ApplyAllSettings();
    }

    public void SetSFXVolume(float value)
    {
        PlayerPrefs.SetFloat(KEY_SFX, value);
        PlayerPrefs.Save();
        ApplyAllSettings();
    }

    public void SetFullscreen(bool isOn)
    {
        PlayerPrefs.SetInt(KEY_FULLSCR, isOn ? 1 : 0);
        PlayerPrefs.Save();
        ApplyAllSettings();
    }
}
