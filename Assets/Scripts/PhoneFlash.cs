using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

public class PhoneFlash : MonoBehaviour
{
    public XRGrabInteractable grabInteractable;
    public InputActionProperty triggerAction;
    public Light flashLight;
    public AudioSource audioSource;

    void OnEnable()
    {
        triggerAction.action.Enable();
    }

    void OnDisable()
    {
        triggerAction.action.Disable();
    }

    void Start()
    {
        flashLight.enabled = false;
    }

    void Update()
    {
        // Si l'objet n'est pas attrapé → tout éteindre
        if (!grabInteractable.isSelected)
        {
            StopEffects();
            return;
        }

        bool isPressed = triggerAction.action.IsPressed();

        // Lumière active tant que la gâchette est pressée
        flashLight.enabled = isPressed;

        // Son synchronisé avec la gâchette
        if (isPressed)
        {
            if (!audioSource.isPlaying)
                audioSource.Play();
        }
        else
        {
            if (audioSource.isPlaying)
                audioSource.Stop();
        }
    }

    void StopEffects()
    {
        flashLight.enabled = false;

        if (audioSource.isPlaying)
            audioSource.Stop();
    }
}