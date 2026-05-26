using UnityEngine;

public class ImpFireProjectile : ProjectileBase
{
    private bool hasHit;
    private float hitAnimationStartTime;
    private float hitAnimationDuration = 0.5f;

    protected override void Start()
    {
        base.Start();
        hasHit = false;
    }

    protected override void FixedUpdate()
    {
        if (!hasHit)
        {
            base.FixedUpdate();
            // Loop animation plays while moving
            if (animator != null)
                animator.SetBool("IsMoving", true);
        }
        else
        {
            // Stop movement after hit
            if (rb != null)
                rb.linearVelocity = Vector2.zero;

            // Destroy after hit animation completes
            if (Time.time >= hitAnimationStartTime + hitAnimationDuration)
                DestroyProjectile();
        }
    }

    protected override void HandleCollision(Collider2D collision)
    {
        if (hasHit)
            return;

        if (!collision.CompareTag("Player"))
            return;

        hasHit = true;

        // Stop moving
        if (rb != null)
            rb.linearVelocity = Vector2.zero;

        DamagePlayer();

        // Play hit animation
        if (animator != null)
        {
            animator.SetBool("IsMoving", false);
            animator.SetTrigger("Hit");
        }

        hitAnimationStartTime = Time.time;
    }

}
