using UnityEngine;
using TMPro;
using System.Collections;

public class DifficultyManager : MonoBehaviour
{
    public static DifficultyManager Instance { get; private set; }

    [Header("Affichage du temps")]
    public TextMeshProUGUI survivalTimeText; 
    public Transform survivalTimeUI;         

    [Header("Temps de survie")]
    public float survivalTime { get; private set; } = 0f;

    [Header("Phases de difficulté (en secondes)")]
    public float phase1Duration = 60f;   //  facil
    public float phase2Duration = 120f;  //  moyenne
    public float phase3Duration = 120f;  // difficil

    [Header("Références - Squelettes")]
    public SkeletonSpawnManager skeletonSpawnManager;

    [Header("Références - Eau")]
    public BathroomTap[] bathroomTaps;           
    public BathtubFiller[] bathtubFillers;        
    public float timeBetweenTapActivation = 15f;   

    [Header("Références - Requin")]
    public SharkSpawner sharkSpawner;

    [Header("Références - Smilers")]
    public SmilerSpawner smilerSpawner;
    public float initialSmilerDelay = 25f; 

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
    public float p4_minSpawnDelay = 2f;
    public float p4_maxSpawnDelay = 5f;
    public float p4_doorOpenSpeed = 14f;

    [Header("Paramètres Eau par phase")]
    public float p1_fillSpeed = 0.02f;
    public float p2_fillSpeed = 0.05f;
    public float p3_fillSpeed = 0.08f;
    public float p4_fillSpeed = 0.12f;

    [Header("Paramètres Smilers par phase (intervalle de spawn)")]
    public float p1_smilerInterval = 15f; 
    public float p2_smilerInterval = 10f;
    public float p3_smilerInterval = 6f;
    public float p4_smilerInterval = 2f;

    [Header("Paramètres Robinets par phase (temps avant réouverture)")]
    public float p1_tapMinWait = 25f;
    public float p1_tapMaxWait = 35f;
    public float p2_tapMinWait = 15f;
    public float p2_tapMaxWait = 25;
    public float p3_tapMinWait = 10f;
    public float p3_tapMaxWait = 15;
    public float p4_tapMinWait = 5f;
    public float p4_tapMaxWait = 8;

    [Header("Nombre de squelettes simultanés par phase")]
    public int p1_maxSkeletons = 1;
    public int p2_maxSkeletons = 1;
    public int p3_maxSkeletons = 2;
    public int p4_maxSkeletons = 3;

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
        if (smilerSpawner != null)
        {
            smilerSpawner.StopSpawning();
        }

        ApplyPhase(1);

        StartCoroutine(ActivateTapsProgressively());
        StartCoroutine(ActivateSmilersAfterDelay());
    }

    void Update()
    {
        survivalTime += Time.deltaTime;
        UpdateSurvivalTimeDisplay();
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
        float tapMinWait, tapMaxWait;
        int maxSkeletons;

        switch (phase)
        {
            case 1:
                minDelay = p1_minSpawnDelay;
                maxDelay = p1_maxSpawnDelay;
                doorSpeed = p1_doorOpenSpeed;
                fillSpeed = p1_fillSpeed;
                smilerInterval = p1_smilerInterval;
                tapMinWait = p1_tapMinWait;
                tapMaxWait = p1_tapMaxWait;
                maxSkeletons = p1_maxSkeletons;
                break;
            case 2:
                minDelay = p2_minSpawnDelay;
                maxDelay = p2_maxSpawnDelay;
                doorSpeed = p2_doorOpenSpeed;
                fillSpeed = p2_fillSpeed;
                smilerInterval = p2_smilerInterval;
                tapMinWait = p2_tapMinWait;
                tapMaxWait = p2_tapMaxWait;
                maxSkeletons = p2_maxSkeletons;
                break;
            case 3:
                minDelay = p3_minSpawnDelay;
                maxDelay = p3_maxSpawnDelay;
                doorSpeed = p3_doorOpenSpeed;
                fillSpeed = p3_fillSpeed;
                smilerInterval = p3_smilerInterval;
                tapMinWait = p3_tapMinWait;
                tapMaxWait = p3_tapMaxWait;
                maxSkeletons = p3_maxSkeletons;
                break;
            default: 
                minDelay = p4_minSpawnDelay;
                maxDelay = p4_maxSpawnDelay;
                doorSpeed = p4_doorOpenSpeed;
                fillSpeed = p4_fillSpeed;
                smilerInterval = p4_smilerInterval;
                tapMinWait = p4_tapMinWait;
                tapMaxWait = p4_tapMaxWait;
                maxSkeletons = p4_maxSkeletons;
                break;
        }

        if (skeletonSpawnManager != null)
        {
            skeletonSpawnManager.minDelayBetweenSpawns = minDelay;
            skeletonSpawnManager.maxDelayBetweenSpawns = maxDelay;
            skeletonSpawnManager.maxSimultaneousSkeletons = maxSkeletons;
        }

        ApplyDoorSpeed(doorSpeed);
        ApplyFillSpeed(fillSpeed);
        ApplySmilerInterval(smilerInterval);
        ApplyTapWaitTime(tapMinWait, tapMaxWait);
    }

    private void ApplyDoorSpeed(float speed)
    {
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

    private void ApplyTapWaitTime(float minWait, float maxWait)
    {
        if (bathroomTaps == null) return;

        foreach (var tap in bathroomTaps)
        {
            if (tap != null)
            {
                tap.minWaitTime = minWait;
                tap.maxWaitTime = maxWait;
            }
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

        yield return new WaitForSeconds(15f);
        for (int i = 0; i < bathroomTaps.Length; i++)
        {
            if (bathroomTaps[i] != null)
            {
                bathroomTaps[i].ActivateByDifficulty();
                Debug.Log($"[Difficulté] Robinet {i + 1}/{bathroomTaps.Length} activé à {GetFormattedTime()}");
            }

            if (i < bathroomTaps.Length - 1)
            {
                yield return new WaitForSeconds(timeBetweenTapActivation);
            }
        }
    }

    public string GetFormattedTime()
    {
        int minutes = Mathf.FloorToInt(survivalTime / 60f);
        int seconds = Mathf.FloorToInt(survivalTime % 60f);
        return string.Format("{0:00}:{1:00}", minutes, seconds);
    }

    public void ResetDifficulty()
    {
        survivalTime = 0f;
        currentPhase = 0;
        ApplyPhase(1);
    }
}
