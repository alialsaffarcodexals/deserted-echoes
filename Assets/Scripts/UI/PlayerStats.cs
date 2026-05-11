using UnityEngine;
using TMPro;

public class PlayerStats : MonoBehaviour
{
    [Header("Lives Settings")]
    public int lives = 3;
    public TextMeshProUGUI livesText;

    [Header("Score Sittings")]
    public int score = 0;
    public TextMeshProUGUI scoreText;

    void Start()
    {
        UpdateLivesUI();
        UpdateScoreUI();
    }

    public void TakeDamage(int amount)
    {
        Debug.Log("Player takes damage: " + amount);
        LoseLife();
    }

    public void LoseLife()
    {
        if (lives > 0)
        {
            lives--;
            UpdateLivesUI();
        }

        if (lives <= 0)
        {
            // Handle player death (e.g., show game over screen)
            Debug.Log("Game Over!");
        }
    }

    void UpdateLivesUI()
    {
        livesText.text = "Lives: " + lives.ToString();
    }

    public void AddScore(int points)
    {
        score += points;
        UpdateScoreUI();
    }

    void UpdateScoreUI()
    {
        // D6 format for score (e.g., 000001, 000010, etc.)
        scoreText.text = "Score: " + score.ToString("D6");
    }
}
