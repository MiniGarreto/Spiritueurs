using UnityEngine;
using System.Collections;

public enum AxeRotation
{
    Y,
    X
}

public class AutoDoor : MonoBehaviour
{
    public Transform pivotPorte;
    public float baseOpenAngle = 90f;   
    public float baseOpenSpeed = 10f;  
    public AxeRotation axeRotation = AxeRotation.Y;
    public bool inverserSens = false;  
    public bool forcerEspaceMonde = false;  
    public float fastCloseSpeed = 60f;
    public float angleJitter = 10f;
    public float speedJitter = 3f;
    public AudioClip openingSound;     
    public AudioClip closingSound;      
    public GameObject skeletonPrefab;
    public Transform skeletonSpawnPoint;
    public Transform playerTransform;

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
        if (SkeletonSpawnManager.Instance != null){
            SkeletonSpawnManager.Instance.RegisterSpawnPoint(this);
        }

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
        
        rotationInitiale = utiliserEspaceMonde ? pivotPorte.rotation : pivotPorte.localRotation;
    }

    void OnDestroy()
    {
        if (SkeletonSpawnManager.Instance != null)
        {
            SkeletonSpawnManager.Instance.UnregisterSpawnPoint(this);
        }
    }

    void Update()
    {
        if(currentSkeleton != null)
        {
            SkeletonAI skeletonAI = currentSkeleton.GetComponent<SkeletonAI>();
            if(skeletonAI == null || !skeletonAI.IsAlive)
            {
                GameObject deadSkeleton = currentSkeleton;
                currentSkeleton = null;
                hasSpawnedSkeleton = false;
                
                if (SkeletonSpawnManager.Instance != null)
                {
                    SkeletonSpawnManager.Instance.OnSkeletonDied(deadSkeleton);
                }
            }
        }

        if(opening)
        {
            currentAngle += currentSpeed * Time.deltaTime;
            currentAngle = Mathf.Clamp(currentAngle, 0f, targetAngle);

            if(currentAngle >= targetAngle)
            {
                opening = false;

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
            
            if (SkeletonSpawnManager.Instance != null)
            {
                SkeletonSpawnManager.Instance.OnSkeletonSpawned(currentSkeleton, this);
            }
        }
    }

    public void StartOpening()
    {
        targetAngle = baseOpenAngle + Random.Range(-angleJitter, angleJitter);

        currentSpeed = baseOpenSpeed + Random.Range(-speedJitter, speedJitter);
        currentSpeed = Mathf.Max(1f, currentSpeed);

        closing = false;
        opening = true;
        hasSpawnedSkeleton = false;

        PlayOpeningSound();
    }

    public void CloseDoorFast()
    {
        bool wasOpening = opening;
        
        opening = false;
        closing = true;
        currentSpeed = fastCloseSpeed;

        PlayClosingSound();
        
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

    private IEnumerator CloseAfterSkeletonEnters()
    {
        yield return new WaitForSeconds(1f);
        closing = true;
        opening = false;
        currentSpeed = fastCloseSpeed;
        
        PlayClosingSound();
    }
}
