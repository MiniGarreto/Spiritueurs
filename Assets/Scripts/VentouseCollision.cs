using UnityEngine;

public class VentouseCollision : MonoBehaviour
{
    private void OnCollisionEnter(Collision collision)
    {
        if(collision.gameObject.CompareTag("skeleton"))
        {
            SkeletonAI skeleton = collision.gameObject.GetComponent<SkeletonAI>();
            if(skeleton != null)
            {
                skeleton.Die();
            }
        }
    }
}
