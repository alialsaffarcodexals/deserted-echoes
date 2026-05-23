using UnityEngine;
using UnityEngine.InputSystem;

public class LevelFootsteps : MonoBehaviour
{
    
    public AudioSource footstepSource;
    public AudioClip footstepSound;

    // adjust the pitch of the footsteps when walking/runnign
    public float walkPitch = 1f;
    public float runPitch = 1.5f;

    bool wasMoving = false;

    void Start()
    {
        footstepSource.loop = true;
        footstepSource.clip = footstepSound;
    }

    void Update()
    {
        Keyboard keyboard = Keyboard.current;

        // check if any movement key is held 
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

        // check if running 
        bool isRunning = keyboard != null && keyboard.leftShiftKey.isPressed;

        // speed the footstep sound up when running
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
}