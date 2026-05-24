using UnityEngine;
using UnityEngine.UI;
using System;

public class TeleportSelectionUI : MonoBehaviour
{
    public static TeleportSelectionUI Instance { get; private set; }

    [Header("UI Panels")]
    [SerializeField] private GameObject panel;

    [Header("Buttons")]
    public Button centralButton;
    public Button normalButton;
    public Button closeButton;

    [Header("Audio")]
    [SerializeField] private AudioSource uiAudio;
    [SerializeField] private AudioClip buttonClickClip;
    [SerializeField] private AudioClip menuOpenClip;

    private Action<bool> onChoiceSelected;  // true = Central_Teleport, false = normal (last position)
    private Action onCancelled;

    private void Awake()
    {
        // This panel exists in every scene that can teleport to Open-World, so the
        // newest scene's instance must always claim Instance. Deferring to a prior
        // scene's instance (which is mid-unload) left Instance pointing at a destroyed
        // object, so PortalSpawnManager saw it as null and skipped the panel.
        Instance = this;

        if (panel != null) panel.SetActive(false);

        if (centralButton != null) centralButton.onClick.AddListener(OnCentralSelected);
        if (normalButton != null) normalButton.onClick.AddListener(OnNormalSelected);
        if (closeButton != null) closeButton.onClick.AddListener(OnCloseSelected);
    }

    private void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }

    /// <summary>
    /// Presents the destination choice. <paramref name="onSelected"/> receives true
    /// for the central teleport or false for a normal teleport. <paramref name="onCancel"/>
    /// fires if the player closes the panel without choosing.
    /// </summary>
    public void Show(Action<bool> onSelected, Action onCancel = null)
    {
        Debug.Log("Showing Teleport Selection UI.");
        onChoiceSelected = onSelected;
        onCancelled = onCancel;

        if (panel != null) panel.SetActive(true);
        PlaySound(menuOpenClip);
        Time.timeScale = 0f; // Pause while choosing
    }

    private void OnCentralSelected()
    {
        PlaySound(buttonClickClip);
        Finish(() => onChoiceSelected?.Invoke(true));
    }

    private void OnNormalSelected()
    {
        PlaySound(buttonClickClip);
        Finish(() => onChoiceSelected?.Invoke(false));
    }

    private void OnCloseSelected()
    {
        PlaySound(buttonClickClip);
        Finish(() => onCancelled?.Invoke());
    }

    private void Finish(Action callback)
    {
        if (panel != null) panel.SetActive(false);
        Time.timeScale = 1f;

        // Hide the whole modal (this root carries the dimmer overlay) before
        // running the callback, which may start a scene transition.
        gameObject.SetActive(false);

        callback?.Invoke();
    }

    private void PlaySound(AudioClip clip)
    {
        if (uiAudio != null && clip != null)
            uiAudio.PlayOneShot(clip);
    }
}
