using UnityEngine;

public class LichController : EnemyControllerBase
{
    [SerializeField] private GameObject lichFireProjectilePrefab;
    [SerializeField] private Transform throwPoint;
    [SerializeField] private float projectileSpeed = 8f;

    protected override void Start()
    {
        base.Start();
        
        // If throwPoint not assigned, create one at this position
        if (throwPoint == null)
        {
            GameObject throwObj = new GameObject("ThrowPoint");
            throwObj.transform.SetParent(transform);
            throwObj.transform.localPosition = Vector3.right * 0.5f;
            throwPoint = throwObj.transform;
        }
    }

    // Override the melee attack to use ranged attack instead
    public void ThrowFireball()
    {
        if (lichFireProjectilePrefab == null)
        {
            Debug.LogWarning("LichController: lichFireProjectilePrefab not assigned!");
            return;
        }

        // Get throw direction
        Vector2 throwDirection = GetLastMoveDirection();
        if (throwDirection == Vector2.zero)
            throwDirection = Vector2.right;

        // Spawn projectile
        GameObject projectileObj = Instantiate(lichFireProjectilePrefab, throwPoint.position, Quaternion.identity);
        ProjectileBase projectile = projectileObj.GetComponent<ProjectileBase>();
        Animator projAnimator = projectileObj.GetComponent<Animator>();

        if (projectile != null)
        {
            projectile.SetDirection(throwDirection);

            // Set animator direction parameters
            if (projAnimator != null)
            {
                projAnimator.SetFloat("DirX", throwDirection.x);
                projAnimator.SetFloat("DirY", throwDirection.y);
            }

            // Set projectile speed
            Rigidbody2D projRb = projectileObj.GetComponent<Rigidbody2D>();
            if (projRb != null)
            {
                projRb.linearVelocity = throwDirection * projectileSpeed;
            }
        }
    }

    private Vector2 GetLastMoveDirection()
    {
        // This is a simplified approach - in a real scenario, 
        // you might want to expose lastMoveDirection from base class
        return Vector2.right;
    }
}
