using UnityEngine;

public class TemperatureSystem : MonoBehaviour
{
    public static TemperatureSystem Instance { get; private set; }

    // coldest and hottest the world can get
    public float minTemp = -10f;
    public float maxTemp = 45f;

    // the sweet spot where the player isnt affected by anything
    public float comfortMin = 15f;
    public float comfortMax = 30f;

    // storms make it way hotter
    public float sandstormHeatBonus = 15f;

    // how warm each item makes you feel
    public float torchWarmth = 8f;
    public float campfireWarmth = 30f;
    public float waterCoolAmount = 10f;

    // how much faster things drain when youre too cold
    public float coldHungerMultiplier = 2f;
    public float coldStaminaMultiplier = 1.5f;

    // how much faster things drain when youre too hot
    public float hotThirstMultiplier = 2.5f;
    public float hotStaminaMultiplier = 1.5f;

    // if you hit these temps your health starts dropping
    public float criticalColdTemp = 0f;
    public float criticalHotTemp = 40f;
    public float extremeDamageRate = 2f;

    public float CurrentTemperature { get; private set; }

    // stuff that changes the temperature
    bool inNeutralZone = false;
    bool torchOn = false;
    bool nearCampfire = false;
    float waterTimeLeft = 0f;

    // these get read by the survival system
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

        // water cooling doesnt last forever
        if (waterTimeLeft > 0f)
            waterTimeLeft -= Time.deltaTime;
    }

    void CalculateTemperature()
    {
        // oasis keeps you comfortable no matter what time it is
        if (inNeutralZone)
        {
            float neutralTarget = (comfortMin + comfortMax) / 2f;
            CurrentTemperature = Mathf.Lerp(CurrentTemperature, neutralTarget, Time.deltaTime * 0.5f);
            return;
        }

        if (DayNightCycle.Instance == null) return;

        float t = DayNightCycle.Instance.timeOfDay;

        // gets hot at noon and cold at night
        float normalised = Mathf.Sin(Mathf.Clamp01((t - 0.3f) / 0.4f) * Mathf.PI);
        float baseTemp = Mathf.Lerp(minTemp, maxTemp, normalised);

        // sandstorms spike the heat instantly
        if (SandstormSystem.Instance != null && SandstormSystem.Instance.StormIsActive)
            baseTemp += sandstormHeatBonus;

        // items gradually push the temperature towards a better spot
        float targetTemp = baseTemp;
        if (torchOn) targetTemp += torchWarmth;
        if (nearCampfire) targetTemp += campfireWarmth;
        if (waterTimeLeft > 0f) targetTemp -= waterCoolAmount;

        // slowly move towards the target so it doesnt feel instant
        CurrentTemperature = Mathf.Lerp(CurrentTemperature, targetTemp, Time.deltaTime * 0.3f);
    }

    void ApplyEffects()
    {
        if (SurvivalSystem.Instance == null) return;

        // start fresh every frame
        hungerModifier = 1f;
        thirstModifier = 1f;
        staminaModifier = 1f;

        // torch campfire and oasis all keep you safe so skip any penalties
        if (inNeutralZone || torchOn || nearCampfire) return;

        if (CurrentTemperature < comfortMin)
        {
            // freezing - body burns more calories and energy trying to stay warm
            hungerModifier = coldHungerMultiplier;
            staminaModifier = coldStaminaMultiplier;

            // dangerously cold - health starts dropping
            if (CurrentTemperature <= criticalColdTemp)
                SurvivalSystem.Instance.TakeDamage(extremeDamageRate * Time.deltaTime);
        }
        else if (CurrentTemperature > comfortMax)
        {
            // overheating - sweating like crazy and exhausted
            thirstModifier = hotThirstMultiplier;
            staminaModifier = hotStaminaMultiplier;

            // dangerously hot - health starts dropping
            if (CurrentTemperature >= criticalHotTemp)
                SurvivalSystem.Instance.TakeDamage(extremeDamageRate * Time.deltaTime);
        }
    }

    public float GetHungerModifier() => hungerModifier;
    public float GetThirstModifier() => thirstModifier;
    public float GetStaminaModifier() => staminaModifier;

    public void EnterNeutralZone() => inNeutralZone = true;
    public void ExitNeutralZone() => inNeutralZone = false;

    public void SetTorchState(bool state) => torchOn = state;

    public void EnterCampfire() => nearCampfire = true;
    public void ExitCampfire() => nearCampfire = false;

    public void DrinkWater() => waterTimeLeft = 180f;
}