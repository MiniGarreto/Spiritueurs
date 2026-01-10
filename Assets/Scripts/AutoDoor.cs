using UnityEngine;
using System.Collections;

public class AutoDoor : MonoBehaviour
{
    [Header("Référence")]
    public Transform pivotPorte;

    [Header("Ouverture")]
    public float baseOpenAngle = 90f;   // angle de base
    public float baseOpenSpeed = 10f;   // vitesse ouverture normale

    [Header("Fermeture")]
    public float fastCloseSpeed = 60f;  // vitesse fermeture rapide

    [Header("Temps d'attente pour réouverture")]
    public float minDelay = 2f;
    public float maxDelay = 5f;

    [Header("Jitter")]
    public float angleJitter = 10f;
    public float speedJitter = 3f;

    [Header("Squelette")]
    public GameObject skeletonPrefab;
    public Transform skeletonSpawnPoint;
    public Transform playerTransform;  // XR Rig ou caméra du joueur

    private GameObject currentSkeleton;

    private float targetAngle; 
    private float currentSpeed;
    private float currentAngle = 0f;

    private bool opening = false;
    private bool closing = false;
    private bool waitingToOpen = false;
    private bool canReopen = true;

    void Start()
    {
        float initialDelay = Random.Range(minDelay, maxDelay);
        StartCoroutine(WaitAndOpen(initialDelay));
    }

    void Update()
    {
        // Si un squelette est vivant, la porte ne peut pas s'ouvrir
        if(currentSkeleton != null)
        {
            SkeletonAI skeletonAI = currentSkeleton.GetComponent<SkeletonAI>();
            // Vérifier si le squelette est mort (IsAlive = false) ou détruit
            if(skeletonAI == null || !skeletonAI.IsAlive)
            {
                currentSkeleton = null; // squelette mort
                canReopen = true;
                
                // Relancer le cycle d'ouverture de la porte après la mort du squelette
                if(!waitingToOpen && !opening)
                {
                    waitingToOpen = true;
                    float delay = Random.Range(minDelay, maxDelay);
                    StartCoroutine(WaitAndOpen(delay));
                }
            }
            else
            {
                canReopen = false;
            }
        }

        // Ouverture
        if(opening && canReopen)
        {
            currentAngle += currentSpeed * Time.deltaTime;
            currentAngle = Mathf.Clamp(currentAngle, 0f, targetAngle);

            // Quand la porte est complètement ouverte
            if(currentAngle >= targetAngle)
            {
                opening = false;

                // Spawn le squelette si rien n'est présent
                if(currentSkeleton == null && skeletonPrefab != null)
                {
                    SpawnSkeleton();
                    canReopen = false; // bloque la porte tant que squelette en vie
                    StartCoroutine(CloseAfterSkeletonEnters());
                }
            }
        }
        // Fermeture
        else if(closing)
        {
            currentAngle -= currentSpeed * Time.deltaTime;
            currentAngle = Mathf.Clamp(currentAngle, 0f, targetAngle);

            // Quand la porte est complètement fermée, on peut lancer le timer pour réouverture
            if(currentAngle <= 0f && !waitingToOpen)
            {
                waitingToOpen = true;
                float delay = Random.Range(minDelay, maxDelay);
                StartCoroutine(WaitAndOpen(delay));
            }
        }

        pivotPorte.localRotation = Quaternion.Euler(0f, -currentAngle, 0f);
    }

    void SpawnSkeleton()
    {
        if(currentSkeleton == null && skeletonPrefab != null && skeletonSpawnPoint != null)
        {
            currentSkeleton = Instantiate(skeletonPrefab, skeletonSpawnPoint.position, skeletonSpawnPoint.rotation);
            SkeletonAI ai = currentSkeleton.GetComponent<SkeletonAI>();
            if(ai != null) ai.player = playerTransform;
        }
    }

    // Ferme la porte très vite (appelé quand tapée par papier toilette)
    public void CloseDoorFast()
    {
        opening = false;
        closing = true;
        waitingToOpen = false;
        currentSpeed = fastCloseSpeed;
    }

    private IEnumerator WaitAndOpen(float delay)
    {
        yield return new WaitForSeconds(delay);

        // Nouveau targetAngle avec jitter
        targetAngle = baseOpenAngle + Random.Range(-angleJitter, angleJitter);

        // Nouvelle vitesse avec jitter
        currentSpeed = baseOpenSpeed + Random.Range(-speedJitter, speedJitter);
        currentSpeed = Mathf.Max(1f, currentSpeed);

        closing = false;
        opening = true;
        waitingToOpen = false;
    }

    // Ferme la porte après que le squelette soit passé
    private IEnumerator CloseAfterSkeletonEnters()
    {
        // On attend que le squelette avance un peu (tu peux ajuster le temps)
        yield return new WaitForSeconds(1f);
        closing = true;
        currentSpeed = fastCloseSpeed;
    }
}
