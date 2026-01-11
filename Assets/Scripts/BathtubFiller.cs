using UnityEngine;

public class BathtubFiller : MonoBehaviour
{
    [SerializeField] private BathroomTap tap;
    [SerializeField] private Transform waterLevel;
    [SerializeField] private float fillSpeed = 0.1f;
    [SerializeField] private float maxHeight = 1f;
    [SerializeField] private float minScale = 0.5f; // Scale au fond de la baignoire
    [SerializeField] private float maxScale = 1f;   // Scale en haut de la baignoire

    private float currentHeight = 0f;

    void Update()
    {
        if (tap != null && tap.IsTapOpen)
        {
            // Remplir la baignoire si le robinet est ouvert
            if (currentHeight < maxHeight)
            {
                currentHeight += fillSpeed * Time.deltaTime;
                currentHeight = Mathf.Clamp(currentHeight, 0f, maxHeight);

                // Mettre à jour la position et la scale du niveau d'eau
                if (waterLevel != null)
                {
                    // Position Y
                    Vector3 newPos = waterLevel.localPosition;
                    newPos.y = currentHeight;
                    waterLevel.localPosition = newPos;

                    // Scale adaptée à la hauteur (interpolation linéaire)
                    float normalizedHeight = currentHeight / maxHeight;
                    float currentScale = Mathf.Lerp(minScale, maxScale, normalizedHeight);
                    Vector3 newScale = waterLevel.localScale;
                    newScale.y = currentScale;
                    newScale.z = currentScale;
                    waterLevel.localScale = newScale;
                }
            }
        }
    }

    public float GetFillPercentage()
    {
        return (currentHeight / maxHeight) * 100f;
    }

    public void EmptyBathtub()
    {
        currentHeight = 0f;
        if (waterLevel != null)
        {
            Vector3 newPos = waterLevel.localPosition;
            newPos.y = 0.15f;
            waterLevel.localPosition = newPos;

            // Remettre la scale au minimum
            Vector3 newScale = waterLevel.localScale;
            newScale.x = minScale;
            newScale.z = minScale;
            waterLevel.localScale = newScale;
        }
    }
}
