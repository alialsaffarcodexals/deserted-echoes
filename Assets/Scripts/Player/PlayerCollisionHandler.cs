using UnityEngine;

public class PlayerCollisionHandler : MonoBehaviour
{
    private PlayerStats stats;

    void Start()
    {
        // Cache the reference once at the start to save performance
        stats = Object.FindAnyObjectByType<PlayerStats>();
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        Debug.Log("Collided with: " + collision.collider.name);

<<<<<<< HEAD
        if (collision.gameObject.CompareTag("Enemy") || collision.gameObject.CompareTag("Trap"))
=======
        if (stats != null && (collision.gameObject.CompareTag("Enemy") || collision.gameObject.CompareTag("Trap")))
>>>>>>> develop
        {
            stats.LoseLife();
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Trap"))
        {
            Debug.Log("Stepped into a trigger trap!");
            stats?.LoseLife();
        }
        Debug.Log("Trigger with: " + other.name);
    }
}