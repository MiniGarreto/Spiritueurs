using UnityEngine;

public class ScaleOnMaxHeight : MonoBehaviour
{
    [Header("Source de niveau d'eau")]
    [SerializeField] private BathtubFiller[] bathtubFillers;

    [Header("Objet à scaler")]
    [SerializeField] private Transform targetObject;

    [Header("Paramètres de scaling")]
    [SerializeField] private float yScaleSpeed = 0.5f; // vitesse d'augmentation du scale en Y
    [SerializeField] private float yDrainSpeed = 0.3f; // vitesse de diminution du scale en Y quand pas de remplissage
    [SerializeField] private float maxYScale = 2f;     // scale Y maximum à atteindre
    [SerializeField] private float minYScale = 0f;     // scale Y minimum (0 = pas d'eau)

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

        // Compter combien de bathtubFillers ont atteint 100%
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
                // L'eau monte - la vitesse est multipliée par le nombre de bathtubs à 100%
                float currentSpeed = yScaleSpeed * countAtMax;
                scale.y += currentSpeed * Time.deltaTime;
                scale.y = Mathf.Min(scale.y, maxYScale);
            }
            else
            {
                // Aucun robinet ne coule - l'eau descend
                scale.y -= yDrainSpeed * Time.deltaTime;
                scale.y = Mathf.Max(scale.y, minYScale);
            }

            targetObject.localScale = scale;
        }
    }
}
