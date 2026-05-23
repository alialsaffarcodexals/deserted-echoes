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
        Debug.Log("ImpController.ThrowFireball() called!");
        
        if (impFireProjectilePrefab == null)
        {
            Debug.LogError("ImpController: impFireProjectilePrefab is NOT ASSIGNED in Inspector!");
            return;
        }

        Debug.Log("Spawning Imp fireball at position: " + throwPoint.position);

        // Spawn projectile
        GameObject projectileObj = Instantiate(impFireProjectilePrefab, throwPoint.position, Quaternion.identity);
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
                projAnimator.SetBool("IsMoving", true);
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
