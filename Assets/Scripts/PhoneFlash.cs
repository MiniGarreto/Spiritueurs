using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.InputSystem;

public class PhoneFlash : MonoBehaviour
{
    public XRGrabInteractable grabInteractable;
    public InputActionProperty triggerAction;
    public Light flashLight;

    void Start()
    {
        flashLight.enabled = false;
    }

    void Update()
    {
        if (grabInteractable.isSelected &&
            triggerAction.action.WasPressedThisFrame())
        {
            ToggleFlash();
        }
    }

    void ToggleFlash()
    {
        flashLight.enabled = !flashLight.enabled;
    }
}
