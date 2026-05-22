using UnityEngine;

public class EnemyCombatSFX : MonoBehaviour
{
    [SerializeField] AudioClip attackClip;
    [SerializeField] AudioClip hurtClip;

    AudioSource audioSource;
    EnemyControllerBase enemyController;

    void Awake()
    {
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.playOnAwake = false;
        audioSource.spatialBlend = 0f;

        // Route to SFX group in mixer
        if (SettingsManager.Instance != null)
        {
            audioSource.outputAudioMixerGroup = SettingsManager.Instance.SFXGroup;
        }

        enemyController = GetComponent<EnemyControllerBase>();
    }

    void OnEnable()
    {
        if (enemyController != null)
        {
            enemyController.OnAttackPerformed += PlayAttackSFX;
            enemyController.OnHitReceived += PlayHurtSFX;
        }
    }

    void OnDisable()
    {
        if (enemyController != null)
        {
            enemyController.OnAttackPerformed -= PlayAttackSFX;
            enemyController.OnHitReceived -= PlayHurtSFX;
        }
    }

    void PlayAttackSFX()
    {
        if (attackClip != null)
            audioSource.PlayOneShot(attackClip);
    }

    void PlayHurtSFX()
    {
        if (hurtClip != null)
            audioSource.PlayOneShot(hurtClip);
    }
}
