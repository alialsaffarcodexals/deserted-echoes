using UnityEngine;

public class CarpetZone : MonoBehaviour
{
    // grab the footstep script from the player
    private FootstepSounds footstepSounds;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            // tell the footstep script we're on carpet
            footstepSounds = other.GetComponent<FootstepSounds>();
            if (footstepSounds != null)
                footstepSounds.SetOnCarpet(true);
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            // tell the footstep script we're back on wood
            if (footstepSounds != null)
                footstepSounds.SetOnCarpet(false);
        }
    }
}