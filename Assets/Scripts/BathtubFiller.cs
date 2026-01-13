using UnityEngine;

public class BathtubFiller : MonoBehaviour
{
    [SerializeField] private BathroomTap tap;
    [SerializeField] private Transform waterLevel;
    [SerializeField] private float fillSpeed = 0.1f;
    [SerializeField] private float drainSpeed = 0.05f;  
    [SerializeField] private float minHeight = 1f;
    [SerializeField] private float maxHeight = 1f;
    [SerializeField] private bool enableWaterScaling = true;
    [SerializeField] private float minScale = 0.5f; 
    [SerializeField] private float maxScale = 1f;  
    private float currentHeight;
    

    void Start()
    {
        currentHeight = minHeight;
    }


    void Update()
    {
        if (tap != null && tap.IsTapOpen)
        {
            if (currentHeight < maxHeight)
            {
                currentHeight += fillSpeed * Time.deltaTime;
            }
        }
        else
        {

            if (currentHeight > minHeight)
            {
                currentHeight -= drainSpeed * Time.deltaTime;
            }
        }

        currentHeight = Mathf.Clamp(currentHeight, minHeight, maxHeight);

        if (waterLevel != null)
        {
            Vector3 newPos = waterLevel.localPosition;
            newPos.y = currentHeight;
            waterLevel.localPosition = newPos;

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

    public void SetFillSpeed(float newSpeed)
    {
        fillSpeed = newSpeed;
    }

    public void SetDrainSpeed(float newSpeed)
    {
        drainSpeed = newSpeed;
    }

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