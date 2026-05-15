// ─────────────────────────────────────────────────────────────
// SandZone.cs
// Deserted Echoes | IT8101 Games Development | Group 3
// Author: Ali Husain Ali Alsaffar
// Sprint: 4 | Created: May 14, 2026
// Description: Attach to the Floor tilemap (or any child Empty
//              GameObject that covers the sand area) in level-01,
//              level-02, and Open-World. Requires a Collider2D
//              with Is Trigger = true on the same GameObject.
//              Tells FootstepSounds on the Player to switch to
//              the sand footstep clip on enter, and revert on exit.
// ─────────────────────────────────────────────────────────────

using UnityEngine;

public class SandZone : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        FootstepSounds footsteps = other.GetComponentInParent<FootstepSounds>();
        if (footsteps != null)
            footsteps.SetSurface(FootstepSounds.SurfaceType.Sand);
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        FootstepSounds footsteps = other.GetComponentInParent<FootstepSounds>();
        if (footsteps != null)
            footsteps.SetSurface(FootstepSounds.SurfaceType.Wood);
    }
}
