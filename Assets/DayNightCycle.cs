using UnityEngine;
using UnityEngine.Rendering.Universal;

public class DayNightCycle : MonoBehaviour
{
    // How long a full day lasts in real seconds
    public float dayDuration = 120f;

    // Current time of day (0 = midnight, 0.5 = noon, 1 = midnight again)
    [Range(0f, 1f)]
    public float timeOfDay = 0.5f;
    public bool IsDay => timeOfDay > 0.25f && timeOfDay < 0.75f;

    // The global light in the scene
    public Light2D globalLight;

    // Controls what colour the light is at each time of day
    public Gradient lightColour;

    // Controls how bright the light is at each time of day
    public AnimationCurve lightIntensity;

    public static DayNightCycle Instance { get; private set; }

    void Awake()
    {
        Instance = this;
    }

    void Update()
    {
        // Move time forward
        timeOfDay += Time.deltaTime / dayDuration;

        // Reset back to 0 when a full day is done
        if (timeOfDay >= 1f)
            timeOfDay = 0f;

        // Apply the colour and brightness to the light
        if (globalLight != null)
        {
            globalLight.color = lightColour.Evaluate(timeOfDay);
            globalLight.intensity = lightIntensity.Evaluate(timeOfDay);
        }
    }
}