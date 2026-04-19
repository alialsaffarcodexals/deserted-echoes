using UnityEngine;

public class PlayerCollisionHandler : MonoBehaviour
{
    void OnCollisionEnter2D(Collision2D collision)
    {
        Debug.Log("Collided with: " + collision.collider.name);
        // handle solid collisions here
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        Debug.Log("Trigger with: " + other.name);
    }
}
