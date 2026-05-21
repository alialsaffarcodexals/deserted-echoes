using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using TMPro;

// main dialogue script handles showing the box, typing text, music, pause, everything
// only one of these should exist in the game (singleton)
public class DialogueManager : MonoBehaviour
{
    public static DialogueManager Instance { get; private set; }

    [Header("UI References")]
    [SerializeField] private GameObject dialogueRoot;
    [SerializeField] private TMP_Text dialogueText;
    [SerializeField] private Image portraitImage;

    [Header("Default Portrait")]
    [SerializeField] private Sprite playerPortrait;

    [Header("Audio")]
    [SerializeField] private AudioSource musicSource;
    [SerializeField] private AudioSource sfxSource;
    [SerializeField] private AudioClip calmTrack;
    [SerializeField] private AudioClip tenseTrack;
    [SerializeField] private AudioClip blipClip;
    [SerializeField, Range(0f, 1f)] private float musicVolume = 0.4f;
    [SerializeField, Range(0f, 1f)] private float blipVolume = 0.6f;

    [Header("Typewriter")]
    [SerializeField] private float charactersPerSecond = 35f;
    [SerializeField] private int blipEveryNChars = 2;

    private Conversation current;
    private int lineIndex;
    private Coroutine typingRoutine;
    private bool isTyping;
    private string currentFullLine;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        dialogueRoot.SetActive(false);
    }

    private void Update()
    {
        if (current == null) return;

        // press enter to skip typing or go to next line
        bool enterPressed = Keyboard.current != null &&
            (Keyboard.current.enterKey.wasPressedThisFrame ||
             Keyboard.current.numpadEnterKey.wasPressedThisFrame);

        if (enterPressed)
        {
            if (isTyping) FinishLineInstantly();
            else AdvanceLine();
        }
    }

    // called from outside to start a conversation
    public void StartConversation(Conversation conversation)
    {
        if (conversation == null || conversation.lines == null || conversation.lines.Length == 0) return;
        if (current != null) return;

        current = conversation;
        lineIndex = 0;

        Time.timeScale = 0f;
        dialogueRoot.SetActive(true);

        // pick the music based on the mood set on the conversation asset
        var clip = conversation.mood == DialogueMood.Tense ? tenseTrack : calmTrack;
        if (clip != null)
        {
            musicSource.clip = clip;
            musicSource.volume = musicVolume;
            musicSource.loop = true;
            musicSource.ignoreListenerPause = true;
            musicSource.Play();
        }

        ShowLine(current.lines[0]);
    }

    private void ShowLine(DialogueLine line)
    {
        portraitImage.sprite = line.portraitOverride != null ? line.portraitOverride : playerPortrait;
        currentFullLine = line.text;
        if (typingRoutine != null) StopCoroutine(typingRoutine);
        typingRoutine = StartCoroutine(TypeLine(currentFullLine));
    }

    // types the line one letter at a time and plays the blip
    private IEnumerator TypeLine(string text)
    {
        isTyping = true;
        dialogueText.text = "";
        float delay = 1f / Mathf.Max(1f, charactersPerSecond);

        for (int i = 0; i < text.Length; i++)
        {
            dialogueText.text += text[i];

            if (!char.IsWhiteSpace(text[i]) && i % blipEveryNChars == 0 && blipClip != null)
            {
                sfxSource.PlayOneShot(blipClip, blipVolume);
            }

            // realtime because time.timescale is 0 during dialogue
            yield return new WaitForSecondsRealtime(delay);
        }

        isTyping = false;
        typingRoutine = null;
    }

    private void FinishLineInstantly()
    {
        if (typingRoutine != null) StopCoroutine(typingRoutine);
        dialogueText.text = currentFullLine;
        isTyping = false;
        typingRoutine = null;
    }

    private void AdvanceLine()
    {
        lineIndex++;
        if (lineIndex >= current.lines.Length)
        {
            EndConversation();
            return;
        }
        ShowLine(current.lines[lineIndex]);
    }

    private void EndConversation()
    {
        // save that the player has seen this convo (only if it has a key)
        if (!string.IsNullOrEmpty(current.firstVisitKey))
        {
            PlayerPrefs.SetInt(FirstVisitPref(current.firstVisitKey), 1);
            PlayerPrefs.Save();
        }

        current = null;
        dialogueRoot.SetActive(false);
        musicSource.Stop();
        Time.timeScale = 1f;
    }

    // helper to make the playerprefs key consistent
    public static string FirstVisitPref(string key) => $"dlg_seen_{key}";

    // used by dialoguetrigger to check if a convo was already seen
    public static bool HasSeen(string key) =>
        !string.IsNullOrEmpty(key) && PlayerPrefs.GetInt(FirstVisitPref(key), 0) == 1;
}