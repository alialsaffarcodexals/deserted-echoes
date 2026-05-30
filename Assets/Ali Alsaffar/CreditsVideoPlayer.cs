using System;
using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

public class CreditsVideoPlayer : MonoBehaviour
{
    [SerializeField] private string returnScene = "main-menu";

    private VideoPlayer _vp;
    private RawImage    _screen;
    private TextMeshProUGUI _hud;
    private RenderTexture   _rt;
    private Action _onClose;
    private bool   _isPaused;

    public static void Play(Action onClose)
    {
        var go   = new GameObject("CreditsVideoOverlay");
        var comp = go.AddComponent<CreditsVideoPlayer>();
        comp._onClose = onClose;
    }

    private void Awake()
    {
        BuildUI();
    }

    private void Start()
    {
        BuildVideoPlayer();
    }

    // ── UI Construction ──────────────────────────────────────

    private static T GetOrAdd<T>(GameObject go) where T : Component
    {
        T comp = go.GetComponent<T>();
        return comp != null ? comp : go.AddComponent<T>();
    }

    private void BuildUI()
    {
        var canvas = GetOrAdd<Canvas>(gameObject);
        canvas.renderMode   = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 999;
        GetOrAdd<CanvasScaler>(gameObject);
        GetOrAdd<GraphicRaycaster>(gameObject);

        var bg    = new GameObject("Background");
        bg.transform.SetParent(transform, false);
        var bgImg = bg.AddComponent<RawImage>();
        bgImg.color = Color.black;
        Stretch(bgImg.rectTransform);

        var screenGo = new GameObject("VideoScreen");
        screenGo.transform.SetParent(transform, false);
        _screen = screenGo.AddComponent<RawImage>();
        Stretch(_screen.rectTransform);

        var hudGo = new GameObject("HUD");
        hudGo.transform.SetParent(transform, false);
        _hud           = hudGo.AddComponent<TextMeshProUGUI>();
        _hud.text      = "[Space]  Pause / Resume\n[R]       Replay\n[Esc]    Exit";
        _hud.fontSize  = 20;
        _hud.color     = new Color(1f, 1f, 1f, 0.90f);
        _hud.alignment = TextAlignmentOptions.TopLeft;
        var hudRect    = hudGo.GetComponent<RectTransform>();
        hudRect.anchorMin        = new Vector2(0f, 1f);
        hudRect.anchorMax        = new Vector2(0f, 1f);
        hudRect.pivot            = new Vector2(0f, 1f);
        hudRect.anchoredPosition = new Vector2(24f, -24f);
        hudRect.sizeDelta        = new Vector2(320f, 110f);
    }

    private static void Stretch(RectTransform rt)
    {
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = rt.offsetMax = Vector2.zero;
    }

    // ── Video Player Construction ────────────────────────────

    private void BuildVideoPlayer()
    {
        _rt             = new RenderTexture(1920, 1080, 0);
        _screen.texture = _rt;

        _vp = gameObject.AddComponent<VideoPlayer>();
        _vp.renderMode      = VideoRenderMode.RenderTexture;
        _vp.targetTexture   = _rt;
        _vp.audioOutputMode = VideoAudioOutputMode.AudioSource;
        _vp.playOnAwake     = false;
        _vp.isLooping       = false;
        _vp.skipOnDrop      = true;

        var audio = gameObject.AddComponent<AudioSource>();
        _vp.SetTargetAudioSource(0, audio);

        string path = Path.Combine(Application.streamingAssetsPath, "CreditsVideo.mp4");
        if (!File.Exists(path))
        {
            Debug.LogError("CreditsVideoPlayer: video not found at " + path);
            Close();
            return;
        }

        _vp.source = VideoSource.Url;
        _vp.url    = "file://" + path;

        _vp.prepareCompleted += _ => _vp.Play();
        _vp.loopPointReached += _ => Close();
        _vp.errorReceived    += (_, msg) => Debug.LogError("CreditsVideoPlayer: " + msg);
        _vp.Prepare();
    }

    // ── Input ────────────────────────────────────────────────

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape)) { Close(); return; }
        if (Input.GetKeyDown(KeyCode.Space))  { TogglePause(); return; }
        if (Input.GetKeyDown(KeyCode.R))      { Replay(); }
    }

    private void TogglePause()
    {
        _isPaused = !_isPaused;
        if (_isPaused) _vp.Pause(); else _vp.Play();
    }

    private void Replay()
    {
        _isPaused = false;
        _vp.time  = 0;
        _vp.Play();
    }

    // ── Cleanup ──────────────────────────────────────────────

    private void Close()
    {
        if (_rt != null) { _rt.Release(); Destroy(_rt); }
        Destroy(gameObject);
        if (_onClose != null)
            _onClose.Invoke();
        else if (!string.IsNullOrEmpty(returnScene))
            SceneLoader.LoadScene(returnScene);
    }
}
