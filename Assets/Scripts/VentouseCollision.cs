using UnityEngine;

public class VentouseCollision : MonoBehaviour
{
    // On détecte les collisions
    private void OnCollisionEnter(Collision collision)
    {
        // Si on touche un squelette
        if(collision.gameObject.CompareTag("skeleton"))
        {
            SkeletonAI skeleton = collision.gameObject.GetComponent<SkeletonAI>();
            if(skeleton != null)
            {
                skeleton.Die(); // fait mourir le squelette
            }
        }
    }
}
