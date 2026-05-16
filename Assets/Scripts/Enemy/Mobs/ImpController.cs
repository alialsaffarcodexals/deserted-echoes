using UnityEngine;

public class ImpController : EnemyControllerBase
{
    [SerializeField] private GameObject impFireProjectilePrefab;
    [SerializeField] private Transform throwPoint;
    [SerializeField] private float projectileSpeed = 6f;

    protected override void Start()
    {
        base.Start();
        
        // If throwPoint not assigned, create one at this position
        if (throwPoint == null)
        {
            GameObject throwObj = new GameObject("ThrowPoint");
            throwObj.transform.SetParent(transform);
            throwObj.transform.localPosition = Vector3.right * 0.4f;
            throwPoint = throwObj.transform;
        }
    }

    public void ThrowFireball()
    {
        if (impFireProjectilePrefab == null)
        {
            Debug.LogWarning("ImpController: impFireProjectilePrefab not assigned!");
            return;
        }

        // Get throw direction
        Vector2 throwDirection = GetLastMoveDirection();
        if (throwDirection == Vector2.zero)
            throwDirection = Vector2.right;

        // Spawn projectile
        GameObject projectileObj = Instantiate(impFireProjectilePrefab, throwPoint.position, Quaternion.identity);
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
                projAnimator.SetBool("IsMoving", true);
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
