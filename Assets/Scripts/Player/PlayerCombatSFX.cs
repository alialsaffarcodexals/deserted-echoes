using UnityEngine;

public class PlayerCombatSFX : MonoBehaviour
{
    [SerializeField] AudioClip weaponSwingClip;
    [SerializeField] AudioClip playerHurtClip;

    AudioSource audioSource;
    PlayerController playerController;

    void Awake()
    {
        // Add a dedicated AudioSource so FootstepSounds.Stop() on the shared
        // source can never kill in-flight combat sounds.
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.playOnAwake = false;
        audioSource.spatialBlend = 0f;
        playerController = GetComponent<PlayerController>();
    }

    void OnEnable()
    {
        if (playerController != null)
        {
            playerController.OnAttackPerformed += PlayAttackSFX;
            playerController.OnHitReceived += PlayHurtSFX;
        }
    }

    void OnDisable()
    {
        if (playerController != null)
        {
            playerController.OnAttackPerformed -= PlayAttackSFX;
            playerController.OnHitReceived -= PlayHurtSFX;
        }
    }

    void PlayAttackSFX()
    {
        if (weaponSwingClip != null)
            audioSource.PlayOneShot(weaponSwingClip);
    }

    void PlayHurtSFX()
    {
        if (playerHurtClip != null)
            audioSource.PlayOneShot(playerHurtClip);
    }
}
