using UnityEngine;

public class FootstepSounds : MonoBehaviour
{
    // the two footstep sounds
    public AudioClip woodSound;
    public AudioClip carpetSound;

    // volume for each sound
    public float woodVolume = 1f;
    public float carpetVolume = 1f;

    // how fast footsteps play
    public float footstepInterval = 0.4f;

    private AudioSource audioSource;
    private float footstepTimer = 0f;
    private bool isOnCarpet = false;
    private Vector3 lastPosition;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        lastPosition = transform.position;
        footstepTimer = footstepInterval;
    }

    void Update()
    {
        // did the player move this frame
        bool isMoving = Vector3.Distance(transform.position, lastPosition) > 0.001f;
        lastPosition = transform.position;

        if (isMoving)
        {
            footstepTimer += Time.deltaTime;

            // time to play a sound
            if (footstepTimer >= footstepInterval)
            {
                audioSource.Stop();
                audioSource.PlayOneShot(isOnCarpet ? carpetSound : woodSound,
                                        isOnCarpet ? carpetVolume : woodVolume);
                footstepTimer = 0f;
            }
        }
        else
        {
            // player stopped
            audioSource.Stop();
            footstepTimer = footstepInterval;
        }
    }

    // called by the carpet zone scripts
    public void SetOnCarpet(bool onCarpet)
    {
        isOnCarpet = onCarpet;
        audioSource.Stop();
        footstepTimer = footstepInterval;
    }
}