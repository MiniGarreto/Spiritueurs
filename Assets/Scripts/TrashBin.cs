using UnityEngine;

public class TrashBin : MonoBehaviour
{
    public DiscoLight discoLight;
    public bool destroyPaper = true;  

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("PapierToilette"))
        {
            if (discoLight != null)
            {
                discoLight.StartDiscoMode();
            }

            if (destroyPaper)
            {
                Destroy(other.gameObject);
            }
        }
    }

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
