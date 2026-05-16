using UnityEngine;

public class LichFireProjectile : ProjectileBase
{
    private float animationEndTime;
    private bool shouldDestroyOnAnimationEnd;

    protected override void Start()
    {
        base.Start();
        // Get throw animation duration from animator
        if (animator != null)
        {
            AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);
            animationEndTime = Time.time + stateInfo.length;
            shouldDestroyOnAnimationEnd = true;
        }
    }

    protected override void FixedUpdate()
    {
        base.FixedUpdate();

        // Destroy when animation ends
        if (shouldDestroyOnAnimationEnd && Time.time >= animationEndTime)
        {
            DestroyProjectile();
        }
    }

    protected override void HandleCollision(Collider2D collision)
    {
        // Check if hit player
        if (collision.CompareTag("Player"))
        {
            DamagePlayer();
            DestroyProjectile();
        }
        // Check if hit enemy or wall
        else if (collision.CompareTag("Enemy") || !collision.CompareTag("Player"))
        {
            // Hit something else, destroy
            DestroyProjectile();
        }
    }
}
