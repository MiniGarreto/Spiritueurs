using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class AutoDespawnWhenNotHeld : MonoBehaviour
{
    [Header("Temps avant disparition quand non tenu")]
    public float despawnDelay = 3f;

    private UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable grabInteractable;
    private float timer;
    private bool isHeld;

    void Awake()
    {
        grabInteractable = GetComponent<UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable>();

        grabInteractable.selectEntered.AddListener(OnGrab);
        grabInteractable.selectExited.AddListener(OnRelease);
    }

    void OnGrab(SelectEnterEventArgs args)
    {
        isHeld = true;
        timer = 0f; // reset timer
    }

    void OnRelease(SelectExitEventArgs args)
    {
        isHeld = false;
        timer = 0f; // restart timer
    }

    void Update()
    {
        if (isHeld)
            return;

        timer += Time.deltaTime;

        if (timer >= despawnDelay)
        {
            Destroy(gameObject);
        }
    }
}
