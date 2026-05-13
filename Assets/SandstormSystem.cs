using UnityEngine;
using UnityEngine.UI;

public class SandstormSystem : MonoBehaviour
{
    
    public ParticleSystem sandstorm;
    public Image sandstormOverlay;

    // how long the storm lasts in seconds
    public float stormDuration = 15f;

    // how fast the overlay fades in and out
    public float fadeSpeed = 1f;

    // how dark the overlay gets at its strongest (0 = invisible, 1 = fully opaque) still testing values around 0.1 since the particles themselves are already pretty strong visually
    public float maxOverlayAlpha = 0.1f;

    // daytime starts at 0.3 and 1 in game minute is 0.1 (since a day is 10 mins)
    // so the storm triggers at 0.3 + 0.1 = 0.4
    float stormTriggerTime = 0.4f;

    // keeping track of whats happening
    bool stormIsActive = false;
    bool fadingIn = false;
    bool fadingOut = false;
    float stormTimer = 0f;
    bool stormHappenedToday = false;

    void Start()
    {
        // make sure particles arent playing at the start
        sandstorm.Stop();
    }

    void Update()
    {
        if (DayNightCycle.Instance == null) return;

        float timeOfDay = DayNightCycle.Instance.timeOfDay;
        int currentDay = DayNightCycle.Instance.currentDay;

        // reset so the storm can happen again next day if needed
        if (DayNightCycle.Instance.JustStartedNewDay())
            stormHappenedToday = false;

        // trigger the storm 1 in game minute into day 1
        if (currentDay == 1 && !stormHappenedToday && timeOfDay >= stormTriggerTime)
        {
            stormHappenedToday = true;
            StartStorm();
        }

        // slowly fade the overlay in
        if (fadingIn)
        {
            float a = sandstormOverlay.color.a + Time.deltaTime * fadeSpeed;
            if (a >= maxOverlayAlpha) { a = maxOverlayAlpha; fadingIn = false; }
            SetOverlayAlpha(a);
        }

        // slowly fade the overlay out
        if (fadingOut)
        {
            float a = sandstormOverlay.color.a - Time.deltaTime * fadeSpeed;
            if (a <= 0f) { a = 0f; fadingOut = false; stormIsActive = false; }
            SetOverlayAlpha(a);
        }

        // count down how long the storm has left
        if (stormIsActive && !fadingIn && !fadingOut)
        {
            stormTimer -= Time.deltaTime;
            if (stormTimer <= 0f) StopStorm();
        }
    }

    void StartStorm()
    {
        stormIsActive = true;
        fadingIn = true;
        fadingOut = false;
        stormTimer = stormDuration;
        sandstorm.Play();
    }

    void StopStorm()
    {
        fadingIn = false;
        fadingOut = true;
        sandstorm.Stop();
    }

    // helper to change the overlay transparency
    void SetOverlayAlpha(float alpha)
    {
        Color c = sandstormOverlay.color;
        c.a = alpha;
        sandstormOverlay.color = c;
    }

    // hooked up to the test button in the inspector
    public void TestStorm()
    {
        if (!stormIsActive) StartStorm();
        else StopStorm();
    }
}