using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class PlayerCombatSFX : MonoBehaviour
{
    [SerializeField] AudioClip weaponSwingClip;

    AudioSource audioSource;
    PlayerController playerController;

    void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        playerController = GetComponent<PlayerController>();
    }

    void OnEnable()
    {
        if (playerController != null)
            playerController.OnAttackPerformed += PlayAttackSFX;
    }

    void OnDisable()
    {
        if (playerController != null)
            playerController.OnAttackPerformed -= PlayAttackSFX;
    }

    void PlayAttackSFX()
    {
        if (weaponSwingClip != null)
            audioSource.PlayOneShot(weaponSwingClip);
    }
}
