using UnityEngine;
using UnityEngine.Rendering.Universal;


public class CampfireFlicker : MonoBehaviour
{
    public Light2D campfireLight;

    public float maxIntensity = 2f;

    public float minIntensity = 0.8f;

    public float flickerSpeed = 8f;

    void Update()
    {
        if (campfireLight == null) return;

        float noise = Mathf.PerlinNoise(Time.time * flickerSpeed, 0f);

        campfireLight.intensity = Mathf.Lerp(minIntensity, maxIntensity, noise);
    }
}