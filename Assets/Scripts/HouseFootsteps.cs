using UnityEngine;
using UnityEngine.InputSystem;

public class HouseFootsteps : MonoBehaviour
{
    public AudioSource footstepSource;
    public AudioClip woodSound;
    public AudioClip carpetSound;

    // speeds up the sound when runnign 
    public float walkPitch = 1f;
    public float runPitch = 1.5f;

    bool wasMoving = false;
    bool onCarpet = false;

    void Start()
    {
        footstepSource.loop = true;
        footstepSource.clip = woodSound; 
    }

    void Update()
    {
        Keyboard keyboard = Keyboard.current;

        // check if any movement key is held (same keys as PlayerController)
        bool isMoving = false;
        if (keyboard != null)
        {
            if (keyboard.wKey.isPressed || keyboard.upArrowKey.isPressed ||
                keyboard.sKey.isPressed || keyboard.downArrowKey.isPressed ||
                keyboard.aKey.isPressed || keyboard.leftArrowKey.isPressed ||
                keyboard.dKey.isPressed || keyboard.rightArrowKey.isPressed)
            {
                isMoving = true;
            }
        }

        // checks if running same as PlayerController
        bool isRunning = keyboard != null && keyboard.leftShiftKey.isPressed;

        // speeds up the footstep sound when running
        if (isRunning == true)
            footstepSource.pitch = runPitch;
        else
            footstepSource.pitch = walkPitch;

        // only play/stop when the state changes, not every frame
        if (isMoving == true && wasMoving == false)
        {
            footstepSource.Play();
        }
        else if (isMoving == false && wasMoving == true)
        {
            footstepSource.Stop();
        }

        wasMoving = isMoving;
    }

    public void SetOnCarpet(bool isOnCarpet)
    {
        onCarpet = isOnCarpet;

        AudioClip newClip;
        if (onCarpet == true)
            newClip = carpetSound;
        else
            newClip = woodSound;

        // if its already that sound dont do anything
        if (footstepSource.clip == newClip)
            return;

        footstepSource.clip = newClip;

        // if player is walking when they switch floors, keep sound going
        if (wasMoving == true)
            footstepSource.Play();
    }
}