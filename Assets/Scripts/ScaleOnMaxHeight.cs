using UnityEngine;

public class ScaleOnMaxHeight : MonoBehaviour
{
    [SerializeField] private BathtubFiller[] bathtubFillers;
    [SerializeField] private Transform targetObject;
    [SerializeField] private float yScaleSpeed = 0.5f; 
    [SerializeField] private float yDrainSpeed = 0.3f;  
    [SerializeField] private float maxYScale = 2f; 
    [SerializeField] private float minYScale = 0f;

    private bool started = false;
    private float initialYScale;

    private void Awake()
    {
        if (targetObject != null)
        {
            initialYScale = targetObject.localScale.y;
        }
    }

    private void Update()
    {
        if (bathtubFillers == null || bathtubFillers.Length == 0 || targetObject == null)
        {
            return;
        }

        int countAtMax = 0;
        foreach (var filler in bathtubFillers)
        {
            if (filler != null && filler.heightMaxAtteinte())
            {
                countAtMax++;
            }
        }

        bool atLeastOneAtMax = countAtMax > 0;

        if (atLeastOneAtMax)
        {
            started = true;
        }

        if (started)
        {
            Vector3 scale = targetObject.localScale;

            if (countAtMax > 0)
            {
                float currentSpeed = yScaleSpeed * countAtMax;
                scale.y += currentSpeed * Time.deltaTime;
                scale.y = Mathf.Min(scale.y, maxYScale);
            }
            else
            {
                scale.y -= yDrainSpeed * Time.deltaTime;
                scale.y = Mathf.Max(scale.y, minYScale);
            }

            targetObject.localScale = scale;
        }
    }
}
