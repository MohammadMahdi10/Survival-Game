using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DayNightManager : MonoBehaviour
{
    public float dayDuration = 120f; // Duration of a full day in seconds

    public float currentTimeOfDay = 12f;

    public Material daySkybox;
    public Material nightSkybox;


    public Light directionalLight;
    public float maxLightIntensity = 1.2f;
    public float minLightIntensity = 0.2f;

    private bool isNight = false;
    void Start()
    {
        
    }

    void Update()
    {
        currentTimeOfDay += (24f / dayDuration) * Time.deltaTime;

        if (currentTimeOfDay >= 24f)
        {
            currentTimeOfDay = 0f;
        }

        float sunRotation = (currentTimeOfDay / 24f) * 360f - 90f;
        if(directionalLight != null)
        {
            directionalLight.transform.localRotation = Quaternion.Euler(sunRotation, 170f, 0f);
            directionalLight.intensity = Mathf.Lerp(minLightIntensity, maxLightIntensity, Mathf.Clamp01((currentTimeOfDay - 6f) / 12f));
        }

        checkDayNightTransition();

        UpdateLightIntensity();
    }

    private void checkDayNightTransition()
    {
        if (currentTimeOfDay >=6f && currentTimeOfDay < 18f)
        {
            if (isNight || RenderSettings.skybox != daySkybox)
            {
                isNight = false;
                if (daySkybox != null)
                {
                    RenderSettings.skybox = daySkybox;
                    DynamicGI.UpdateEnvironment();  
                }
            }
        }
        else
        {
            if (!isNight || RenderSettings.skybox != nightSkybox)
            {
                isNight = true;
                if (nightSkybox != null)
                {
                    RenderSettings.skybox = nightSkybox;
                    DynamicGI.UpdateEnvironment();      
                }
            }
        }
    }

    void UpdateLightIntensity() {
        if (directionalLight == null) return;
        
        if (currentTimeOfDay >= 6f && currentTimeOfDay <= 18f)
        {
            directionalLight.intensity = Mathf.Lerp(minLightIntensity, maxLightIntensity, Mathf.Sin((currentTimeOfDay - 6f) / 12f * Mathf.PI));
        }
        else
        {
            directionalLight.intensity = Mathf.Lerp(maxLightIntensity, minLightIntensity, Mathf.Sin((currentTimeOfDay - 18f) / 12f * Mathf.PI));
        }
    }

    public string GetFormattedTime()
    {
        int hours = Mathf.FloorToInt(currentTimeOfDay);
        int minutes = Mathf.FloorToInt((currentTimeOfDay - hours) * 60f);
        return string.Format("{0:00}:{1:00}", hours, minutes);
    }
}