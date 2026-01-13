using UnityEngine;
using System.Collections;

public enum AxeRotation
{
    Y,  // Porte/Fenêtre (gauche-droite)
    X   // Vent/Trappe (haut-bas)
}

public class AutoDoor : MonoBehaviour
{
    [Header("Référence")]
    public Transform pivotPorte;

    [Header("Ouverture")]
    public float baseOpenAngle = 90f;   // angle de base
    public float baseOpenSpeed = 10f;   // vitesse ouverture normale
    public AxeRotation axeRotation = AxeRotation.Y;  // Y = porte, X = vent
    public bool inverserSens = false;   // inverse le sens d'ouverture
    public bool forcerEspaceMonde = false;  // cocher si étirement à cause de scale parent

    [Header("Fermeture")]
    public float fastCloseSpeed = 60f;  // vitesse fermeture rapide

    [Header("Jitter")]
    public float angleJitter = 10f;
    public float speedJitter = 3f;

    [Header("Sons d'entrée")]
    public AudioClip openingSound;      // Son joué pendant l'ouverture
    public AudioClip closingSound;      // Son joué à la fermeture

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
    private bool hasSpawnedSkeleton = false;

    private Quaternion rotationInitiale;
    private bool utiliserEspaceMonde = false;

    void Start()
    {
        // S'enregistrer auprès du manager
        if (SkeletonSpawnManager.Instance != null)
        {
            SkeletonSpawnManager.Instance.RegisterSpawnPoint(this);
        }

        // Utiliser espace monde si forcé OU si un parent a une scale non-uniforme
        utiliserEspaceMonde = forcerEspaceMonde;
        
        if(!utiliserEspaceMonde)
        {
            Transform parent = pivotPorte.parent;
            while(parent != null)
            {
                Vector3 s = parent.localScale;
                if(!Mathf.Approximately(s.x, s.y) || !Mathf.Approximately(s.y, s.z))
                {
                    utiliserEspaceMonde = true;
                    break;
                }
                parent = parent.parent;
            }
        }
        
        // Sauvegarder la rotation initiale du pivot
        rotationInitiale = utiliserEspaceMonde ? pivotPorte.rotation : pivotPorte.localRotation;
    }

    void OnDestroy()
    {
        // Se désenregistrer du manager
        if (SkeletonSpawnManager.Instance != null)
        {
            SkeletonSpawnManager.Instance.UnregisterSpawnPoint(this);
        }
    }

    void Update()
    {
        // Vérifier si le squelette spawné par cette porte est mort
        if(currentSkeleton != null)
        {
            SkeletonAI skeletonAI = currentSkeleton.GetComponent<SkeletonAI>();
            if(skeletonAI == null || !skeletonAI.IsAlive)
            {
                GameObject deadSkeleton = currentSkeleton;
                currentSkeleton = null;
                hasSpawnedSkeleton = false;
                
                // Notifier le manager que le squelette est mort (passer la référence pour le retirer de la liste)
                if (SkeletonSpawnManager.Instance != null)
                {
                    SkeletonSpawnManager.Instance.OnSkeletonDied(deadSkeleton);
                }
            }
        }

        // Ouverture
        if(opening)
        {
            currentAngle += currentSpeed * Time.deltaTime;
            currentAngle = Mathf.Clamp(currentAngle, 0f, targetAngle);

            // Quand la porte est complètement ouverte
            if(currentAngle >= targetAngle)
            {
                opening = false;

                // Spawn le squelette si c'est le point actif et qu'on peut encore spawner
                if(!hasSpawnedSkeleton && skeletonPrefab != null && 
                   SkeletonSpawnManager.Instance != null && 
                   SkeletonSpawnManager.Instance.IsActiveSpawnPoint(this) &&
                   SkeletonSpawnManager.Instance.CanSpawnMoreSkeletons())
                {
                    SpawnSkeleton();
                    StartCoroutine(CloseAfterSkeletonEnters());
                }
            }
        }
        // Fermeture
        else if(closing)
        {
            currentAngle -= currentSpeed * Time.deltaTime;
            currentAngle = Mathf.Clamp(currentAngle, 0f, targetAngle);

            if(currentAngle <= 0f)
            {
                closing = false;
            }
        }

        float direction = inverserSens ? 1f : -1f;
        float angle = direction * currentAngle;
        
        // Appliquer la rotation
        Vector3 axe = (axeRotation == AxeRotation.X) ? Vector3.right : Vector3.up;
        
        if(utiliserEspaceMonde)
        {
            pivotPorte.rotation = rotationInitiale * Quaternion.AngleAxis(angle, axe);
        }
        else
        {
            pivotPorte.localRotation = rotationInitiale * Quaternion.AngleAxis(angle, axe);
        }
    }

    void SpawnSkeleton()
    {
        if(currentSkeleton == null && skeletonPrefab != null && skeletonSpawnPoint != null)
        {
            currentSkeleton = Instantiate(skeletonPrefab, skeletonSpawnPoint.position, skeletonSpawnPoint.rotation);
            SkeletonAI ai = currentSkeleton.GetComponent<SkeletonAI>();
            if(ai != null) ai.player = playerTransform;
            
            hasSpawnedSkeleton = true;
            
            // Notifier le manager (passer aussi la référence de cette porte)
            if (SkeletonSpawnManager.Instance != null)
            {
                SkeletonSpawnManager.Instance.OnSkeletonSpawned(currentSkeleton, this);
            }
        }
    }

    // Appelé par le manager pour démarrer l'ouverture
    public void StartOpening()
    {
        // Nouveau targetAngle avec jitter
        targetAngle = baseOpenAngle + Random.Range(-angleJitter, angleJitter);

        // Nouvelle vitesse avec jitter
        currentSpeed = baseOpenSpeed + Random.Range(-speedJitter, speedJitter);
        currentSpeed = Mathf.Max(1f, currentSpeed);

        closing = false;
        opening = true;
        hasSpawnedSkeleton = false;

        // Jouer le son d'ouverture
        PlayOpeningSound();
    }

    // Ferme la porte très vite (appelé quand tapée par papier toilette)
    public void CloseDoorFast()
    {
        bool wasOpening = opening;
        
        opening = false;
        closing = true;
        currentSpeed = fastCloseSpeed;

        // Jouer le son de fermeture via le manager
        PlayClosingSound();
        
        // Si la porte était en train de s'ouvrir et qu'aucun squelette n'a été spawné,
        // notifier le manager pour choisir un nouveau point de spawn
        if (wasOpening && !hasSpawnedSkeleton && SkeletonSpawnManager.Instance != null)
        {
            SkeletonSpawnManager.Instance.OnDoorClosedByPlayer(this);
        }
    }

    private void PlayOpeningSound()
    {
        if (SkeletonSpawnManager.Instance != null && openingSound != null)
        {
            SkeletonSpawnManager.Instance.PlayDoorSound(openingSound);
        }
    }

    private void PlayClosingSound()
    {
        if (SkeletonSpawnManager.Instance != null && closingSound != null)
        {
            SkeletonSpawnManager.Instance.PlayDoorSound(closingSound);
        }
    }

    // Ferme la porte après que le squelette soit passé
    private IEnumerator CloseAfterSkeletonEnters()
    {
        yield return new WaitForSeconds(1f);
        closing = true;
        opening = false;
        currentSpeed = fastCloseSpeed;
        
        // Jouer le son de fermeture
        PlayClosingSound();
    }
}
