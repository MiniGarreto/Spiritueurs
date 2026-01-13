using UnityEngine;
using System.Collections;

public class DiscoLight : MonoBehaviour
{
    public Light spotLight; 
    public float discoDuration = 10f;       
    public float colorChangeSpeed = 0.2f;
    public Color[] discoColors = new Color[] { Color.red, Color.green, Color.blue };
    public Color normalColor = Color.white;
    public float normalIntensity = 1f;

    private bool isDiscoMode = false;
    private Coroutine discoCoroutine;

    void Start()
    {
        if (spotLight == null)
        {
            spotLight = GetComponent<Light>();
        }

        if (spotLight != null)
        {
            normalColor = spotLight.color;
            normalIntensity = spotLight.intensity;
        }
    }

    public void StartDiscoMode()
    {
        if (discoCoroutine != null)
        {
            StopCoroutine(discoCoroutine);
        }

        discoCoroutine = StartCoroutine(DiscoEffect());
    }

    private IEnumerator DiscoEffect()
    {
        isDiscoMode = true;
        int colorIndex = 0;
        float elapsedTime = 0f;

        while (elapsedTime < discoDuration)
        {
            spotLight.color = discoColors[colorIndex];
            spotLight.intensity = normalIntensity * Random.Range(0.8f, 1.5f);
            colorIndex = (colorIndex + 1) % discoColors.Length;
            yield return new WaitForSeconds(colorChangeSpeed);
            elapsedTime += colorChangeSpeed;
        }

        spotLight.color = normalColor;
        spotLight.intensity = normalIntensity;
        isDiscoMode = false;
        discoCoroutine = null;
    }

    public void ExtendDiscoTime(float extraTime)
    {
        if (isDiscoMode)
        {
            discoDuration += extraTime;
        }
    }
}
