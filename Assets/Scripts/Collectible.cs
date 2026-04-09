// ------------------------------------------------------------
// Collectible.cs
// Deserted Echoes | IT8101 Games Development | Group 3
// Author: Khizar Azhar
// Sprint: [1] | Created: 4/9/2026
// Description: Handles collectible pickup, adds score, and destroys object
// ------------------------------------------------------------

using UnityEngine;

public class Collectible : MonoBehaviour, ICollectible
{
    [SerializeField] private int scoreValue = 10;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        Collect(other.gameObject);
    }

    public void Collect(GameObject collector)
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.AddScore(scoreValue);
        }

        Destroy(gameObject);
    }
}
