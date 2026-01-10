using UnityEngine;

public class DoorCollision : MonoBehaviour
{
    private AutoDoor door;

    void Awake()
    {
        door = GetComponentInParent<AutoDoor>();
    }

    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("PapierToilette"))
        {
            door.CloseDoorFast();
        }
    }
}
