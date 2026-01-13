using UnityEngine;
using System.Collections;

public class SmilerPhaseController : MonoBehaviour, IFlashable
{
    public enum Phase { Particles = 0, Phase1 = 1, Phase2 = 2, Phase3 = 3 }

    [SerializeField] private GameObject particleSystemRoot;
    [SerializeField] private GameObject phase1Plane;
    [SerializeField] private GameObject phase2Plane;
    [SerializeField] private GameObject phase3Plane;
    [SerializeField] private float phaseDuration = 5f;
    [SerializeField] private bool loop = false;
    [SerializeField] private bool startOnAwake = true;
    [SerializeField] private Phase startPhase = Phase.Particles;
    [SerializeField] private GameObject flashDeathVFX; 
    [SerializeField] private AudioClip deathSfx;
    [Range(0f, 1f)]
    [SerializeField] private float deathSfxVolume = 1f;
    [SerializeField] private AudioClip phase3Sfx;
    [Range(0f, 3f)]
    [SerializeField] private float phase3SfxVolume = 1f;
    [SerializeField] private float approachSpeed = 2f;
    [SerializeField] private float approachStopDistance = 0.5f;
    [SerializeField] private float rotationSpeed = 5f;
    [SerializeField] private bool rotateYOnly = true;
    [SerializeField] private Transform approachTarget;

    private Phase currentPhase;
    private Coroutine cycleCoroutine;

    private void Reset()
    {
        particleSystemRoot = transform.Find("Particle System")?.gameObject;
        phase1Plane = transform.Find("Phase 1")?.gameObject;
        phase2Plane = transform.Find("Phase 2")?.gameObject;
        phase3Plane = transform.Find("Phase 3")?.gameObject;
    }

    private void OnValidate()
    {
        if (particleSystemRoot == null) particleSystemRoot = transform.Find("Particle System")?.gameObject;
        if (phase1Plane == null) phase1Plane = transform.Find("Phase 1")?.gameObject;
        if (phase2Plane == null) phase2Plane = transform.Find("Phase 2")?.gameObject;
        if (phase3Plane == null) phase3Plane = transform.Find("Phase 3")?.gameObject;

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
        if (particleSystemRoot != null)
            particleSystemRoot.SetActive(true);

        if (phase1Plane != null)
            phase1Plane.SetActive(p == Phase.Phase1);

        if (phase2Plane != null)
            phase2Plane.SetActive(p == Phase.Phase2);

        if (phase3Plane != null)
            phase3Plane.SetActive(p == Phase.Phase3);
    }

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

        Vector3 lookDir = targetPos - current;
        if (rotateYOnly) lookDir.y = 0f;
        if (lookDir.sqrMagnitude > 0.0001f)
        {
            Quaternion desiredRot = Quaternion.LookRotation(lookDir.normalized);
            transform.rotation = Quaternion.Slerp(transform.rotation, desiredRot, rotationSpeed * Time.deltaTime);
        }

        Vector3 desired = targetPos;
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