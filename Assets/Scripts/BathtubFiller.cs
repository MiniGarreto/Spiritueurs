using UnityEngine;

public class BathtubFiller : MonoBehaviour
{
    [SerializeField] private BathroomTap tap;
    [SerializeField] private Transform waterLevel;
    [SerializeField] private float fillSpeed = 0.1f;
    [SerializeField] private float maxHeight = 1f;

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

                // Mettre à jour la position du niveau d'eau
                if (waterLevel != null)
                {
                    Vector3 newPos = waterLevel.localPosition;
                    newPos.y = currentHeight;
                    waterLevel.localPosition = newPos;
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
            newPos.y = 0f;
            waterLevel.localPosition = newPos;
        }
    }
}
