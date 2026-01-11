using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class SkeletonSpawnManager : MonoBehaviour
{
    public static SkeletonSpawnManager Instance { get; private set; }

    [Header("Configuration")]
    public float minDelayBetweenSpawns = 2f;
    public float maxDelayBetweenSpawns = 5f;

    [Header("Nombre maximum de squelettes simultanés")]
    public int maxSimultaneousSkeletons = 1;  // Modifié par DifficultyManager

    [Header("Désactivation par niveau d'eau")]
    public Transform waterTransform;          // L'eau qui monte
    public float disableAtWaterYScale = 1f;   // Désactiver le spawn quand l'eau dépasse ce scale Y

    private List<AutoDoor> spawnPoints = new List<AutoDoor>();
    private List<AutoDoor> activeSpawnPoints = new List<AutoDoor>();  // Plusieurs points actifs possibles
    private AutoDoor lastUsedSpawnPoint;
    private List<GameObject> activeSkeletons = new List<GameObject>();  // Plusieurs squelettes possibles
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
        // Nettoyer les squelettes détruits de la liste
        activeSkeletons.RemoveAll(s => s == null);
        
        // Vérifier le niveau d'eau
        if (waterTransform != null)
        {
            bool waterTooHigh = waterTransform.localScale.y >= disableAtWaterYScale;

            // L'eau vient de dépasser le seuil
            if (waterTooHigh && !spawnDisabledByWater)
            {
                spawnDisabledByWater = true;
                
                // Fermer les portes actives qui n'ont pas encore spawné de squelette
                foreach (var spawnPoint in activeSpawnPoints.ToArray())
                {
                    if (spawnPoint != null && !HasSkeletonFromSpawnPoint(spawnPoint))
                    {
                        spawnPoint.CloseDoorFast();
                        activeSpawnPoints.Remove(spawnPoint);
                    }
                }
            }
            // L'eau vient de redescendre sous le seuil
            else if (!waterTooHigh && spawnDisabledByWater)
            {
                spawnDisabledByWater = false;
                
                // Relancer le cycle de spawn si on peut encore spawner
                if (!isWaitingToChooseSpawn && CanSpawnMore())
                {
                    StartCoroutine(ChooseAndActivateSpawnPoint());
                }
            }
        }
    }

    // Vérifie si on peut spawner plus de squelettes
    private bool CanSpawnMore()
    {
        return activeSkeletons.Count < maxSimultaneousSkeletons;
    }

    // Vérifie si un spawn point a un squelette actif
    private bool HasSkeletonFromSpawnPoint(AutoDoor spawnPoint)
    {
        // On ne peut pas facilement savoir quel squelette vient de quel spawn
        // Donc on suppose que si le point est actif et il y a des squelettes, on ne ferme pas
        return activeSkeletons.Count > 0;
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
        activeSpawnPoints.Remove(spawnPoint);
    }

    // Vérifie si ce spawn point est celui actuellement actif
    public bool IsActiveSpawnPoint(AutoDoor spawnPoint)
    {
        return activeSpawnPoints.Contains(spawnPoint);
    }

    // Vérifie si un squelette existe déjà
    public bool HasActiveSkeleton()
    {
        return activeSkeletons.Count > 0;
    }

    // Appelé quand un squelette est spawné
    public void OnSkeletonSpawned(GameObject skeleton)
    {
        if (!activeSkeletons.Contains(skeleton))
        {
            activeSkeletons.Add(skeleton);
        }
        
        // Si on peut encore spawner plus, lancer un nouveau cycle
        if (CanSpawnMore() && !isWaitingToChooseSpawn && !spawnDisabledByWater)
        {
            StartCoroutine(ChooseAndActivateSpawnPoint());
        }
    }

    // Appelé quand un squelette meurt
    public void OnSkeletonDied()
    {
        // La liste sera nettoyée dans Update() car on ne sait pas quel squelette est mort
        
        // Choisir un nouveau point de spawn si on peut
        if (!isWaitingToChooseSpawn && !spawnDisabledByWater && CanSpawnMore())
        {
            StartCoroutine(ChooseAndActivateSpawnPoint());
        }
    }

    // Appelé quand le joueur ferme une porte avec du papier toilette
    public void OnDoorClosedByPlayer(AutoDoor closedDoor)
    {
        // Si c'était un point de spawn actif
        if (activeSpawnPoints.Contains(closedDoor))
        {
            activeSpawnPoints.Remove(closedDoor);
            
            // Choisir un nouveau point de spawn si on peut
            if (!isWaitingToChooseSpawn && !spawnDisabledByWater && CanSpawnMore())
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

        // Ne pas spawner si désactivé par l'eau ou si on a atteint le max
        if (spawnDisabledByWater || !CanSpawnMore())
        {
            isWaitingToChooseSpawn = false;
            yield break;
        }

        // Choisir un point de spawn différent du dernier utilisé et pas déjà actif
        AutoDoor chosenSpawnPoint = ChooseRandomSpawnPoint();

        if (chosenSpawnPoint != null)
        {
            activeSpawnPoints.Add(chosenSpawnPoint);
            lastUsedSpawnPoint = chosenSpawnPoint;
            
            // Activer l'ouverture de ce point de spawn
            chosenSpawnPoint.StartOpening();
        }

        isWaitingToChooseSpawn = false;
    }

    private AutoDoor ChooseRandomSpawnPoint()
    {
        if (spawnPoints.Count == 0) return null;

        // Créer une liste des points disponibles (exclure le dernier utilisé et les déjà actifs)
        List<AutoDoor> availablePoints = new List<AutoDoor>();
        
        foreach (AutoDoor point in spawnPoints)
        {
            // Ne pas choisir un point déjà actif
            if (activeSpawnPoints.Contains(point))
                continue;
            
            // Ne pas choisir le même point que la dernière fois (si on a plus d'un point disponible)
            if (spawnPoints.Count > 1 && point == lastUsedSpawnPoint)
                continue;
                
            availablePoints.Add(point);
        }

        // Si aucun point disponible à cause du lastUsed, ignorer cette règle
        if (availablePoints.Count == 0)
        {
            foreach (AutoDoor point in spawnPoints)
            {
                if (!activeSpawnPoints.Contains(point))
                    availablePoints.Add(point);
            }
        }
        
        if (availablePoints.Count == 0) return null;

        // Choisir aléatoirement parmi les points disponibles
        int randomIndex = Random.Range(0, availablePoints.Count);
        return availablePoints[randomIndex];
    }
}
