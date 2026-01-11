using UnityEngine;
using TMPro;
using System.Collections;

public class DifficultyManager : MonoBehaviour
{
    public static DifficultyManager Instance { get; private set; }

    [Header("Affichage du temps")]
    public TextMeshProUGUI survivalTimeText;  // UI Text pour afficher le temps
    public Transform survivalTimeUI;           // Canvas/UI à positionner dans le monde

    [Header("Temps de survie")]
    public float survivalTime { get; private set; } = 0f;

    [Header("Phases de difficulté (en secondes)")]
    public float phase1Duration = 60f;   // Phase facile (0-60s)
    public float phase2Duration = 120f;  // Phase moyenne (60-180s)
    public float phase3Duration = 180f;  // Phase difficile (180-360s)
    // Après phase3 = Phase extrême

    [Header("Références - Squelettes")]
    public SkeletonSpawnManager skeletonSpawnManager;

    [Header("Références - Eau")]
    public BathroomTap[] bathroomTaps;             // Les robinets à activer progressivement
    public BathtubFiller[] bathtubFillers;         // Les fillers pour ajuster la vitesse
    public float timeBetweenTapActivation = 30f;   // Délai entre activation des robinets

    [Header("Références - Requin")]
    public SharkSpawner sharkSpawner;

    [Header("Références - Smilers")]
    public SmilerSpawner smilerSpawner;
    public float initialSmilerDelay = 30f;  // Délai avant que les smilers commencent à spawn

    [Header("Paramètres Squelettes - Phase 1 (Facile)")]
    public float p1_minSpawnDelay = 12f;
    public float p1_maxSpawnDelay = 20f;
    public float p1_doorOpenSpeed = 4f;

    [Header("Paramètres Squelettes - Phase 2 (Moyen)")]
    public float p2_minSpawnDelay = 8f;
    public float p2_maxSpawnDelay = 14f;
    public float p2_doorOpenSpeed = 6f;

    [Header("Paramètres Squelettes - Phase 3 (Difficile)")]
    public float p3_minSpawnDelay = 5f;
    public float p3_maxSpawnDelay = 10f;
    public float p3_doorOpenSpeed = 10f;

    [Header("Paramètres Squelettes - Phase 4 (Extrême)")]
    public float p4_minSpawnDelay = 3f;
    public float p4_maxSpawnDelay = 6f;
    public float p4_doorOpenSpeed = 14f;

    [Header("Paramètres Eau par phase")]
    public float p1_fillSpeed = 0.02f;
    public float p2_fillSpeed = 0.05f;
    public float p3_fillSpeed = 0.08f;
    public float p4_fillSpeed = 0.12f;

    [Header("Paramètres Smilers par phase (intervalle de spawn)")]
    public float p1_smilerInterval = 20f; 
    public float p2_smilerInterval = 12f;
    public float p3_smilerInterval = 8f;
    public float p4_smilerInterval = 5f;

    private int currentPhase = 0;

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
        // Désactiver les smilers au début, ils seront activés après un délai
        if (smilerSpawner != null)
        {
            smilerSpawner.StopSpawning();
        }

        // Appliquer la phase 1 (facile)
        ApplyPhase(1);

        // Programmer l'activation progressive des robinets
        StartCoroutine(ActivateTapsProgressively());

        // Programmer l'activation des smilers après un délai
        StartCoroutine(ActivateSmilersAfterDelay());
    }

    void Update()
    {
        // Incrémenter le temps de survie
        survivalTime += Time.deltaTime;

        // Mettre à jour l'affichage
        UpdateSurvivalTimeDisplay();

        // Vérifier les changements de phase
        CheckPhaseTransition();
    }

    private void UpdateSurvivalTimeDisplay()
    {
        if (survivalTimeText != null)
        {
            int minutes = Mathf.FloorToInt(survivalTime / 60f);
            int seconds = Mathf.FloorToInt(survivalTime % 60f);
            survivalTimeText.text = string.Format("{0:00}:{1:00}", minutes, seconds);
        }
    }

    private void CheckPhaseTransition()
    {
        int newPhase = GetCurrentPhase();

        if (newPhase != currentPhase)
        {
            currentPhase = newPhase;
            ApplyPhase(currentPhase);
            Debug.Log($"[Difficulté] Passage à la phase {currentPhase}");
        }
    }

    private int GetCurrentPhase()
    {
        if (survivalTime < phase1Duration)
            return 1;
        else if (survivalTime < phase1Duration + phase2Duration)
            return 2;
        else if (survivalTime < phase1Duration + phase2Duration + phase3Duration)
            return 3;
        else
            return 4;
    }

    private void ApplyPhase(int phase)
    {
        float minDelay, maxDelay, doorSpeed, fillSpeed, smilerInterval;

        switch (phase)
        {
            case 1:
                minDelay = p1_minSpawnDelay;
                maxDelay = p1_maxSpawnDelay;
                doorSpeed = p1_doorOpenSpeed;
                fillSpeed = p1_fillSpeed;
                smilerInterval = p1_smilerInterval;
                break;
            case 2:
                minDelay = p2_minSpawnDelay;
                maxDelay = p2_maxSpawnDelay;
                doorSpeed = p2_doorOpenSpeed;
                fillSpeed = p2_fillSpeed;
                smilerInterval = p2_smilerInterval;
                break;
            case 3:
                minDelay = p3_minSpawnDelay;
                maxDelay = p3_maxSpawnDelay;
                doorSpeed = p3_doorOpenSpeed;
                fillSpeed = p3_fillSpeed;
                smilerInterval = p3_smilerInterval;
                break;
            default: // Phase 4+
                minDelay = p4_minSpawnDelay;
                maxDelay = p4_maxSpawnDelay;
                doorSpeed = p4_doorOpenSpeed;
                fillSpeed = p4_fillSpeed;
                smilerInterval = p4_smilerInterval;
                break;
        }

        // Appliquer aux squelettes
        if (skeletonSpawnManager != null)
        {
            skeletonSpawnManager.minDelayBetweenSpawns = minDelay;
            skeletonSpawnManager.maxDelayBetweenSpawns = maxDelay;
        }

        // Appliquer la vitesse d'ouverture des portes
        ApplyDoorSpeed(doorSpeed);

        // Appliquer la vitesse de l'eau
        ApplyFillSpeed(fillSpeed);

        // Appliquer l'intervalle des smilers
        ApplySmilerInterval(smilerInterval);
    }

    private void ApplyDoorSpeed(float speed)
    {
        // Trouver tous les AutoDoor dans la scène
        AutoDoor[] doors = FindObjectsOfType<AutoDoor>();
        foreach (var door in doors)
        {
            door.baseOpenSpeed = speed;
        }
    }

    private void ApplyFillSpeed(float speed)
    {
        if (bathtubFillers == null) return;

        foreach (var filler in bathtubFillers)
        {
            if (filler != null)
            {
                filler.SetFillSpeed(speed);
            }
        }
    }

    private void ApplySmilerInterval(float interval)
    {
        if (smilerSpawner != null)
        {
            smilerSpawner.spawnInterval = interval;
        }
    }

    private IEnumerator ActivateSmilersAfterDelay()
    {
        yield return new WaitForSeconds(initialSmilerDelay);

        if (smilerSpawner != null)
        {
            smilerSpawner.StartSpawning();
            Debug.Log($"[Difficulté] Smilers activés à {GetFormattedTime()}");
        }
    }

    private IEnumerator ActivateTapsProgressively()
    {
        if (bathroomTaps == null || bathroomTaps.Length == 0) yield break;

        // Attendre un délai initial avant le premier robinet
        yield return new WaitForSeconds(15f);

        // Activer les robinets un par un avec un délai entre chaque
        for (int i = 0; i < bathroomTaps.Length; i++)
        {
            if (bathroomTaps[i] != null)
            {
                bathroomTaps[i].ActivateByDifficulty();
                Debug.Log($"[Difficulté] Robinet {i + 1}/{bathroomTaps.Length} activé à {GetFormattedTime()}");
            }

            // Attendre avant d'activer le suivant (sauf pour le dernier)
            if (i < bathroomTaps.Length - 1)
            {
                yield return new WaitForSeconds(timeBetweenTapActivation);
            }
        }
    }

    // Méthode pour obtenir le temps formaté (pour d'autres scripts)
    public string GetFormattedTime()
    {
        int minutes = Mathf.FloorToInt(survivalTime / 60f);
        int seconds = Mathf.FloorToInt(survivalTime % 60f);
        return string.Format("{0:00}:{1:00}", minutes, seconds);
    }

    // Méthode pour reset (nouvelle partie)
    public void ResetDifficulty()
    {
        survivalTime = 0f;
        currentPhase = 0;
        ApplyPhase(1);
    }
}
