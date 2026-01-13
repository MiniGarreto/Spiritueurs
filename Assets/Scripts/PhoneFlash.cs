using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

public class PhoneFlash : MonoBehaviour
{
    public XRGrabInteractable grabInteractable;
    public InputActionProperty triggerAction;
    public InputActionProperty leftTriggerAction;
    public InputActionProperty rightTriggerAction;
    public Light flashLight;
    public AudioSource audioSource;

    void OnEnable()
    {
        triggerAction.action.Enable();
        leftTriggerAction.action.Enable();
        rightTriggerAction.action.Enable();
    }

    void OnDisable()
    {
        triggerAction.action.Disable();
        leftTriggerAction.action.Disable();
        rightTriggerAction.action.Disable();
    }

    void Start()
    {
        flashLight.enabled = false;
    }

    void Update()
    {
        if (!grabInteractable.isSelected)
        {
            StopEffects();
            return;
        }

        bool isPressed = triggerAction.action.IsPressed();
        flashLight.enabled = isPressed;

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