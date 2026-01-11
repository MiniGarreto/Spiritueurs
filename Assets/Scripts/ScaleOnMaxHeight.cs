using UnityEngine;

public class ScaleOnMaxHeight : MonoBehaviour
{
    [Header("Source de niveau d'eau")]
    [SerializeField] private BathtubFiller bathtubFiller;

    [Header("Objet à scaler")]
    [SerializeField] private Transform targetObject;

    [Header("Paramètres de scaling")]
    [SerializeField] private float yScaleSpeed = 0.5f; // vitesse d'augmentation du scale en Y
    [SerializeField] private float maxYScale = 2f;     // scale Y maximum à atteindre
    [SerializeField] private bool triggerOnce = true;  // si vrai, arrête le script une fois le max atteint

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
        if (bathtubFiller == null || targetObject == null)
        {
            return;
        }

        bool atMax = bathtubFiller.GetFillPercentage() >= 100f;

        if (atMax)
        {
            started = true;
        }

        if (started)
        {
            Vector3 scale = targetObject.localScale;
            scale.y += yScaleSpeed * Time.deltaTime;
            scale.y = Mathf.Min(scale.y, maxYScale);
            targetObject.localScale = scale;

            if (triggerOnce && Mathf.Approximately(scale.y, maxYScale))
            {
                // Désactive le script une fois le max atteint si demandé
                enabled = false;
            }
        }
    }
}
