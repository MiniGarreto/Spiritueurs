using UnityEngine;
using System.Collections;

public class DiscoLight : MonoBehaviour
{
    [Header("Référence")]
    public Light spotLight;  // Le Spot Light à contrôler

    [Header("Effet Disco")]
    public float discoDuration = 10f;       // Durée de l'effet disco
    public float colorChangeSpeed = 0.2f;   // Temps entre chaque changement de couleur
    public Color[] discoColors = new Color[] { Color.red, Color.green, Color.blue };

    [Header("État normal")]
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

        // Sauvegarder les valeurs normales
        if (spotLight != null)
        {
            normalColor = spotLight.color;
            normalIntensity = spotLight.intensity;
        }
    }

    // Appelé par la poubelle quand un papier toilette est jeté
    public void StartDiscoMode()
    {
        // Si déjà en mode disco, relancer le timer
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
            // Changer la couleur
            spotLight.color = discoColors[colorIndex];
            
            // Varier légèrement l'intensité pour plus d'effet
            spotLight.intensity = normalIntensity * Random.Range(0.8f, 1.5f);

            colorIndex = (colorIndex + 1) % discoColors.Length;

            yield return new WaitForSeconds(colorChangeSpeed);
            elapsedTime += colorChangeSpeed;
        }

        // Retour à l'état normal
        spotLight.color = normalColor;
        spotLight.intensity = normalIntensity;
        isDiscoMode = false;
        discoCoroutine = null;
    }

    // Pour ajouter du temps si on jette un autre papier pendant le disco
    public void ExtendDiscoTime(float extraTime)
    {
        if (isDiscoMode)
        {
            discoDuration += extraTime;
        }
    }
}
