using UnityEngine;
using UnityEngine.UI;

public class SandstormSystem : MonoBehaviour
{
    [Header("Drag these in from the Hierarchy")]
    public ParticleSystem sandstorm;
    public Image sandstormOverlay;

    [Header("Sandstorm Settings")]
    public float minTimeBetweenStorms = 30f;
    public float maxTimeBetweenStorms = 90f;
    public float stormDuration = 15f;
    public float fadeSpeed = 1f;
    public float maxOverlayAlpha = 0.5f;

    // Keep track of what the sandstorm is doing
    bool stormIsActive = false;
    bool fadingIn = false;
    bool fadingOut = false;
    float stormTimer = 0f;
    float timUntilNextStorm = 0f;

    void Start()
    {
        // Stop particles at the start
        sandstorm.Stop();

        // Pick a random time for the first storm
        timUntilNextStorm = Random.Range(minTimeBetweenStorms, maxTimeBetweenStorms);
    }

    void Update()
    {
        // Only trigger storms during daytime
        bool isDay = DayNightCycle.Instance != null && DayNightCycle.Instance.IsDay;

        if (!stormIsActive && isDay)
        {
            // Count down to the next storm
            timUntilNextStorm -= Time.deltaTime;

            if (timUntilNextStorm <= 0f)
                StartStorm();
        }

        // Handle fading the overlay in
        if (fadingIn)
        {
            float currentAlpha = sandstormOverlay.color.a;
            currentAlpha += Time.deltaTime * fadeSpeed;

            if (currentAlpha >= maxOverlayAlpha)
            {
                currentAlpha = maxOverlayAlpha;
                fadingIn = false;
            }

            SetOverlayAlpha(currentAlpha);
        }

        // Handle fading the overlay out
        if (fadingOut)
        {
            float currentAlpha = sandstormOverlay.color.a;
            currentAlpha -= Time.deltaTime * fadeSpeed;

            if (currentAlpha <= 0f)
            {
                currentAlpha = 0f;
                fadingOut = false;
                stormIsActive = false;

                // Pick time for next storm
                timUntilNextStorm = Random.Range(minTimeBetweenStorms, maxTimeBetweenStorms);
            }

            SetOverlayAlpha(currentAlpha);
        }

        // Count down the storm duration
        if (stormIsActive && !fadingIn && !fadingOut)
        {
            stormTimer -= Time.deltaTime;

            if (stormTimer <= 0f)
                StopStorm();
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

    // Helper to set the overlay transparency
    void SetOverlayAlpha(float alpha)
    {
        Color c = sandstormOverlay.color;
        c.a = alpha;
        sandstormOverlay.color = c;
    }

    // Call this from a UI button to test the storm
    public void TestStorm()
    {
        if (!stormIsActive)
            StartStorm();
        else
            StopStorm();
    }
}