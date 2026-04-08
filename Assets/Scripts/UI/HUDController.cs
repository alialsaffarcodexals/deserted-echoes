// ─────────────────────────────────────────────────────────────
// HUDController.cs
// Deserted Echoes | IT8101 Games Development | Group 3
// Author: Prepared by Ali Husain — Implementation: Faisal Alasfoor
// Sprint: 1 | Created: April 9, 2026
// Description: Controls the in-game HUD canvas (health bar,
//              stamina bar, score, lives). Attach to HUDCanvas
//              in the test-level scene. Assign references in Inspector.
// ─────────────────────────────────────────────────────────────

using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class HUDController : MonoBehaviour
{
    [Header("Health & Stamina")]
    [SerializeField] private Slider healthBar;
    [SerializeField] private Slider staminaBar;

    [Header("Score & Lives")]
    [SerializeField] private TextMeshProUGUI scoreText;
    [SerializeField] private TextMeshProUGUI livesText;

    private void Update()
    {
        // Pull live data from GameManager each frame
        if (GameManager.Instance != null)
        {
            UpdateScore(GameManager.Instance.playerScore);
            UpdateLives(GameManager.Instance.playerLives);
        }
    }

    // ── Public Methods (call these from PlayerStats) ─────────

    /// <summary>Update health bar. value range: 0f–1f (normalized).</summary>
    public void UpdateHealth(float normalizedValue)
    {
        if (healthBar != null)
            healthBar.value = Mathf.Clamp01(normalizedValue);
    }

    /// <summary>Update stamina bar. value range: 0f–1f (normalized).</summary>
    public void UpdateStamina(float normalizedValue)
    {
        if (staminaBar != null)
            staminaBar.value = Mathf.Clamp01(normalizedValue);
    }

    /// <summary>Update score display.</summary>
    public void UpdateScore(int score)
    {
        if (scoreText != null)
            scoreText.text = score.ToString();
    }

    /// <summary>Update lives display.</summary>
    public void UpdateLives(int lives)
    {
        if (livesText != null)
            livesText.text = $"x{lives}";
    }
}
