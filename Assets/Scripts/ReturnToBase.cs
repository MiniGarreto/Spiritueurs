using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class ReturnToBase : MonoBehaviour
{
    public float returnDelay = 2f;
    public float returnSpeed = 6f;

    private Vector3 startPos;
    private Quaternion startRot;

    private Rigidbody rb;
    private UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable grab;
    private float timer;
    private bool returning;

    void Awake()
    {
        startPos = transform.position;
        startRot = transform.rotation;

        rb = GetComponent<Rigidbody>();
        grab = GetComponent<UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable>();

        grab.selectEntered.AddListener(OnGrab);
        grab.selectExited.AddListener(OnRelease);
    }

    void OnGrab(SelectEnterEventArgs args)
    {
        returning = false;
        timer = 0f;
        rb.isKinematic = false;
    }

    void OnRelease(SelectExitEventArgs args)
    {
        timer = 0f;
    }

    void Update()
    {
        if (grab.isSelected)
            return;

        timer += Time.deltaTime;

        if (timer >= returnDelay)
            returning = true;

        if (!returning)
            return;

        rb.isKinematic = true;

        transform.position = Vector3.Lerp(
            transform.position,
            startPos,
            Time.deltaTime * returnSpeed
        );

        transform.rotation = Quaternion.Lerp(
            transform.rotation,
            startRot,
            Time.deltaTime * returnSpeed
        );

        if (Vector3.Distance(transform.position, startPos) < 0.02f)
        {
            rb.isKinematic = false;
            returning = false;
            timer = 0f;
        }
    }
}
