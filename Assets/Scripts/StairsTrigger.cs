using UnityEngine;
using Unity.Cinemachine;

public class StairsTrigger : MonoBehaviour
{
    [Header("Teleport Destination")]
    public Transform destination;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            // Find the Cinemachine camera and disable damping
            CinemachinePositionComposer composer = FindObjectOfType<CinemachinePositionComposer>();

            Vector3 originalDamping = Vector3.zero;
            if (composer != null)
            {
                originalDamping = composer.Damping;
                composer.Damping = Vector3.zero;
            }

            // Stop velocity and teleport
            Rigidbody2D rb = other.GetComponent<Rigidbody2D>();
            if (rb != null) rb.linearVelocity = Vector2.zero;
            other.transform.position = destination.position;

            // Restore damping next frame
            if (composer != null)
                StartCoroutine(RestoreDamping(composer, originalDamping));
        }
    }

    private System.Collections.IEnumerator RestoreDamping(CinemachinePositionComposer composer, Vector3 damping)
    {
        yield return new WaitForEndOfFrame();
        composer.Damping = damping;
    }
}