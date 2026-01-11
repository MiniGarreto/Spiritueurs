using UnityEngine;

public class BathtubFiller : MonoBehaviour
{
    [Header("Système écoulement d'eau")]
    [SerializeField] private BathroomTap tap;
    [Header("Niveau d'eau")]
    [SerializeField] private Transform waterLevel;
    [SerializeField] private float fillSpeed = 0.1f;
    [SerializeField] private float drainSpeed = 0.05f;  // Vitesse de vidange quand robinet fermé
    [SerializeField] private float minHeight = 1f;
    [SerializeField] private float maxHeight = 1f;
    [SerializeField] private bool enableWaterScaling = true;
    [SerializeField] private float minScale = 0.5f; // Scale au fond de la baignoire
    [SerializeField] private float maxScale = 1f;   // Scale en haut de la baignoire

    private float currentHeight;
    

    void Start()
    {
        currentHeight = minHeight;
    }


    void Update()
    {
        if (tap != null && tap.IsTapOpen)
        {
            // Remplir la baignoire si le robinet est ouvert
            if (currentHeight < maxHeight)
            {
                currentHeight += fillSpeed * Time.deltaTime;
            }
        }
        else
        {
            // Vider la baignoire si le robinet est fermé
            if (currentHeight > minHeight)
            {
                currentHeight -= drainSpeed * Time.deltaTime;
            }
        }

        currentHeight = Mathf.Clamp(currentHeight, minHeight, maxHeight);

        // Mettre à jour la position et la scale du niveau d'eau
        if (waterLevel != null)
        {
            // Position Y
            Vector3 newPos = waterLevel.localPosition;
            newPos.y = currentHeight;
            waterLevel.localPosition = newPos;

            // Scale adaptée à la hauteur (interpolation linéaire) si activé
            if (enableWaterScaling)
            {
                float normalizedHeight = (currentHeight - minHeight) / (maxHeight - minHeight);
                float currentScale = Mathf.Lerp(minScale, maxScale, normalizedHeight);
                Vector3 newScale = waterLevel.localScale;
                newScale.y = currentScale;
                newScale.z = currentScale;
                waterLevel.localScale = newScale;
            }
        }
    }

    public bool heightMaxAtteinte()
    {
        return currentHeight >= maxHeight;
    }

    // Permet de modifier la vitesse de remplissage depuis l'extérieur
    public void SetFillSpeed(float newSpeed)
    {
        fillSpeed = newSpeed;
    }

    // Permet de modifier la vitesse de vidange depuis l'extérieur
    public void SetDrainSpeed(float newSpeed)
    {
        drainSpeed = newSpeed;
    }

    // Permet de lire la vitesse actuelle
    public float GetFillSpeed()
    {
        return fillSpeed;
    }

    public void EmptyBathtub()
    {
        currentHeight = minHeight;
        
        if (waterLevel != null)
        {
            Vector3 newPos = waterLevel.localPosition;
            newPos.y = minHeight;
            waterLevel.localPosition = newPos;

            // Remettre la scale au minimum si le scaling est activé
            if (enableWaterScaling)
            {
                Vector3 newScale = waterLevel.localScale;
                newScale.y = minScale;
                newScale.z = minScale;
                waterLevel.localScale = newScale;
            }
        }
    }
}