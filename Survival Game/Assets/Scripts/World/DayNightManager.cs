using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System;

/// Handles the day and night cycle.
/// Updates the time, lighting, sky colours, and UI while enabling hardcore mode on the third day.
public class DayNightManager : MonoBehaviour
{
    [Header("Time Settings")]
    public float dayDurationInSeconds = 120f;

    [Range(0f, 24f)]
    public float currentTimeOfDay = 8f;
    public int currentDay = 1;

    [Header("Hardcore Mode")]
    public bool hardcoreMode = false;

    [Header("Sun Light Reference")]
    public Light directionalSun;

    [Header("UI References")]
    public TextMeshProUGUI dayText;
    public TextMeshProUGUI timeText;
    public GameObject hardcoreText;

    [Header("Sky Color Settings (No Assets)")]
    public Color daySkyColor = new Color(0.4f, 0.7f, 1f);
    public Color nightSkyColor = new Color(0.05f, 0.05f, 0.15f);
    public Color dayEquatorColor = new Color(0.7f, 0.8f, 0.9f);
    public Color nightEquatorColor = new Color(0.1f, 0.1f, 0.2f);

    /// Sets the ambient lighting mode when the game starts.
    /// Allows the environment to use separate sky, equator, and ground lighting.
    private void Start()
    {
        RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Trilight;
    }

    /// Advances the time of day every frame.
    /// Increments the day when a full day passes and enables hardcore mode from day three.
    /// Updates the hardcore mode indicator, lighting, sky colours, and time display.
    private void Update()
    {
        currentTimeOfDay += (24f / dayDurationInSeconds) * Time.deltaTime;

        if (currentTimeOfDay >= 24f) {
            currentTimeOfDay -= 24f;
            currentDay++;

            if (currentDay >= 3 && !hardcoreMode) {
                hardcoreMode = true;
                Debug.Log("HARDCORE!!!!");
            }
        }

        if (hardcoreText != null) {
            hardcoreText.SetActive(hardcoreMode);
        }

        UpdateLightingAndSky();
        UpdateUI();
    }

    /// Updates the sun's rotation and intensity based on the time of day.
    /// Blends the ambient lighting and camera background between day and night colours.
    private void UpdateLightingAndSky()
    {
        float timePercent = currentTimeOfDay / 24f;

        if (directionalSun != null)
        {
            directionalSun.transform.rotation = Quaternion.Euler((timePercent * 360f) - 90f, 170f, 0f);

            float sunIntensity = Mathf.Clamp01(Mathf.Sin(timePercent * Mathf.PI));
            directionalSun.intensity = sunIntensity * 1.2f;
        }

        float skyBlend = Mathf.Clamp01(Mathf.Sin(timePercent * Mathf.PI));
        RenderSettings.ambientSkyColor = Color.Lerp(nightSkyColor, daySkyColor, skyBlend);
        RenderSettings.ambientEquatorColor = Color.Lerp(nightEquatorColor, dayEquatorColor, skyBlend);

        if(Camera.main != null)
        {
            Camera.main.clearFlags = CameraClearFlags.SolidColor;
            Camera.main.backgroundColor = Color.Lerp(nightSkyColor, daySkyColor, skyBlend);
        }
    }

    /// Updates the day and time UI elements.
    /// Displays the current day number and formats the time as hours and minutes.
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
