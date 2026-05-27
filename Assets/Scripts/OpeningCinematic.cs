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
    private AudioSource _music;

    private Coroutine _kbCo;
    private Coroutine _typeCo;
    private Coroutine _textDelayCo;

    private bool _endActive;
    private bool _leaving;

    private void Awake()
    {
        _font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        BuildUI();
        SetupAudio();
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

    private void SetupAudio()
    {
        AudioClip clip = Resources.Load<AudioClip>(FrameResourceFolder + "opening-music");
        if (clip == null) return;
        _music = gameObject.AddComponent<AudioSource>();
        _music.clip = clip;
        _music.loop = true;
        _music.volume = 0.6f;
        _music.Play();
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
            S(1, 7f, KB.PanRight, narr: "A small group of travelers crosses an endless desert."),
            S(2, 10f, KB.ZoomIn, spk: "LUCIAN (inner voice)",
                dlg: "\"We had been walking for two days straight. No map. No compass. Just the sun ahead of us and the road we thought we knew.\""),
            S(3, 6.5f, KB.ZoomOut, spk: "LUCIAN", dlg: "\"We make camp before nightfall.  We rest here.\""),
            S(4, 5.5f, KB.ZoomIn),
            S(5, 5.5f, KB.ZoomIn),
            S(6, 7.5f, KB.ZoomIn, spk: "LUCIAN (inner voice)",
                dlg: "\"I should have kept watch. I should have seen them coming.\""),
            S(7, 5f, KB.ZoomOut),
            S(8, 5f, KB.ZoomIn),
            S(9, 5.5f, KB.PanLeft),
            S(10, 4.5f, KB.ZoomIn),
            S(11, 4.5f, KB.Shake),
            S(12, 4.5f, KB.Shake),
            S(13, 4f, KB.ZoomIn),
            S(14, 4.5f, KB.ZoomIn, spk: "LUCIAN", dlg: "\"GET OFF — LET GO OF ME!\""),
            S(15, 4.5f, KB.PanRight),
            S(16, 4.5f, KB.ZoomIn, spk: "FRIEND", dlg: "\"LUCIAN — !\""),
            S(17, 4.5f, KB.PanLeft),
            S(18, 3f, KB.ZoomIn),
            S(19, 2.2f, KB.ZoomIn, flash: true),
            S(20, 4.5f, KB.ZoomOut),
            S(21, 5f, KB.None, black: true),
            S(22, 5f, KB.ZoomIn),
            S(23, 5f, KB.ZoomIn),
            S(24, 5f, KB.PanRight),
            S(25, 6f, KB.ZoomIn),
            S(26, 7.5f, KB.ZoomIn, spk: "LUCIAN (inner voice)",
                dlg: "\"They took everything.  They took everyone.\""),
            S(27, 7.5f, KB.ZoomIn, spk: "LUCIAN", dlg: "\"But they left me alive.  That was their mistake.\""),
            S(28, 6.5f, KB.ZoomOut),
            S(29, 8f, KB.ZoomIn, title: true),
            S(30, 12f, KB.None, end: true),
        };
    }

    private static Scene S(int img, float dur, KB kb, string narr = null,
        string spk = null, string dlg = null, bool black = false,
        bool title = false, bool end = false, bool flash = false)
    {
        return new Scene
        {
            img = img, dur = dur, kb = kb, narration = narr,
            speaker = spk, dialogue = dlg, blackOverlay = black,
            titleCard = title, endText = end, flash = flash,
        };
    }
}
