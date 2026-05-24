using UnityEngine;
using TMPro;


public class TemperatureUI : MonoBehaviour
{
    public TextMeshProUGUI tempText;

    // changes the colour of the text based on how hot or cold it is
    Color coldColor = new Color(0.2f, 0.5f, 1f);    // blue when cold
    Color neutralColor = new Color(0.2f, 0.8f, 0.2f);  // green when nutrual
    Color hotColor = new Color(1f, 0.2f, 0.1f);    // red when hot

    void Update()
    {
        if (TemperatureSystem.Instance == null) return;
        if (tempText == null) return;

        float temp = TemperatureSystem.Instance.CurrentTemperature;
        float comfortMin = TemperatureSystem.Instance.comfortMin;
        float comfortMax = TemperatureSystem.Instance.comfortMax;

        // update the text to show the current temperature
        tempText.text = Mathf.RoundToInt(temp) + "°C";

        // change the colour based on how hot or cold it is
        if (temp < comfortMin)
            tempText.color = coldColor;
        else if (temp > comfortMax)
            tempText.color = hotColor;
        else
            tempText.color = neutralColor;
    }
}