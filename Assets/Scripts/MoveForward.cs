using UnityEngine;

public class MoveForward : MonoBehaviour
{
    [Header("Paramètres de déplacement")]
    [Tooltip("Vitesse d'avancement en unités par seconde.")]
    public float speed = 1f;

    [Tooltip("Si activé, l'entité avance selon son axe Z local (forward). Sinon, selon l'axe Z global.")]
    public bool useLocalSpace = true;

    private void Update()
    {
        // Détermine la direction d'avancement
        Vector3 direction = useLocalSpace ? transform.forward : Vector3.forward;

        // Avance en continu à chaque frame
        transform.position += direction * speed * Time.deltaTime;
    }
}
