using UnityEngine;
using UnityEngine.Rendering.Universal;

public class DayNightCycle : MonoBehaviour
{
    // i need this so other scripts can access this one
    public static DayNightCycle Instance { get; private set; }

    // drag the global light into this in the inspector
    public Light2D globalLight;

    // these control the colour and brightness over time - set them in the inspector
    public Gradient lightColour;
    public AnimationCurve lightIntensity;

    // 10 mins = one full day (600 seconds)
    // 3 mins dark -> 4 mins day -> 3 mins dark
    public float totalDayDuration = 600f;

    // goes from 0 to 1 over the course of a day
    // 0 = midnight, 0.3 = sunrise, 0.7 = sunset, 1 = midnight again
    [Range(0f, 1f)]
    public float timeOfDay = 0f;

    // keeps track of what day it is
    public int currentDay { get; private set; } = 1;

    // true when the sun is up (between 0.3 and 0.7)
    public bool IsDay => timeOfDay > 0.3f && timeOfDay < 0.7f;

    // used to tell other scripts when a new day just started
    bool justStartedNewDay = false;

    void Awake()
    {
        // singleton setup so other scripts can find this one
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    void Update()
    {
        MoveTimeForward();
        UpdateLight();
    }

    void MoveTimeForward()
    {
        // slowly increase time of day based on how long a day is
        timeOfDay += Time.deltaTime / totalDayDuration;

        // once we hit 1 a full day has passed
        if (timeOfDay >= 1f)
        {
            timeOfDay = 0f;
            currentDay++;
            justStartedNewDay = true;
            Debug.Log("Day " + currentDay + " started!");
        }
        else
        {
            justStartedNewDay = false;
        }
    }

    void UpdateLight()
    {
        if (globalLight == null) return;

        // use the gradient and curve from the inspector to update the light
        globalLight.color = lightColour.Evaluate(timeOfDay);
        globalLight.intensity = lightIntensity.Evaluate(timeOfDay);
    }

    // other scripts can call this to check if a new day just started
    public bool JustStartedNewDay() => justStartedNewDay;

    // checks if the current day divides evenly by x
    // e.g IsEveryXDays(2) is true on day 2, 4, 6 etc
    public bool IsEveryXDays(int x) => currentDay % x == 0;
}