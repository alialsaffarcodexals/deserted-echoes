// ─────────────────────────────────────────────────────────────
// FootstepSounds.cs
// Deserted Echoes | IT8101 Games Development | Group 3
// Author: Ali Husain Ali Alsaffar
// Sprint: 4 | Updated: May 14, 2026
// Description: Attach to the Player. Plays a looping footstep
//              sound while the player is moving. Surface type is
//              set externally by zone scripts (SandZone, CarpetZone).
//              Default surface is Wood (used in House-Interior).
// ─────────────────────────────────────────────────────────────

using UnityEngine;

public class FootstepSounds : MonoBehaviour
{
    public enum SurfaceType { Wood, Carpet, Sand }

    [Header("Surface Sounds")]
    public AudioClip woodSound;
    public AudioClip carpetSound;
    public AudioClip sandSound;

    [Header("Volumes")]
    public float woodVolume   = 1f;
    public float carpetVolume = 1f;
    public float sandVolume   = 1f;

    [Header("Timing")]
    public float footstepInterval    = 0.4f;
    public float runFootstepInterval = 0.2f;

    private AudioSource audioSource;
    private float footstepTimer = 0f;
    private SurfaceType currentSurface = SurfaceType.Wood;
    private Vector3 lastPosition;
    private PlayerController playerController;

    void Start()
    {
        audioSource      = GetComponent<AudioSource>();
        playerController = GetComponent<PlayerController>();
        lastPosition     = transform.position;
        footstepTimer    = footstepInterval;
    }

    void Update()
    {
        bool isMoving = Vector3.Distance(transform.position, lastPosition) > 0.001f;
        lastPosition  = transform.position;

        if (isMoving)
        {
            float activeInterval = (playerController != null && playerController.IsSprinting)
                ? runFootstepInterval
                : footstepInterval;

            footstepTimer += Time.deltaTime;

            if (footstepTimer >= activeInterval)
            {
                PlayCurrentSurface();
                footstepTimer = 0f;
            }
        }
        else
        {
            audioSource.Stop();
            footstepTimer = footstepInterval;
        }
    }

    private void PlayCurrentSurface()
    {
        switch (currentSurface)
        {
            case SurfaceType.Sand:
                if (sandSound != null)
                    audioSource.PlayOneShot(sandSound, sandVolume);
                break;
            case SurfaceType.Carpet:
                if (carpetSound != null)
                    audioSource.PlayOneShot(carpetSound, carpetVolume);
                break;
            default:
                if (woodSound != null)
                    audioSource.PlayOneShot(woodSound, woodVolume);
                break;
        }
    }

    /// <summary>Called by SandZone / CarpetZone to switch the active surface.</summary>
    public void SetSurface(SurfaceType surface)
    {
        currentSurface = surface;
        audioSource.Stop();
        footstepTimer = footstepInterval;
    }

    /// <summary>Backward-compatible shim — still works with existing CarpetZone.</summary>
    public void SetOnCarpet(bool onCarpet)
    {
        SetSurface(onCarpet ? SurfaceType.Carpet : SurfaceType.Wood);
    }

    /// <summary>Called by SettingsPanelUI to control SFX volume at runtime.</summary>
    public void SetVolume(float volume)
    {
        if (audioSource != null) audioSource.volume = volume;
    }
}
