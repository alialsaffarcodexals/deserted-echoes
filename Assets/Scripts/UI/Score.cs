using UnityEngine;

public class Score : MonoBehaviour
{
    public int scoreValue = 100;
    
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            //This will give score to the player
            PlayerStats stats = Object.FindAnyObjectByType<PlayerStats>();

            if (stats != null)
            {
                stats.AddScore(scoreValue);
            }

            // Destroy the score object after collecting
            Destroy(gameObject);
        }
    }
  
}
