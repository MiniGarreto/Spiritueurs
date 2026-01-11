using System.Collections;
using UnityEngine;

public class ExampleFlashableEnemy : MonoBehaviour, IFlashable
{
    [Header("Example reaction")]
    public GameObject flashDeathVFX;

    public void OnFlashed()
    {
        if (flashDeathVFX != null) Instantiate(flashDeathVFX, transform.position, Quaternion.identity);
        // Exemple simple : détruire l'ennemi
        Destroy(gameObject);
    }
}