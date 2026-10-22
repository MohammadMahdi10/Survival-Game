using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System;

public class DayNightManager : MonoBehaviour
{
    [Header("Time Settings")]
    public float dayDurationInSeconds = 120f; // Duration of a full day in seconds

    [Range(0f, 24f)]
    public float currentTimeOfDay = 8f; // Current time of day in hours (0-24)
    public int currentDay = 1;

    [Header("Sun Light Reference")]
    public Light directionalSun;

    [Header("UI References")]
    public TextMeshProUGUI dayText;
    public TextMeshProUGUI timeText;

    [Header("Sky Color Settings (No Assets)")]
    public Color daySkyColor = new Color(0.4f, 0.7f, 1f);
    public Color nightSkyColor = new Color(0.05f, 0.05f, 0.15f);
    public Color dayEquatorColor = new Color(0.7f, 0.8f, 0.9f);
    public Color nightEquatorColor = new Color(0.1f, 0.1f, 0.2f);

    private void Start()
    {
        RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Trilight;
    }

    private void Update()
    {
        currentTimeOfDay += (24f / dayDurationInSeconds) * Time.deltaTime;// Increment time based on day duration

        if (currentTimeOfDay >= 24f)
        {
            currentTimeOfDay -= 24f;
            currentDay++;
        }

        UpdateLightingAndSky();
        UpdateUI();
    }

    private void UpdateLightingAndSky()
    {

        float timePercent = currentTimeOfDay / 24f;

        if (directionalSun != null)
        {
            directionalSun.transform.rotation = Quaternion.Euler((timePercent * 360f) - 90f, 170f, 0f);

            float sunIntensity = Mathf.Clamp01(Mathf.Sin(timePercent * Mathf.PI));  
            directionalSun.intensity = sunIntensity * 1.2f;
        }

        // Adjust sky and equator colors based on time of day
        float skyBlend = Mathf.Clamp01(Mathf.Sin(timePercent * Mathf.PI));
        RenderSettings.ambientSkyColor = Color.Lerp(nightSkyColor, daySkyColor, skyBlend);
        RenderSettings.ambientEquatorColor = Color.Lerp(nightEquatorColor, dayEquatorColor, skyBlend);

        if(Camera.main != null)
        {
            Camera.main.clearFlags = CameraClearFlags.SolidColor;
            Camera.main.backgroundColor = Color.Lerp(nightSkyColor, daySkyColor, skyBlend);
        }
    }

    private void UpdateUI()
    {
        if (dayText != null)
        {
            dayText.text = "Day " + currentDay;
        }

        if (timeText != null)
        {
            int hours = Mathf.FloorToInt(currentTimeOfDay);
            int minutes = Mathf.FloorToInt((currentTimeOfDay - hours) * 60f);
            timeText.text = string.Format("{0:00}:{1:00}", hours, minutes);
        }
    }
}
