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

    [Header("Son de spawn")]
    public AudioSource skeletonSpawnSound;    // AudioSource pour le son de spawn

    [Header("Son des entrées (porte/fenêtre/vent)")]
    public AudioSource doorAudioSource;       // AudioSource global pour les sons des entrées

    private List<AutoDoor> spawnPoints = new List<AutoDoor>();
    private List<AutoDoor> activeSpawnPoints = new List<AutoDoor>();  // Portes en train de s'ouvrir
    private AutoDoor lastUsedSpawnPoint;
    private List<GameObject> activeSkeletons = new List<GameObject>();  // Squelettes actifs
    private int pendingSpawnCoroutines = 0;  // Nombre de coroutines de spawn en attente
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
                if (CanSpawnMore())
                {
                    StartCoroutine(ChooseAndActivateSpawnPoint());
                }
            }
        }
    }

    // Vérifie si on peut spawner plus de squelettes
    // Prend en compte: squelettes actifs + portes en train de s'ouvrir + spawns en attente
    private bool CanSpawnMore()
    {
        int totalPendingOrActive = activeSkeletons.Count + activeSpawnPoints.Count + pendingSpawnCoroutines;
        return totalPendingOrActive < maxSimultaneousSkeletons;
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

    // Joue un son de porte/fenêtre/vent via l'AudioSource global
    public void PlayDoorSound(AudioClip clip)
    {
        if (doorAudioSource != null && clip != null)
        {
            doorAudioSource.PlayOneShot(clip);
        }
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

    // Vérifie si on peut spawner plus de squelettes (utilisé par AutoDoor)
    // Note: Cette porte est déjà dans activeSpawnPoints, donc on vérifie juste les squelettes actifs
    public bool CanSpawnMoreSkeletons()
    {
        return activeSkeletons.Count < maxSimultaneousSkeletons;
    }

    // Appelé quand un squelette est spawné
    public void OnSkeletonSpawned(GameObject skeleton, AutoDoor spawnDoor = null)
    {
        if (!activeSkeletons.Contains(skeleton))
        {
            activeSkeletons.Add(skeleton);
        }

        // Retirer la porte de la liste des points actifs (elle a fait son travail)
        if (spawnDoor != null)
        {
            activeSpawnPoints.Remove(spawnDoor);
        }

        // Jouer le son de spawn
        if (skeletonSpawnSound != null)
        {
            skeletonSpawnSound.Play();
        }
        
        // Si on peut encore spawner plus, lancer un nouveau cycle de spawn
        if (CanSpawnMore() && !spawnDisabledByWater)
        {
            StartCoroutine(ChooseAndActivateSpawnPoint());
        }
    }

    // Appelé quand un squelette meurt
    public void OnSkeletonDied(GameObject skeleton = null)
    {
        // Retirer le squelette de la liste
        if (skeleton != null)
        {
            activeSkeletons.Remove(skeleton);
        }
        
        // Nettoyer aussi les entrées null au cas où
        activeSkeletons.RemoveAll(s => s == null);
        
        // Choisir un nouveau point de spawn si on peut (toujours lancer un nouveau cycle)
        if (!spawnDisabledByWater && CanSpawnMore())
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
            
            // Choisir un nouveau point de spawn si on peut (toujours lancer un nouveau cycle)
            if (!spawnDisabledByWater && CanSpawnMore())
            {
                StartCoroutine(ChooseAndActivateSpawnPoint());
            }
        }
    }

    private IEnumerator ChooseAndActivateSpawnPoint()
    {
        pendingSpawnCoroutines++;

        // Attendre un délai aléatoire
        float delay = Random.Range(minDelayBetweenSpawns, maxDelayBetweenSpawns);
        yield return new WaitForSeconds(delay);

        pendingSpawnCoroutines--;

        // Ne pas spawner si désactivé par l'eau
        if (spawnDisabledByWater)
        {
            yield break;
        }

        // Vérifier si on peut encore spawner (compter les squelettes actifs + portes ouvertes + spawns en attente)
        int totalPendingSkeletons = activeSkeletons.Count + activeSpawnPoints.Count;
        if (totalPendingSkeletons >= maxSimultaneousSkeletons)
        {
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
