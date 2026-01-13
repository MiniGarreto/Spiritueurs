using UnityEngine;

public class MoveForward : MonoBehaviour
{
    public float speed = 1f;
    public bool useLocalSpace = true;

    private void Update()
    {
        Vector3 direction = useLocalSpace ? transform.forward : Vector3.forward;
        transform.position += direction * speed * Time.deltaTime;
    }
}
