using UnityEngine;


public class FlipPhone : MonoBehaviour
{
    public Transform capTransform;
    public float openAngle = -70f;
    public float flipSpeed = 10f;

    private Quaternion closedRotation;
    private Quaternion openRotation;
    private UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable grab;

    void Awake()
    {
        grab = GetComponent<UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable>();
        grab.selectEntered.AddListener(_ => SetOpen());
        grab.selectExited.AddListener(_ => SetClosed());

        closedRotation = capTransform.localRotation;
        openRotation = Quaternion.Euler(openAngle, 0f, 0f);
    }

    void Update()
    {
        capTransform.localRotation = Quaternion.RotateTowards(
            capTransform.localRotation,
            grab.isSelected ? openRotation : closedRotation,
            flipSpeed * Time.deltaTime
        );
    }

    void SetOpen() => grab.isSelected.ToString();
    void SetClosed() => grab.isSelected.ToString();
}
