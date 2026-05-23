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

    // Called by animation events to throw fireball
    public void ThrowFireball()
    {
        Debug.Log("LichController.ThrowFireball() called!");
        
        if (lichFireProjectilePrefab == null)
        {
            Debug.LogError("LichController: lichFireProjectilePrefab is NOT ASSIGNED in Inspector!");
            return;
        }

        Debug.Log("Spawning Lich fireball at position: " + throwPoint.position);

        // Spawn projectile
        GameObject projectileObj = Instantiate(lichFireProjectilePrefab, throwPoint.position, Quaternion.identity);
        Debug.Log("Projectile instantiated: " + projectileObj.name);
        
        ProjectileBase projectile = projectileObj.GetComponent<ProjectileBase>();
        Animator projAnimator = projectileObj.GetComponent<Animator>();

        if (projectile != null)
        {
            Debug.Log("ProjectileBase component found!");
            // Default to right direction
            Vector2 throwDirection = Vector2.right;
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
                Debug.Log("Projectile Rigidbody found, setting velocity to: " + (throwDirection * projectileSpeed));
                projRb.linearVelocity = throwDirection * projectileSpeed;
            }
            else
            {
                Debug.LogError("Projectile has NO Rigidbody2D!");
            }
        }
        else
        {
            Debug.LogError("Projectile has NO ProjectileBase component!");
        }
    }
}
