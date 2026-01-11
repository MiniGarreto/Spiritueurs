using UnityEngine;

public class TrashBin : MonoBehaviour
{
    [Header("Référence à la lampe")]
    public DiscoLight discoLight;  // La lampe qui fera l'effet disco

    [Header("Configuration")]
    public bool destroyPaper = true;  // Détruire le papier quand il tombe dans la poubelle

    private void OnTriggerEnter(Collider other)
    {
        // Vérifier si c'est un papier toilette
        if (other.gameObject.CompareTag("PapierToilette"))
        {
            // Activer le mode disco sur la lampe
            if (discoLight != null)
            {
                discoLight.StartDiscoMode();
            }

            // Optionnel : détruire le papier
            if (destroyPaper)
            {
                Destroy(other.gameObject);
            }
        }
    }

    // Alternative avec OnCollisionEnter si tu utilises des colliders non-trigger
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("PapierToilette"))
        {
            if (discoLight != null)
            {
                discoLight.StartDiscoMode();
            }

            if (destroyPaper)
            {
                Destroy(collision.gameObject);
            }
        }
    }
}
