using UnityEngine;
using UnityEngine.XR;
using UnityEngine.XR.Interaction.Toolkit;

[RequireComponent(typeof(UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable))]
public class PhoneFlashGrabTrigger : MonoBehaviour
{
    public PhoneFlashCone phoneFlash;

    private UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable grabInteractable;
    private UnityEngine.XR.Interaction.Toolkit.Interactors.IXRSelectInteractor currentInteractor;
    private InputDevice targetDevice;
    private bool lastTriggerPressed = false;

    private void Reset()
    {
        phoneFlash = GetComponentInChildren<PhoneFlashCone>();
        grabInteractable = GetComponent<UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable>();
    }

    private void OnValidate()
    {
        if (phoneFlash == null) phoneFlash = GetComponentInChildren<PhoneFlashCone>();
        if (grabInteractable == null) grabInteractable = GetComponent<UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable>();
    }

    private void OnEnable()
    {
        grabInteractable = GetComponent<UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable>();
        grabInteractable.selectEntered.AddListener(OnSelectEntered);
        grabInteractable.selectExited.AddListener(OnSelectExited);
    }

    private void OnDisable()
    {
        if (grabInteractable != null)
        {
            grabInteractable.selectEntered.RemoveListener(OnSelectEntered);
            grabInteractable.selectExited.RemoveListener(OnSelectExited);
        }
    }

    private void OnSelectEntered(SelectEnterEventArgs args)
    {
        currentInteractor = args.interactorObject;
        FindAndAssignNearestController(currentInteractor.transform);
        lastTriggerPressed = false;
    }

    private void OnSelectExited(SelectExitEventArgs args)
    {
        currentInteractor = null;
        targetDevice = default;
        lastTriggerPressed = false;
    }

    private void Update()
    {
        if (currentInteractor == null || phoneFlash == null) return;

        if (!targetDevice.isValid)
            FindAndAssignNearestController(currentInteractor.transform);

        bool pressed = false;
        if (targetDevice.isValid)
        {
            targetDevice.TryGetFeatureValue(CommonUsages.triggerButton, out pressed);
        }
        else
        {
            return;
        }

        if (pressed && !lastTriggerPressed)
        {
            phoneFlash.TriggerFlash();
            if (targetDevice.isValid && targetDevice.TryGetHapticCapabilities(out var caps) && caps.supportsImpulse)
            {
                targetDevice.SendHapticImpulse(0, 0.3f, 0.05f);
            }
        }

        lastTriggerPressed = pressed;
    }

    private void FindAndAssignNearestController(Transform reference)
    {
        var devices = new System.Collections.Generic.List<InputDevice>();
        InputDevices.GetDevicesWithCharacteristics(InputDeviceCharacteristics.Controller | InputDeviceCharacteristics.HeldInHand, devices);
        if (devices.Count == 0)
        {
            Debug.LogWarning("PhoneFlashGrabTrigger: aucun contrôleur trouvé.", this);
            targetDevice = default;
            return;
        }

        float bestDist = float.MaxValue;
        InputDevice best = default;
        foreach (var d in devices)
        {
            if (d.TryGetFeatureValue(CommonUsages.devicePosition, out var pos))
            {
                float dist = (pos - reference.position).sqrMagnitude;
                if (dist < bestDist)
                {
                    bestDist = dist;
                    best = d;
                }
            }
            else
            {
                if (!best.isValid) best = d;
            }
        }

        if (best.isValid)
            targetDevice = best;
        else
            targetDevice = default;
    }
}