// ------------------------------------------------------------
// PlayerRespawn.cs
// Deserted Echoes | IT8101 Games Development | Group 3
// Author: Khizar Azhar
// Sprint: 7 | Created: April 9, 2026
// Description: Handles player death, life reduction, and scene respawn
// ------------------------------------------------------------

using UnityEngine;

public class PlayerRespawn : MonoBehaviour
{
    [Header("Respawn Settings")]
    [SerializeField] private string enemyTag = "Enemy";
    [SerializeField] private string hazardTag = "Hazard";

    private bool hasDied = false;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (hasDied) return;

        if (other.CompareTag(enemyTag) || other.CompareTag(hazardTag))
        {
            HandleDeath();
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (hasDied) return;

        if (collision.gameObject.CompareTag(enemyTag) || collision.gameObject.CompareTag(hazardTag))
        {
            HandleDeath();
        }
    }

    private void HandleDeath()
    {
        hasDied = true;

        if (GameManager.Instance == null)
        {
            Debug.LogWarning("PlayerRespawn: GameManager instance was not found.");
            SceneLoader.ReloadCurrentScene();
            return;
        }

        GameManager.Instance.playerLives--;

        Debug.Log($"PlayerRespawn: Player died. Lives remaining: {GameManager.Instance.playerLives}");

        if (GameManager.Instance.playerLives <= 0)
        {
            GameManager.Instance.GameOver();
        }
        else
        {
            SceneLoader.ReloadCurrentScene();
        }
    }
}