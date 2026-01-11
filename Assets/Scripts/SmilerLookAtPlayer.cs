using UnityEngine;

public class SmilerLookAtPlayer : MonoBehaviour
{
    public Transform target;
    public float rotationSpeed = 5f;
    public bool yOnly = true;

    void Start()
    {
        if (target == null)
        {
            if (Camera.main != null) target = Camera.main.transform;
            else target = GameObject.FindWithTag("Player")?.transform;
        }
    }

    void LateUpdate()
    {
        if (target == null) return;
        Vector3 dir = target.position - transform.position;
        if (yOnly) dir.y = 0f;
        if (dir.sqrMagnitude < 0.0001f) return;
        Quaternion desired = Quaternion.LookRotation(dir);
        transform.rotation = Quaternion.Slerp(transform.rotation, desired, rotationSpeed * Time.deltaTime);
    }
}