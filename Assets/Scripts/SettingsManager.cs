// ─────────────────────────────────────────────────────────────
// SettingsManager.cs
// Deserted Echoes | IT8101 Games Development | Group 3
// Author: Ali Husain Ali Alsaffar
// Sprint: 5 | Created: May 20, 2026
// Description: Persistent singleton that manages global settings across all scenes.
//              Automatically auto-bootstraps at startup and applies saved
//              PlayerPrefs volumes and fullscreen settings when a scene loads.
// ─────────────────────────────────────────────────────────────

using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;

public class SettingsManager : MonoBehaviour
{
    // ── Singleton ────────────────────────────────────────────
    public static SettingsManager Instance { get; private set; }

    [Header("Audio Mixer")]
    [SerializeField] private AudioMixer audioMixer;

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
        // Apply settings immediately on startup
        ApplyAllSettings();
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Debug.Log($"SettingsManager: Scene '{scene.name}' loaded. Re-applying settings to maintain consistency.");
        ApplyAllSettings();
    }

    // ── Resolve references ───────────────────────────────────
    public void ResolveMixer()
    {
        if (audioMixer != null) return;
        foreach (var m in Resources.FindObjectsOfTypeAll<AudioMixer>())
        {
            if (m.name == "MainAudioMixer")
            {
                audioMixer = m;
                break;
            }
        }
    }

    // ── Load & Apply Settings ────────────────────────────────

    /// <summary>Loads settings from PlayerPrefs and applies them globally.</summary>
    public void ApplyAllSettings()
    {
        ResolveMixer();

        // Load volumes (defaults to 0.75f)
        float master  = PlayerPrefs.GetFloat(KEY_MASTER,  0.75f);
        float music   = PlayerPrefs.GetFloat(KEY_MUSIC,   0.75f);
        float sfx     = PlayerPrefs.GetFloat(KEY_SFX,     0.75f);
        int   fullscr = PlayerPrefs.GetInt(KEY_FULLSCR,   Screen.fullScreen ? 1 : 0);

        // 1. Apply to Audio Mixer
        ApplyVolume(PARAM_MASTER, master);
        ApplyVolume(PARAM_MUSIC,  music);
        ApplyVolume(PARAM_SFX,    sfx);

        // 2. Apply directly to scene-specific direct audio sources for fallback/scaling
        // Find main music source in scene
        GameObject musicGO = GameObject.Find("negev_desert_music");
        if (musicGO != null)
        {
            AudioSource musicSrc = musicGO.GetComponent<AudioSource>();
            if (musicSrc != null)
            {
                musicSrc.volume = music * master;
            }
        }

        // Find footstep sounds component in scene
        FootstepSounds footstep = FindObjectOfType<FootstepSounds>();
        if (footstep != null)
        {
            footstep.SetVolume(sfx * master);
        }

        // Find parent audio source on active Settings panel if present
        SettingsPanelUI settingsPanel = FindObjectOfType<SettingsPanelUI>();
        if (settingsPanel != null)
        {
            AudioSource uiAS = settingsPanel.GetComponentInParent<AudioSource>();
            if (uiAS != null)
            {
                uiAS.volume = sfx * master;
            }
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
