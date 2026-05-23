// ─────────────────────────────────────────────────────────────
// SceneTransition.cs
// Deserted Echoes | IT8101 Games Development | Group 3
// Author: Ali Husain Ali Alsaffar (202301152)
// Sprint: 5 | Created: May 18, 2026
// Description: Persistent singleton that plays a teleport sound
//              and fades the screen to black when the player uses
//              a portal. Self-bootstraps at runtime — no scene
//              setup required. All scene loads go through
//              SceneLoader.LoadScene() which delegates here.
// ─────────────────────────────────────────────────────────────

using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class SceneTransition : MonoBehaviour
{
    public static SceneTransition Instance { get; private set; }

    [Header("Fade Settings")]
    [SerializeField] private float fadeOutDuration = 0.4f;
    [SerializeField] private float fadeInDuration  = 0.35f;

    [Header("Teleport Sound")]
    [SerializeField] private AudioClip teleportSound;
    [SerializeField] private float     teleportVolume = 0.85f;

    private CanvasGroup overlay;
    private AudioSource audioSource;
    private bool        isTransitioning;

    // ── Auto-bootstrap before any scene loads ─────────────────

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void AutoBootstrap()
    {
        if (Instance != null) return;
        GameObject go = new GameObject("SceneTransition");
        go.AddComponent<SceneTransition>();
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

        BuildOverlay();
        SetupAudio();
    }

    /// <summary>Builds a full-screen black overlay canvas at runtime.</summary>
    private void BuildOverlay()
    {
        Canvas canvas       = gameObject.AddComponent<Canvas>();
        canvas.renderMode   = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 9999;
        gameObject.AddComponent<CanvasScaler>();
        gameObject.AddComponent<GraphicRaycaster>();

        GameObject panelGO  = new GameObject("FadePanel");
        panelGO.transform.SetParent(transform, false);

        overlay               = panelGO.AddComponent<CanvasGroup>();
        Image img             = panelGO.AddComponent<Image>();
        img.color             = Color.black;
        img.raycastTarget     = true;

        RectTransform rt      = panelGO.GetComponent<RectTransform>();
        rt.anchorMin          = Vector2.zero;
        rt.anchorMax          = Vector2.one;
        rt.offsetMin          = Vector2.zero;
        rt.offsetMax          = Vector2.zero;

        overlay.alpha         = 0f;
        overlay.blocksRaycasts = false;
    }

    private void SetupAudio()
    {
        audioSource            = gameObject.AddComponent<AudioSource>();
        audioSource.playOnAwake = false;

        // Route to SFX group in mixer
        if (SettingsManager.Instance != null)
        {
            audioSource.outputAudioMixerGroup = SettingsManager.Instance.SFXGroup;
        }

        if (teleportSound == null)
            teleportSound = Resources.Load<AudioClip>("Audio/teleport_whoosh");
    }

    // ── Public API ─────────────────────────────────────────────

    /// <summary>
    /// Fades to black, plays teleport sound, loads the scene, then fades back in.
    /// Safe to call from any script — ignored if a transition is already running.
    /// </summary>
    public void TransitionToScene(string sceneName)
    {
        if (isTransitioning) return;
        StartCoroutine(DoTransition(sceneName));
    }

    // ── Coroutine ──────────────────────────────────────────────

    private IEnumerator DoTransition(string sceneName)
    {
        isTransitioning        = true;
        overlay.blocksRaycasts = true;

        // Play teleport whoosh
        if (teleportSound != null)
            audioSource.PlayOneShot(teleportSound, teleportVolume);

        // Fade screen to black
        yield return StartCoroutine(Fade(0f, 1f, fadeOutDuration));

        // Load scene asynchronously, hold until ready
        AsyncOperation op       = SceneManager.LoadSceneAsync(sceneName);
        op.allowSceneActivation = false;

        while (op.progress < 0.9f)
            yield return null;

        // Silence listener before the scene activates — nothing audible while volumes settle.
        AudioListener.volume = 0f;
        if (SettingsManager.Instance != null)
            SettingsManager.Instance.ApplyMixerSettings();

        // Activate scene — Awake/Start run and PlayOnAwake sources start,
        // but the listener is muted so nothing is heard.
        op.allowSceneActivation = true;
        yield return null;

        // Force-apply all saved volumes now that scene objects exist.
        // Done while listener is still muted so there is no audible pop.
        if (SettingsManager.Instance != null)
            SettingsManager.Instance.ApplyAllSettings();

        yield return null;
        AudioListener.volume = 1f;

        // Fade back in from black
        yield return StartCoroutine(Fade(1f, 0f, fadeInDuration));

        overlay.blocksRaycasts = false;
        isTransitioning        = false;
    }

    private IEnumerator Fade(float from, float to, float duration)
    {
        float elapsed = 0f;
        overlay.alpha = from;

        while (elapsed < duration)
        {
            elapsed      += Time.unscaledDeltaTime;
            overlay.alpha = Mathf.Lerp(from, to, elapsed / duration);
            yield return null;
        }

        overlay.alpha = to;
    }
}
