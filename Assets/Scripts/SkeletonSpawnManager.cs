using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class SkeletonSpawnManager : MonoBehaviour
{
    public static SkeletonSpawnManager Instance { get; private set; }

    public float minDelayBetweenSpawns = 2f;
    public float maxDelayBetweenSpawns = 5f;
    public int maxSimultaneousSkeletons = 1;  
    public Transform waterTransform;         
    public float disableAtWaterYScale = 1f;  
    public AudioSource skeletonSpawnSound;  
    public AudioSource doorAudioSource;     
    private List<AutoDoor> spawnPoints = new List<AutoDoor>();
    private List<AutoDoor> activeSpawnPoints = new List<AutoDoor>();
    private AutoDoor lastUsedSpawnPoint;
    private List<GameObject> activeSkeletons = new List<GameObject>(); 
    private int pendingSpawnCoroutines = 0;
    private bool spawnDisabledByWater = false;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        StartCoroutine(ChooseAndActivateSpawnPoint());
    }

    void Update()
    {

        activeSkeletons.RemoveAll(s => s == null);
        
        if (waterTransform != null)
        {
            bool waterTooHigh = waterTransform.localScale.y >= disableAtWaterYScale;

            if (waterTooHigh && !spawnDisabledByWater)
            {
                spawnDisabledByWater = true;
                
                foreach (var spawnPoint in activeSpawnPoints.ToArray())
                {
                    if (spawnPoint != null && !HasSkeletonFromSpawnPoint(spawnPoint))
                    {
                        spawnPoint.CloseDoorFast();
                        activeSpawnPoints.Remove(spawnPoint);
                    }
                }
            }
            else if (!waterTooHigh && spawnDisabledByWater)
            {
                spawnDisabledByWater = false;
                
                if (CanSpawnMore())
                {
                    StartCoroutine(ChooseAndActivateSpawnPoint());
                }
            }
        }
    }

    private bool CanSpawnMore()
    {
        int totalPendingOrActive = activeSkeletons.Count + activeSpawnPoints.Count + pendingSpawnCoroutines;
        return totalPendingOrActive < maxSimultaneousSkeletons;
    }

    private bool HasSkeletonFromSpawnPoint(AutoDoor spawnPoint)
    {
        return activeSkeletons.Count > 0;
    }

    public bool IsSpawnDisabled()
    {
        return spawnDisabledByWater;
    }

    public void PlayDoorSound(AudioClip clip)
    {
        if (doorAudioSource != null && clip != null)
        {
            doorAudioSource.PlayOneShot(clip);
        }
    }

    public void RegisterSpawnPoint(AutoDoor spawnPoint)
    {
        if (!spawnPoints.Contains(spawnPoint))
        {
            spawnPoints.Add(spawnPoint);
        }
    }

    public void UnregisterSpawnPoint(AutoDoor spawnPoint)
    {
        spawnPoints.Remove(spawnPoint);
        activeSpawnPoints.Remove(spawnPoint);
    }

    public bool IsActiveSpawnPoint(AutoDoor spawnPoint)
    {
        return activeSpawnPoints.Contains(spawnPoint);
    }

    public bool HasActiveSkeleton()
    {
        return activeSkeletons.Count > 0;
    }

    public bool CanSpawnMoreSkeletons()
    {
        return activeSkeletons.Count < maxSimultaneousSkeletons;
    }

    public void OnSkeletonSpawned(GameObject skeleton, AutoDoor spawnDoor = null)
    {
        if (!activeSkeletons.Contains(skeleton))
        {
            activeSkeletons.Add(skeleton);
        }

        if (spawnDoor != null)
        {
            activeSpawnPoints.Remove(spawnDoor);
        }

        if (skeletonSpawnSound != null)
        {
            skeletonSpawnSound.Play();
        }
        
        if (CanSpawnMore() && !spawnDisabledByWater)
        {
            StartCoroutine(ChooseAndActivateSpawnPoint());
        }
    }

    public void OnSkeletonDied(GameObject skeleton = null)
    {
        if (skeleton != null)
        {
            activeSkeletons.Remove(skeleton);
        }
        
        activeSkeletons.RemoveAll(s => s == null);
        
        if (!spawnDisabledByWater && CanSpawnMore())
        {
            StartCoroutine(ChooseAndActivateSpawnPoint());
        }
    }

    public void OnDoorClosedByPlayer(AutoDoor closedDoor)
    {
        if (activeSpawnPoints.Contains(closedDoor))
        {
            activeSpawnPoints.Remove(closedDoor);
            
            if (!spawnDisabledByWater && CanSpawnMore())
            {
                StartCoroutine(ChooseAndActivateSpawnPoint());
            }
        }
    }

    private IEnumerator ChooseAndActivateSpawnPoint()
    {
        pendingSpawnCoroutines++;

        float delay = Random.Range(minDelayBetweenSpawns, maxDelayBetweenSpawns);
        yield return new WaitForSeconds(delay);

        pendingSpawnCoroutines--;

        if (spawnDisabledByWater)
        {
            yield break;
        }

        int totalPendingSkeletons = activeSkeletons.Count + activeSpawnPoints.Count;
        if (totalPendingSkeletons >= maxSimultaneousSkeletons)
        {
            yield break;
        }

        AutoDoor chosenSpawnPoint = ChooseRandomSpawnPoint();

        if (chosenSpawnPoint != null)
        {
            activeSpawnPoints.Add(chosenSpawnPoint);
            lastUsedSpawnPoint = chosenSpawnPoint;
            
            chosenSpawnPoint.StartOpening();
        }
    }

    private AutoDoor ChooseRandomSpawnPoint()
    {
        if (spawnPoints.Count == 0) return null;

        List<AutoDoor> availablePoints = new List<AutoDoor>();
        
        foreach (AutoDoor point in spawnPoints)
        {
            if (activeSpawnPoints.Contains(point))
                continue;
            
            if (spawnPoints.Count > 1 && point == lastUsedSpawnPoint)
                continue;
                
            availablePoints.Add(point);
        }

        if (availablePoints.Count == 0)
        {
            foreach (AutoDoor point in spawnPoints)
            {
                if (!activeSpawnPoints.Contains(point))
                    availablePoints.Add(point);
            }
        }
        
        if (availablePoints.Count == 0) return null;

        int randomIndex = Random.Range(0, availablePoints.Count);

        return availablePoints[randomIndex];
    }
}
