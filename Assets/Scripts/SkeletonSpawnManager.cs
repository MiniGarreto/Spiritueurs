using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class SkeletonSpawnManager : MonoBehaviour
{
    public static SkeletonSpawnManager Instance { get; private set; }

    [Header("Configuration")]
    public float minDelayBetweenSpawns = 2f;
    public float maxDelayBetweenSpawns = 5f;

    [Header("Désactivation par niveau d'eau")]
    public Transform waterTransform;          // L'eau qui monte
    public float disableAtWaterYScale = 1f;   // Désactiver le spawn quand l'eau dépasse ce scale Y

    private List<AutoDoor> spawnPoints = new List<AutoDoor>();
    private AutoDoor currentActiveSpawnPoint;
    private AutoDoor lastUsedSpawnPoint;
    private GameObject currentSkeleton;
    private bool isWaitingToChooseSpawn = false;
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
        // Démarre le premier cycle de spawn après un délai
        StartCoroutine(ChooseAndActivateSpawnPoint());
    }

    void Update()
    {
        // Vérifier le niveau d'eau
        if (waterTransform != null)
        {
            bool waterTooHigh = waterTransform.localScale.y >= disableAtWaterYScale;

            // L'eau vient de dépasser le seuil
            if (waterTooHigh && !spawnDisabledByWater)
            {
                spawnDisabledByWater = true;
                
                // Fermer la porte active s'il y en a une
                if (currentActiveSpawnPoint != null && currentSkeleton == null)
                {
                    currentActiveSpawnPoint.CloseDoorFast();
                    currentActiveSpawnPoint = null;
                }
            }
            // L'eau vient de redescendre sous le seuil
            else if (!waterTooHigh && spawnDisabledByWater)
            {
                spawnDisabledByWater = false;
                
                // Relancer le cycle de spawn
                if (!isWaitingToChooseSpawn && currentSkeleton == null)
                {
                    StartCoroutine(ChooseAndActivateSpawnPoint());
                }
            }
        }
    }

    // Vérifie si le spawn est désactivé par l'eau
    public bool IsSpawnDisabled()
    {
        return spawnDisabledByWater;
    }

    // Appelé par chaque AutoDoor pour s'enregistrer
    public void RegisterSpawnPoint(AutoDoor spawnPoint)
    {
        if (!spawnPoints.Contains(spawnPoint))
        {
            spawnPoints.Add(spawnPoint);
        }
    }

    // Appelé par chaque AutoDoor pour se désenregistrer
    public void UnregisterSpawnPoint(AutoDoor spawnPoint)
    {
        spawnPoints.Remove(spawnPoint);
    }

    // Vérifie si ce spawn point est celui actuellement actif
    public bool IsActiveSpawnPoint(AutoDoor spawnPoint)
    {
        return currentActiveSpawnPoint == spawnPoint;
    }

    // Vérifie si un squelette existe déjà
    public bool HasActiveSkeleton()
    {
        return currentSkeleton != null;
    }

    // Appelé quand un squelette est spawné
    public void OnSkeletonSpawned(GameObject skeleton)
    {
        currentSkeleton = skeleton;
    }

    // Appelé quand un squelette meurt
    public void OnSkeletonDied()
    {
        currentSkeleton = null;
        currentActiveSpawnPoint = null;
        
        // Choisir un nouveau point de spawn seulement si le spawn n'est pas désactivé
        if (!isWaitingToChooseSpawn && !spawnDisabledByWater)
        {
            StartCoroutine(ChooseAndActivateSpawnPoint());
        }
    }

    // Appelé quand le joueur ferme une porte avec du papier toilette
    public void OnDoorClosedByPlayer(AutoDoor closedDoor)
    {
        // Si c'était le point de spawn actif et qu'aucun squelette n'est encore spawné
        if (closedDoor == currentActiveSpawnPoint && currentSkeleton == null)
        {
            currentActiveSpawnPoint = null;
            
            // Choisir un nouveau point de spawn différent seulement si le spawn n'est pas désactivé
            if (!isWaitingToChooseSpawn && !spawnDisabledByWater)
            {
                StartCoroutine(ChooseAndActivateSpawnPoint());
            }
        }
    }

    private IEnumerator ChooseAndActivateSpawnPoint()
    {
        isWaitingToChooseSpawn = true;

        // Attendre un délai aléatoire
        float delay = Random.Range(minDelayBetweenSpawns, maxDelayBetweenSpawns);
        yield return new WaitForSeconds(delay);

        // Ne pas spawner si désactivé par l'eau
        if (spawnDisabledByWater)
        {
            isWaitingToChooseSpawn = false;
            yield break;
        }

        // Choisir un point de spawn différent du dernier utilisé
        AutoDoor chosenSpawnPoint = ChooseRandomSpawnPoint();

        if (chosenSpawnPoint != null)
        {
            currentActiveSpawnPoint = chosenSpawnPoint;
            lastUsedSpawnPoint = chosenSpawnPoint;
            
            // Activer l'ouverture de ce point de spawn
            chosenSpawnPoint.StartOpening();
        }

        isWaitingToChooseSpawn = false;
    }

    private AutoDoor ChooseRandomSpawnPoint()
    {
        if (spawnPoints.Count == 0) return null;

        // Créer une liste des points disponibles (exclure le dernier utilisé si possible)
        List<AutoDoor> availablePoints = new List<AutoDoor>();
        
        foreach (AutoDoor point in spawnPoints)
        {
            // Ne pas choisir le même point que la dernière fois (si on a plus d'un point)
            if (spawnPoints.Count > 1 && point == lastUsedSpawnPoint)
                continue;
                
            availablePoints.Add(point);
        }

        // Si aucun point disponible (ne devrait pas arriver), utiliser tous les points
        if (availablePoints.Count == 0)
        {
            availablePoints = new List<AutoDoor>(spawnPoints);
        }

        // Choisir aléatoirement parmi les points disponibles
        int randomIndex = Random.Range(0, availablePoints.Count);
        return availablePoints[randomIndex];
    }
}
