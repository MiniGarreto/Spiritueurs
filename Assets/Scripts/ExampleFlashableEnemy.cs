using System.Collections;
using UnityEngine;

public class ExampleFlashableEnemy : MonoBehaviour, IFlashable
{
    public GameObject flashDeathVFX;

    public void OnFlashed()
    {
        if (flashDeathVFX != null) Instantiate(flashDeathVFX, transform.position, Quaternion.identity);
        Destroy(gameObject);
    }
}