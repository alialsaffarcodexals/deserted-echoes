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

    private void Start()
    {
        // Push this scene's slider references directly into SurvivalSystem.
        // This is the authoritative wiring point — no tag search needed.
        if (SurvivalSystem.Instance != null)
            SurvivalSystem.Instance.RegisterSliders(healthBar, staminaBar, null, null);
    }

    private void Update()
    {
        // Pull live data from GameManager each frame
        if (GameManager.Instance != null)
        {
            UpdateScore(GameManager.Instance.playerScore);
            UpdateLives(GameManager.Instance.playerLives);
        }

        // Drive the health and stamina bars from SurvivalSystem.
        // UpdateHealth/UpdateStamina were never called automatically,
        // so the bars stayed frozen at their inspector default (full).
        if (SurvivalSystem.Instance != null)
        {
            float maxHp = SurvivalSystem.Instance.MaxHealth;
            if (maxHp > 0f)
                UpdateHealth(SurvivalSystem.Instance.CurrentHealth / maxHp);

            float maxSt = SurvivalSystem.Instance.maxStamina;
            if (maxSt > 0f)
                UpdateStamina(SurvivalSystem.Instance.CurrentStamina / maxSt);
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
            scoreText.text = $"<b>{score:D6}</b>";
    }

    /// <summary>Update lives display.</summary>
    public void UpdateLives(int lives)
    {
        if (livesText != null)
            livesText.text = $"<b>x{lives}</b>";
    }
}
