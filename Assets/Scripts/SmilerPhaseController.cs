using UnityEngine;
using System.Collections;

public class SmilerPhaseController : MonoBehaviour, IFlashable
{
    public enum Phase { Particles = 0, Phase1 = 1, Phase2 = 2, Phase3 = 3 }

    [Header("Assign children (ou laissez OnValidate les trouver automatiquement)")]
    [SerializeField] private GameObject particleSystemRoot;
    [SerializeField] private GameObject phase1Plane;
    [SerializeField] private GameObject phase2Plane;
    [SerializeField] private GameObject phase3Plane;

    [Header("Timing")]
    [SerializeField] private float phaseDuration = 5f;
    [SerializeField] private bool loop = false;
    [SerializeField] private bool startOnAwake = true;

    [SerializeField] private Phase startPhase = Phase.Particles;


    [Header("Reaction au flash")]
    [Tooltip("Effet visuel instancié quand flashé (optionnel)")]
    [SerializeField] private GameObject flashDeathVFX; 

    [Header("Audio")]
    [Tooltip("Optional sound played when the object is destroyed by a flash")]
    [SerializeField] private AudioClip deathSfx;
    [Range(0f, 1f)]
    [SerializeField] private float deathSfxVolume = 1f;
    [Tooltip("Optional sound played when entering Phase 3")]
    [SerializeField] private AudioClip phase3Sfx;
    [Range(0f, 3f)]
    [SerializeField] private float phase3SfxVolume = 1f;

    [Header("Approach")]
    [Tooltip("Vitesse à laquelle le smiler se rapproche de la cible")]
    [SerializeField] private float approachSpeed = 2f;
    [Tooltip("Distance d'arrêt avant d'atteindre la cible")]
    [SerializeField] private float approachStopDistance = 0.5f;
    [Tooltip("Rotation speed when facing the target during Phase3")]
    [SerializeField] private float rotationSpeed = 5f;
    [Tooltip("Only rotate on Y axis (no pitch)")]
    [SerializeField] private bool rotateYOnly = true;
    [SerializeField] private Transform approachTarget;

    private Phase currentPhase;
    private Coroutine cycleCoroutine;

    private void Reset()
    {
        // appelé en éditor si vous choisissez "Reset" dans Inspector — tentative d'assign automatique
        particleSystemRoot = transform.Find("Particle System")?.gameObject;
        phase1Plane = transform.Find("Phase 1")?.gameObject;
        phase2Plane = transform.Find("Phase 2")?.gameObject;
        phase3Plane = transform.Find("Phase 3")?.gameObject;
    }

    private void OnValidate()
    {
        // picks children if left null, utile pour l'édition
        if (particleSystemRoot == null) particleSystemRoot = transform.Find("Particle System")?.gameObject;
        if (phase1Plane == null) phase1Plane = transform.Find("Phase 1")?.gameObject;
        if (phase2Plane == null) phase2Plane = transform.Find("Phase 2")?.gameObject;
        if (phase3Plane == null) phase3Plane = transform.Find("Phase 3")?.gameObject;

        // appliquer état en mode Éditeur pour visualisation
        if (!Application.isPlaying)
        {
            ApplyPhaseState(startPhase);
        }
    }

    private void Awake()
    {
        currentPhase = startPhase;
        ApplyPhaseState(currentPhase);

        if (startOnAwake)
            StartCycle();
    }

    public void StartCycle()
    {
        if (cycleCoroutine == null)
            cycleCoroutine = StartCoroutine(CyclePhases());
    }

    public void StopCycle()
    {
        if (cycleCoroutine != null)
        {
            StopCoroutine(cycleCoroutine);
            cycleCoroutine = null;
        }
    }

    public void AdvancePhase()
    {
        currentPhase = (Phase)Mathf.Min((int)currentPhase + 1, (int)Phase.Phase3);
        ApplyPhaseState(currentPhase);
        
        if (currentPhase == Phase.Phase3 && phase3Sfx != null)
            AudioSource.PlayClipAtPoint(phase3Sfx, transform.position, phase3SfxVolume);
    }

    private IEnumerator CyclePhases()
    {
        while (true)
        {
            yield return new WaitForSeconds(phaseDuration);
            AdvancePhase();

            if (!loop && currentPhase == Phase.Phase3)
            {
                StopCycle();
                yield break;
            }
        }
    }

    private void ApplyPhaseState(Phase p)
    {
        // Particules : laisser active (changez si vous voulez qu'elles s'éteignent)
        if (particleSystemRoot != null)
            particleSystemRoot.SetActive(true);

        if (phase1Plane != null)
            phase1Plane.SetActive(p == Phase.Phase1);

        if (phase2Plane != null)
            phase2Plane.SetActive(p == Phase.Phase2);

        if (phase3Plane != null)
            phase3Plane.SetActive(p == Phase.Phase3);
    }

    // Méthodes utilitaires publiques

    private Transform FindPlayerTransform()
    {
        if (Camera.main != null) return Camera.main.transform;
        var go = GameObject.FindWithTag("Player");
        if (go != null) return go.transform;
        return null;
    }

    private void Update()
    {
        if (currentPhase != Phase.Phase3) return;
        if (approachTarget == null) return;

        Vector3 current = transform.position;
        Vector3 targetPos = approachTarget.position;

        // rotate to face the target
        Vector3 lookDir = targetPos - current;
        if (rotateYOnly) lookDir.y = 0f;
        if (lookDir.sqrMagnitude > 0.0001f)
        {
            Quaternion desiredRot = Quaternion.LookRotation(lookDir.normalized);
            transform.rotation = Quaternion.Slerp(transform.rotation, desiredRot, rotationSpeed * Time.deltaTime);
        }

        Vector3 desired = targetPos; // follow full 3D position (including Y)
        float dist = Vector3.Distance(current, desired);
        if (dist > approachStopDistance)
        {
            transform.position = Vector3.MoveTowards(current, desired, approachSpeed * Time.deltaTime);
        }
        else
        {
            GameOverManager.Instance.TriggerSmilerGameOver();
        }
    }

    // IFlashable implementation
    public void OnFlashed()
    {
        if (flashDeathVFX != null)
            Instantiate(flashDeathVFX, transform.position, Quaternion.identity);

        if (deathSfx != null)
            AudioSource.PlayClipAtPoint(deathSfx, transform.position, deathSfxVolume);

        Destroy(gameObject);
    }

    public void ResetToPhase0()
    {
        currentPhase = Phase.Particles;
        ApplyPhaseState(currentPhase);
        
        if (phase3Sfx != null)
            AudioSource.PlayClipAtPoint(phase3Sfx, transform.position, phase3SfxVolume);
    }
}