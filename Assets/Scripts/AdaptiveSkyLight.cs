using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AdaptiveSkyLight : MonoBehaviour
{
    public float intensity;
    private float adaptionTime = 1f;
    private Collider col;
    public bool isInside;
    public int priority;
    private AdaptiveSkyLight[] allAdaptiveSkyLights;

    private void Start()
    {
        allAdaptiveSkyLights = FindObjectsOfType<AdaptiveSkyLight>();
        col = GetComponent<Collider>();
        isInside = false;
    }

    private void Update()
    {
        bool Triggered = false;
        if (col.bounds.Contains(Camera.main.transform.position) && !isInside)
        {
            Triggered = true;
            isInside = true;
        } else if (isInside)
        {
            Triggered = true;
            isInside = false;
        }

        if (Triggered)
        {
            float highestPriority = priority;
            AdaptiveSkyLight currentSky = this;
            foreach(AdaptiveSkyLight obj in allAdaptiveSkyLights)
            {
                if(obj.isInside && obj.priority > highestPriority)
                {
                    highestPriority = obj.priority;
                    currentSky = obj;
                }
            }
            StartCoroutine(ChangeIntensity(currentSky.intensity));
        }
    }

    IEnumerator ChangeIntensity(float currentIntensity)
    {
        float timer = 0f;
        float startIntensity = RenderSettings.ambientIntensity;
        while(timer <= adaptionTime)
        {
            timer += Time.deltaTime;
            RenderSettings.ambientIntensity = Mathf.Clamp(startIntensity + (currentIntensity - startIntensity) * timer / adaptionTime, startIntensity < currentIntensity? startIntensity : currentIntensity, startIntensity < currentIntensity ? currentIntensity : startIntensity);
            yield return new WaitForEndOfFrame();
        }
    }
}
