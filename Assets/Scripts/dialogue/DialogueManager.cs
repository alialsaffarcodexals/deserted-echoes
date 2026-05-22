using System.Collections;
using UnityEngine;
using UnityEngine.UI;
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
    private System.Collections.Generic.List<AudioSource> pausedMusicSources = new System.Collections.Generic.List<AudioSource>();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        // Route to mixer groups
        if (SettingsManager.Instance != null)
        {
            if (musicSource != null) musicSource.outputAudioMixerGroup = SettingsManager.Instance.MusicGroup;
            if (sfxSource != null) sfxSource.outputAudioMixerGroup = SettingsManager.Instance.SFXGroup;
        }

        dialogueRoot.SetActive(false);
    }

    private void Update()
    {
        if (current == null) return;

        // press enter to skip typing or go to next line
        if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
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

        PauseOtherMusic();

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

    private void PauseOtherMusic()
    {
        pausedMusicSources.Clear();
        AudioSource[] allSources = Object.FindObjectsByType<AudioSource>(FindObjectsSortMode.None);
        foreach (var src in allSources)
        {
            // If it's playing music (looping, 2D) and it's not our own music source
            if (src != musicSource && src.isPlaying && src.loop && src.spatialBlend == 0f)
            {
                src.Pause();
                pausedMusicSources.Add(src);
            }
        }
    }

    private void ResumeOtherMusic()
    {
        foreach (var src in pausedMusicSources)
        {
            if (src != null) src.UnPause();
        }
        pausedMusicSources.Clear();
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

            if (SaveManager.Instance != null)
            {
                SaveManager.Instance.MarkDialogueSeen(current.firstVisitKey);
                SaveManager.Instance.SaveGame();
            }
        }

        current = null;
        dialogueRoot.SetActive(false);
        musicSource.Stop();
        ResumeOtherMusic();
        Time.timeScale = 1f;
    }

    // helper to make the playerprefs key consistent
    public static string FirstVisitPref(string key) => $"dlg_seen_{key}";

    // used by dialoguetrigger to check if a convo was already seen
    public static bool HasSeen(string key)
    {
        if (string.IsNullOrEmpty(key))
            return false;

        if (SaveManager.Instance != null)
            return SaveManager.Instance.HasSeenDialogue(key);

        return PlayerPrefs.GetInt(FirstVisitPref(key), 0) == 1;
    }
}
