using UnityEngine;
using UnityEngine.Rendering.Universal;


public class CampfireZone : MonoBehaviour
{
    public Light2D campfireLight;

    void Start()
    {
        // make sure the light is on when the campfire is placed
        if (campfireLight != null)
            campfireLight.enabled = true;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        // player walked into the campfire warmth zone
        if (other.CompareTag("Player"))
            TemperatureSystem.Instance?.EnterCampfire();
    }

    void OnTriggerExit2D(Collider2D other)
    {
        // player walked out of the campfire warmth zone
        if (other.CompareTag("Player"))
            TemperatureSystem.Instance?.ExitCampfire();
    }
}