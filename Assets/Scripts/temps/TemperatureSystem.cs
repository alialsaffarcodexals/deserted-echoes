using UnityEngine;

public class TemperatureSystem : MonoBehaviour
{
    public static TemperatureSystem Instance { get; private set; }

    // the range of temperatures in the game
    public float minTemp = -10f;  // coldest it can get at night time
    public float maxTemp = 45f;   // hottest it can get during the day time

    // anything between these two values is comfortable
    public float comfortMin = 15f;
    public float comfortMax = 30f;

    // how much hotter the sandstorm makes it
    public float sandstormHeatBonus = 15f;

    // how much each item affects the temperature
    public float torchWarmth = 8f;
    public float campfireWarmth = 15f;
    public float waterCoolAmount = 10f;

    // how much faster stats drain when too cold or too hot
    public float coldHungerMultiplier = 2f;
    public float coldStaminaMultiplier = 1.5f;
    public float hotThirstMultiplier = 2.5f;
    public float hotStaminaMultiplier = 1.5f;

    // below or above these the player starts losing health
    public float criticalColdTemp = 0f;
    public float criticalHotTemp = 40f;
    public float extremeDamageRate = 2f;

    // the current temperature the player feels
    public float CurrentTemperature { get; private set; }

    // keeping track of everything that affects temperature
    bool inNeutralZone = false;
    bool torchOn = false;
    bool nearCampfire = false;
    float waterTimeLeft = 0f;

    // these get read by the survival system to adjust stat drain rates
    float hungerModifier = 1f;
    float thirstModifier = 1f;
    float staminaModifier = 1f;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    void Update()
    {
        CalculateTemperature();
        ApplyEffects();

        // count down how long the water cooling lasts
        if (waterTimeLeft > 0f)
            waterTimeLeft -= Time.deltaTime;
    }

    void CalculateTemperature()
    {
        // oasis zone keeps you at a comfortable temperature no matter what
        if (inNeutralZone)
        {
            float neutralTarget = (comfortMin + comfortMax) / 2f;
            CurrentTemperature = Mathf.Lerp(CurrentTemperature, neutralTarget, Time.deltaTime * 0.5f);
            return;
        }

        if (DayNightCycle.Instance == null) return;

        float t = DayNightCycle.Instance.timeOfDay;

        // temperature peaks at noon and drops at night using a sine curve
        float normalised = Mathf.Sin(Mathf.Clamp01((t - 0.3f) / 0.4f) * Mathf.PI);
        float baseTemp = Mathf.Lerp(minTemp, maxTemp, normalised);

        // sandstorm adds extra heat instantly since its a weather event
        if (SandstormSystem.Instance != null && SandstormSystem.Instance.StormIsActive)
            baseTemp += sandstormHeatBonus;

        // torch campfire and water are targets we lerp towards gradually
        float targetTemp = baseTemp;
        if (torchOn) targetTemp += torchWarmth;
        if (nearCampfire) targetTemp += campfireWarmth;
        if (waterTimeLeft > 0f) targetTemp -= waterCoolAmount;

        // gradually move current temperature towards the target
        CurrentTemperature = Mathf.Lerp(CurrentTemperature, targetTemp, Time.deltaTime * 0.3f);
    }

    void ApplyEffects()
    {
        if (SurvivalSystem.Instance == null) return;

        // reset to normal
        hungerModifier = 1f;
        thirstModifier = 1f;
        staminaModifier = 1f;

        if (CurrentTemperature < comfortMin)
        {
            // too cold - hunger and stamina drain faster
            hungerModifier = coldHungerMultiplier;
            staminaModifier = coldStaminaMultiplier;

            // critically cold - start losing health
            if (CurrentTemperature <= criticalColdTemp)
                SurvivalSystem.Instance.TakeDamage(extremeDamageRate * Time.deltaTime);
        }
        else if (CurrentTemperature > comfortMax)
        {
            // too hot - thirst and stamina drain faster
            thirstModifier = hotThirstMultiplier;
            staminaModifier = hotStaminaMultiplier;

            // critically hot - start losing health
            if (CurrentTemperature >= criticalHotTemp)
                SurvivalSystem.Instance.TakeDamage(extremeDamageRate * Time.deltaTime);
        }
    }

    // these get called by the survival system to modify stat drain rates
    public float GetHungerModifier() => hungerModifier;
    public float GetThirstModifier() => thirstModifier;
    public float GetStaminaModifier() => staminaModifier;

    // called by the oasis trigger zone
    public void EnterNeutralZone() => inNeutralZone = true;
    public void ExitNeutralZone() => inNeutralZone = false;

    // called by the torch script when toggled
    public void SetTorchState(bool state) => torchOn = state;

    // called by the campfire trigger zone
    public void EnterCampfire() => nearCampfire = true;
    public void ExitCampfire() => nearCampfire = false;

    // called when the player drinks water - cools them for 3 in game minutes
    public void DrinkWater() => waterTimeLeft = 180f;
}