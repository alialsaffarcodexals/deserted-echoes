// ─────────────────────────────────────────────────────────────
// OpeningCinematic.cs
// Deserted Echoes | IT8101 Games Development | Group 3
// Author: Ali Husain Ali Alsaffar (202301152)
// Description: Plays the opening cinematic (ported from the
//              "Deserted Echoes — Opening Cinematic" HTML reel) as
//              the first scene in build order. Builds its whole UI
//              in code, runs the image/Ken-Burns/dialogue timeline,
//              and on the final "PRESS ANY KEY" screen loads the
//              main-menu via SceneLoader.
//
//              Attach to a single GameObject in the Opening scene.
//              Frames are loaded from Resources/OpeningCinematic/1..30.
//              Optional looped score: drop an AudioClip at
//              Resources/OpeningCinematic/opening-music.
// ─────────────────────────────────────────────────────────────

using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class OpeningCinematic : MonoBehaviour
{
    private const string NextScene = "main-menu";
    private const string FrameResourceFolder = "OpeningCinematic/";

    private enum KB { None, ZoomIn, ZoomOut, PanRight, PanLeft, Shake }

    private struct Scene
    {
        public int img;
        public float dur;            // seconds
        public KB kb;
        public string narration;
        public string speaker;
        public string dialogue;
        public bool blackOverlay;
        public bool titleCard;
        public bool endText;
        public bool flash;
        // audio
        public string music;
        public float wind;
        public float fire;
        public bool steps;
        public bool rush;
        public bool slam;
        public int shout;            // 0 none, 1 lucian, 2 friend
        public bool thud;
    }

    // ── Colours (from the original cinematic CSS) ────────────
    private static readonly Color Parchment = new Color(0.941f, 0.839f, 0.659f);   // #f0d6a8
    private static readonly Color Border    = new Color(0.784f, 0.600f, 0.408f);   // #c89968
    private static readonly Color DialogBg  = new Color(0.078f, 0.047f, 0.024f, 0.94f);
    private static readonly Color SpeakerBg = new Color(0.165f, 0.086f, 0.027f);   // #2a1607
    private static readonly Color TitleGold = new Color(0.941f, 0.784f, 0.439f);   // #f0c870
    private static readonly Color PromptGold = new Color(0.902f, 0.663f, 0.239f);  // #e6a93d
    private static readonly Color EndDim    = new Color(0.078f, 0.047f, 0.024f, 0.5f);

    private readonly Scene[] _scenes = BuildScenes();

    // ── Runtime UI references (built in Awake) ───────────────
    private RawImage _frame;
    private CanvasGroup _frameGroup;
    private Image _flashOverlay;
    private Image _blackOverlay;

    private GameObject _dialogBox;
    private Text _dialogSpeaker;
    private Text _dialogText;

    private CanvasGroup _narrationGroup;
    private Text _narrationText;

    private CanvasGroup _titleGroup;
    private CanvasGroup _endGroup;

    private Font _font;
    private OpeningCinematicAudio _audio;

    private Coroutine _kbCo;
    private Coroutine _typeCo;
    private Coroutine _textDelayCo;

    private bool _endActive;
    private bool _leaving;

    private void Awake()
    {
        _font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        BuildUI();
        _audio = gameObject.AddComponent<OpeningCinematicAudio>();
        _audio.Init();
    }

    private void Start()
    {
        StartCoroutine(RunTimeline());
    }

    private void Update()
    {
        if (_endActive && !_leaving && Input.anyKeyDown)
        {
            _leaving = true;
            SceneLoader.LoadScene(NextScene);
        }
    }

    // ── Timeline ─────────────────────────────────────────────
    private IEnumerator RunTimeline()
    {
        for (int i = 0; i < _scenes.Length; i++)
        {
            ApplyScene(_scenes[i]);
            if (_scenes[i].endText)
            {
                EnableEndScreen();
                yield break;          // hold here; key press exits
            }
            yield return new WaitForSeconds(_scenes[i].dur);
        }
    }

    private void ApplyScene(Scene s)
    {
        // Reset overlays
        _dialogBox.SetActive(false);
        _narrationGroup.alpha = 0f;
        _titleGroup.alpha = 0f;
        _endGroup.alpha = 0f;
        _blackOverlay.gameObject.SetActive(s.blackOverlay);

        if (_textDelayCo != null) StopCoroutine(_textDelayCo);
        if (_typeCo != null) StopCoroutine(_typeCo);

        // Swap image (with quick fade dip) + Ken Burns
        if (_kbCo != null) StopCoroutine(_kbCo);
        _kbCo = StartCoroutine(SwapAndAnimate(s));

        if (s.flash) StartCoroutine(FlashWhite());

        // Audio
        _audio.SetMusic(s.music);
        _audio.SetFire(s.fire);
        if (s.rush) _audio.PlayRush();
        if (s.slam) { _audio.PlayWhoosh(0.6f); StartCoroutine(DelayThen(0.3f, () => _audio.PlayWhoosh(0.5f))); }
        if (s.shout == 1) _audio.PlayShout(true);
        else if (s.shout == 2) StartCoroutine(DelayThen(0.6f, () => _audio.PlayShout(false)));
        if (s.thud) StartCoroutine(DelayThen(0.4f, () => _audio.PlayThud()));

        // Scheduled text
        if (!string.IsNullOrEmpty(s.dialogue))
            _textDelayCo = StartCoroutine(DelayThen(0.7f, () => ShowDialogue(s.speaker, s.dialogue)));
        else if (!string.IsNullOrEmpty(s.narration))
            _textDelayCo = StartCoroutine(DelayThen(0.8f, () => ShowNarration(s.narration)));

        if (s.titleCard)
            StartCoroutine(DelayThen(1.5f, () => StartCoroutine(FadeIn(_titleGroup, 1.2f))));
    }

    private IEnumerator SwapAndAnimate(Scene s)
    {
        // Fade out current frame
        yield return Fade(_frameGroup, _frameGroup.alpha, 0f, 0.2f);

        Texture2D tex = Resources.Load<Texture2D>(FrameResourceFolder + s.img);
        if (tex != null) _frame.texture = tex;

        // Reset transform then start the Ken Burns drift
        RectTransform rt = _frame.rectTransform;
        rt.localScale = Vector3.one;
        rt.anchoredPosition = Vector2.zero;

        yield return Fade(_frameGroup, 0f, 1f, 0.4f);
        yield return KenBurns(rt, s.kb, s.dur);
    }

    private IEnumerator KenBurns(RectTransform rt, KB kb, float dur)
    {
        // Overscan so scaled/panned frames never reveal screen edges.
        const float pan = 1.15f, panShift = 0.05f;
        Vector3 sFrom = Vector3.one, sTo = Vector3.one;
        Vector2 pFrom = Vector2.zero, pTo = Vector2.zero;
        float w = Screen.width;

        switch (kb)
        {
            case KB.ZoomIn:  sFrom = Vector3.one; sTo = new Vector3(1.22f, 1.22f, 1f); break;
            case KB.ZoomOut: sFrom = new Vector3(1.22f, 1.22f, 1f); sTo = Vector3.one; break;
            case KB.PanRight:
                sFrom = sTo = new Vector3(pan, pan, 1f);
                pFrom = new Vector2(w * panShift, 0f); pTo = new Vector2(-w * panShift, 0f); break;
            case KB.PanLeft:
                sFrom = sTo = new Vector3(pan, pan, 1f);
                pFrom = new Vector2(-w * panShift, 0f); pTo = new Vector2(w * panShift, 0f); break;
            case KB.Shake:
                yield return ShakeLoop(rt, dur); yield break;
            case KB.None:
            default:
                rt.localScale = new Vector3(1.04f, 1.04f, 1f);
                yield break;
        }

        rt.localScale = sFrom;
        rt.anchoredPosition = pFrom;
        float t = 0f;
        while (t < dur)
        {
            t += Time.deltaTime;
            float k = Mathf.Clamp01(t / dur);
            rt.localScale = Vector3.Lerp(sFrom, sTo, k);
            rt.anchoredPosition = Vector2.Lerp(pFrom, pTo, k);
            yield return null;
        }
    }

    private IEnumerator ShakeLoop(RectTransform rt, float dur)
    {
        rt.localScale = new Vector3(1.15f, 1.15f, 1f);
        float t = 0f;
        while (t < dur)
        {
            t += Time.deltaTime;
            rt.anchoredPosition = new Vector2(Random.Range(-5f, 5f), Random.Range(-5f, 5f));
            yield return null;
        }
        rt.anchoredPosition = Vector2.zero;
    }

    // ── Text helpers ─────────────────────────────────────────
    private void ShowDialogue(string speaker, string text)
    {
        _dialogBox.SetActive(true);
        _dialogSpeaker.text = speaker;
        _dialogSpeaker.gameObject.SetActive(!string.IsNullOrEmpty(speaker));
        if (_typeCo != null) StopCoroutine(_typeCo);
        _typeCo = StartCoroutine(Typewriter(text));
    }

    private IEnumerator Typewriter(string text)
    {
        _dialogText.text = "";
        for (int i = 0; i <= text.Length; i++)
        {
            _dialogText.text = text.Substring(0, i);
            if (i < text.Length && text[i] != ' ' && i % 2 == 0) _audio.PlayBeep();
            yield return new WaitForSeconds(0.038f);
        }
    }

    private void ShowNarration(string text)
    {
        _narrationText.text = text;
        StartCoroutine(FadeIn(_narrationGroup, 1f));
    }

    private void EnableEndScreen()
    {
        _titleGroup.alpha = 1f;       // pinned title (set higher in layout)
        StartCoroutine(DelayThen(0.8f, () => StartCoroutine(FadeIn(_endGroup, 1f))));
        _endActive = true;
    }

    // ── FX ───────────────────────────────────────────────────
    private IEnumerator FlashWhite()
    {
        _flashOverlay.color = new Color(1f, 1f, 1f, 0.95f);
        float t = 0f;
        while (t < 0.4f)
        {
            t += Time.deltaTime;
            _flashOverlay.color = new Color(1f, 1f, 1f, Mathf.Lerp(0.95f, 0f, t / 0.4f));
            yield return null;
        }
        _flashOverlay.color = new Color(1f, 1f, 1f, 0f);
    }

    private IEnumerator DelayThen(float delay, System.Action action)
    {
        yield return new WaitForSeconds(delay);
        action();
    }

    private IEnumerator FadeIn(CanvasGroup g, float dur) => Fade(g, g.alpha, 1f, dur);

    private IEnumerator Fade(CanvasGroup g, float from, float to, float dur)
    {
        float t = 0f;
        g.alpha = from;
        while (t < dur)
        {
            t += Time.deltaTime;
            g.alpha = Mathf.Lerp(from, to, t / dur);
            yield return null;
        }
        g.alpha = to;
    }

    // ── UI construction ──────────────────────────────────────
    private void BuildUI()
    {
        // Canvas
        var canvasGo = new GameObject("OpeningCanvas",
            typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        canvasGo.transform.SetParent(transform, false);
        var canvas = canvasGo.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 1000;
        var scaler = canvasGo.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.matchWidthOrHeight = 0.5f;
        Transform root = canvasGo.transform;

        // Black background behind everything
        Image bg = MakeImage("Background", root, Color.black);
        Stretch(bg.rectTransform);

        // Frame (image) + crossfade group
        _frame = new GameObject("Frame", typeof(RawImage), typeof(CanvasGroup))
            .GetComponent<RawImage>();
        _frame.transform.SetParent(root, false);
        Stretch(_frame.rectTransform);
        _frameGroup = _frame.GetComponent<CanvasGroup>();
        _frameGroup.alpha = 0f;

        // Letterbox bars (8% top & bottom)
        Image barTop = MakeImage("LetterboxTop", root, Color.black);
        AnchorBar(barTop.rectTransform, true);
        Image barBottom = MakeImage("LetterboxBottom", root, Color.black);
        AnchorBar(barBottom.rectTransform, false);

        // Narration (centred lower)
        var narrGo = new GameObject("Narration", typeof(CanvasGroup));
        narrGo.transform.SetParent(root, false);
        _narrationGroup = narrGo.GetComponent<CanvasGroup>();
        _narrationGroup.alpha = 0f;
        var narrRt = narrGo.AddComponent<RectTransform>();
        narrRt.anchorMin = new Vector2(0.1f, 0.14f);
        narrRt.anchorMax = new Vector2(0.9f, 0.32f);
        narrRt.offsetMin = narrRt.offsetMax = Vector2.zero;
        _narrationText = MakeText("NarrationText", narrGo.transform, 34, TextAnchor.MiddleCenter);
        _narrationText.fontStyle = FontStyle.Italic;
        _narrationText.color = new Color(0.91f, 0.847f, 0.706f);
        Stretch(_narrationText.rectTransform);

        // Dialogue box (bottom centre)
        _dialogBox = MakeImage("DialogueBox", root, DialogBg).gameObject;
        var dlgRt = _dialogBox.GetComponent<RectTransform>();
        dlgRt.anchorMin = new Vector2(0.14f, 0.16f);
        dlgRt.anchorMax = new Vector2(0.86f, 0.30f);
        dlgRt.offsetMin = dlgRt.offsetMax = Vector2.zero;
        var dlgOutline = _dialogBox.AddComponent<Outline>();
        dlgOutline.effectColor = Border;
        dlgOutline.effectDistance = new Vector2(3, -3);

        _dialogSpeaker = MakeText("Speaker", _dialogBox.transform, 20, TextAnchor.UpperLeft);
        _dialogSpeaker.color = Parchment;
        _dialogSpeaker.fontStyle = FontStyle.Bold;
        var spkRt = _dialogSpeaker.rectTransform;
        spkRt.anchorMin = new Vector2(0f, 1f); spkRt.anchorMax = new Vector2(1f, 1f);
        spkRt.pivot = new Vector2(0f, 1f);
        spkRt.anchoredPosition = new Vector2(24, 22);
        spkRt.sizeDelta = new Vector2(-48, 30);

        _dialogText = MakeText("DialogueText", _dialogBox.transform, 30, TextAnchor.MiddleLeft);
        _dialogText.color = Parchment;
        var dtRt = _dialogText.rectTransform;
        Stretch(dtRt);
        dtRt.offsetMin = new Vector2(28, 16);
        dtRt.offsetMax = new Vector2(-28, -28);

        // Title card (DESERTED / ECHOES) pinned to upper third
        var titleGo = new GameObject("TitleCard", typeof(CanvasGroup));
        titleGo.transform.SetParent(root, false);
        _titleGroup = titleGo.GetComponent<CanvasGroup>();
        _titleGroup.alpha = 0f;
        var titleRt = titleGo.AddComponent<RectTransform>();
        titleRt.anchorMin = new Vector2(0f, 0.55f);
        titleRt.anchorMax = new Vector2(1f, 0.9f);
        titleRt.offsetMin = titleRt.offsetMax = Vector2.zero;
        var titleText = MakeText("TitleWords", titleGo.transform, 90, TextAnchor.MiddleCenter);
        titleText.text = "DESERTED\nECHOES";
        titleText.color = TitleGold;
        titleText.fontStyle = FontStyle.Bold;
        Stretch(titleText.rectTransform);

        // End screen (text + blinking prompt)
        var endGo = MakeImage("EndScreen", root, EndDim).gameObject;
        Stretch(endGo.GetComponent<RectTransform>());
        _endGroup = endGo.AddComponent<CanvasGroup>();
        _endGroup.alpha = 0f;

        var endLines = MakeText("EndLines", endGo.transform, 30, TextAnchor.MiddleCenter);
        endLines.text =
            "\"His friends are out there.\nScattered. Captured. Waiting.\"\n\n" +
            "Survive the desert.\nFind them.\nBring them home.";
        endLines.color = Parchment;
        var elRt = endLines.rectTransform;
        elRt.anchorMin = new Vector2(0.1f, 0.30f);
        elRt.anchorMax = new Vector2(0.9f, 0.54f);
        elRt.offsetMin = elRt.offsetMax = Vector2.zero;

        var prompt = MakeText("EndPrompt", endGo.transform, 22, TextAnchor.MiddleCenter);
        prompt.text = "► PRESS ANY KEY TO BEGIN";
        prompt.color = PromptGold;
        prompt.fontStyle = FontStyle.Bold;
        var pRt = prompt.rectTransform;
        pRt.anchorMin = new Vector2(0.1f, 0.16f);
        pRt.anchorMax = new Vector2(0.9f, 0.24f);
        pRt.offsetMin = pRt.offsetMax = Vector2.zero;
        prompt.gameObject.AddComponent<BlinkText>();

        // FX overlays (on top of everything except nothing here)
        _flashOverlay = MakeImage("FxFlash", root, new Color(1f, 1f, 1f, 0f));
        Stretch(_flashOverlay.rectTransform);
        _flashOverlay.raycastTarget = false;

        _blackOverlay = MakeImage("FxBlack", root, Color.black);
        Stretch(_blackOverlay.rectTransform);
        _blackOverlay.raycastTarget = false;
        _blackOverlay.gameObject.SetActive(false);

        // Make sure FX sit above frame but below text where appropriate
        _frame.transform.SetSiblingIndex(1);
        barTop.transform.SetAsLastSibling();
        barBottom.transform.SetAsLastSibling();
        _flashOverlay.transform.SetAsLastSibling();

        _dialogBox.SetActive(false);
    }

    private Image MakeImage(string name, Transform parent, Color color)
    {
        var go = new GameObject(name, typeof(Image));
        go.transform.SetParent(parent, false);
        var img = go.GetComponent<Image>();
        img.color = color;
        return img;
    }

    private Text MakeText(string name, Transform parent, int size, TextAnchor anchor)
    {
        var go = new GameObject(name, typeof(Text));
        go.transform.SetParent(parent, false);
        var t = go.GetComponent<Text>();
        t.font = _font;
        t.fontSize = size;
        t.alignment = anchor;
        t.horizontalOverflow = HorizontalWrapMode.Wrap;
        t.verticalOverflow = VerticalWrapMode.Overflow;
        t.raycastTarget = false;
        Stretch(t.rectTransform);
        return t;
    }

    private static void Stretch(RectTransform rt)
    {
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
        rt.pivot = new Vector2(0.5f, 0.5f);
    }

    private static void AnchorBar(RectTransform rt, bool top)
    {
        rt.anchorMin = new Vector2(0f, top ? 0.92f : 0f);
        rt.anchorMax = new Vector2(1f, top ? 1f : 0.08f);
        rt.offsetMin = rt.offsetMax = Vector2.zero;
    }

    // ── Scene data (ported from the HTML SCENES array) ───────
    private static Scene[] BuildScenes()
    {
        return new[]
        {
            new Scene { img = 1, dur = 7f, kb = KB.PanRight, music = "warm", wind = 0.18f, steps = true,
                narration = "A small group of travelers crosses an endless desert." },
            new Scene { img = 2, dur = 10f, kb = KB.ZoomIn, music = "warm", wind = 0.16f, steps = true,
                speaker = "LUCIAN (inner voice)",
                dialogue = "\"We had been walking for two days straight. No map. No compass. Just the sun ahead of us and the road we thought we knew.\"" },
            new Scene { img = 3, dur = 6.5f, kb = KB.ZoomOut, music = "warm", wind = 0.14f, fire = 0.05f,
                speaker = "LUCIAN", dialogue = "\"We make camp before nightfall.  We rest here.\"" },
            new Scene { img = 4, dur = 5.5f, kb = KB.ZoomIn, music = "warm-soft", wind = 0.12f, fire = 0.32f },
            new Scene { img = 5, dur = 5.5f, kb = KB.ZoomIn, music = "warm-soft", wind = 0.14f, fire = 0.32f },
            new Scene { img = 6, dur = 7.5f, kb = KB.ZoomIn, music = "silent", wind = 0.16f, fire = 0.30f,
                speaker = "LUCIAN (inner voice)", dialogue = "\"I should have kept watch. I should have seen them coming.\"" },
            new Scene { img = 7, dur = 5f, kb = KB.ZoomOut, music = "silent", wind = 0.18f, fire = 0.18f },
            new Scene { img = 8, dur = 5f, kb = KB.ZoomIn, music = "tense", wind = 0.20f, fire = 0.10f },
            new Scene { img = 9, dur = 5.5f, kb = KB.PanLeft, music = "tense", wind = 0.22f, fire = 0.05f },
            new Scene { img = 10, dur = 4.5f, kb = KB.ZoomIn, music = "tense", wind = 0.20f },
            new Scene { img = 11, dur = 4.5f, kb = KB.Shake, music = "combat", wind = 0.10f, slam = true, rush = true },
            new Scene { img = 12, dur = 4.5f, kb = KB.Shake, music = "combat", wind = 0.10f },
            new Scene { img = 13, dur = 4f, kb = KB.ZoomIn, music = "combat", wind = 0.10f },
            new Scene { img = 14, dur = 4.5f, kb = KB.ZoomIn, music = "combat", wind = 0.10f, shout = 1,
                speaker = "LUCIAN", dialogue = "\"GET OFF — LET GO OF ME!\"" },
            new Scene { img = 15, dur = 4.5f, kb = KB.PanRight, music = "combat", wind = 0.10f },
            new Scene { img = 16, dur = 4.5f, kb = KB.ZoomIn, music = "combat", wind = 0.10f, shout = 2,
                speaker = "FRIEND", dialogue = "\"LUCIAN — !\"" },
            new Scene { img = 17, dur = 4.5f, kb = KB.PanLeft, music = "combat", wind = 0.12f },
            new Scene { img = 18, dur = 3f, kb = KB.ZoomIn, music = "combat", wind = 0.12f },
            new Scene { img = 19, dur = 2.2f, kb = KB.ZoomIn, music = "combat", wind = 0.12f, thud = true, flash = true },
            new Scene { img = 20, dur = 4.5f, kb = KB.ZoomOut, music = "silent", wind = 0.18f },
            new Scene { img = 21, dur = 5f, kb = KB.None, music = "silent", wind = 0.12f, blackOverlay = true },
            new Scene { img = 22, dur = 5f, kb = KB.ZoomIn, music = "mournful", wind = 0.20f },
            new Scene { img = 23, dur = 5f, kb = KB.ZoomIn, music = "mournful", wind = 0.20f },
            new Scene { img = 24, dur = 5f, kb = KB.PanRight, music = "mournful", wind = 0.22f },
            new Scene { img = 25, dur = 6f, kb = KB.ZoomIn, music = "mournful", wind = 0.18f },
            new Scene { img = 26, dur = 7.5f, kb = KB.ZoomIn, music = "mournful", wind = 0.16f,
                speaker = "LUCIAN (inner voice)", dialogue = "\"They took everything.  They took everyone.\"" },
            new Scene { img = 27, dur = 7.5f, kb = KB.ZoomIn, music = "determined-low", wind = 0.16f,
                speaker = "LUCIAN", dialogue = "\"But they left me alive.  That was their mistake.\"" },
            new Scene { img = 28, dur = 6.5f, kb = KB.ZoomOut, music = "determined-mid", wind = 0.18f, steps = true },
            new Scene { img = 29, dur = 8f, kb = KB.ZoomIn, music = "thrilling-reveal", titleCard = true },
            new Scene { img = 30, dur = 12f, kb = KB.None, music = "thrilling-resolve", wind = 0.08f, endText = true },
        };
    }
}
