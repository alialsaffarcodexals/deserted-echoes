using UnityEngine;

// script made to create a temperature zone for the oasis (nuteral tempreate zone)
public class TemperatureZone : MonoBehaviour
{
    void OnTriggerEnter2D(Collider2D other)
    {
        // check if its the player walking in
        if (other.CompareTag("Player"))
            TemperatureSystem.Instance?.EnterNeutralZone();
    }

    void OnTriggerExit2D(Collider2D other)
    {
        // check if its the player walking out
        if (other.CompareTag("Player"))
            TemperatureSystem.Instance?.ExitNeutralZone();
    }
}