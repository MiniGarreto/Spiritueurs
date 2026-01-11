using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Spawns one instance of a prefab every `spawnInterval` seconds at one of the child transforms (or provided transforms).
// Avoids spawning at a point that already has a live Smiler (by tracking last spawn or checking physics overlap).
public class SmilerSpawner : MonoBehaviour
{
    [Header("Spawn Settings")]
    public GameObject smilerPrefab;
    public float spawnInterval = 5f;
    [Tooltip("If false, spawning must be started via StartSpawning() or by DifficultyManager")]
    public bool startOnAwake = false;  // Changé à false pour laisser le DifficultyManager contrôler
    [Tooltip("If true, the spawner will use the GameObject's children as spawn points.")]
    public bool useChildrenAsSpawnPoints = true;
    [Tooltip("Optional explicit spawn points (used when useChildrenAsSpawnPoints is false)")]
    public Transform[] spawnPoints;

    [Header("Occupation")]
    [Tooltip("Radius used to consider a spawn point occupied")]
    public float occupancyRadius = 0.5f;
    [Tooltip("Layer mask to detect existing smilers (optional). If left empty the script will fall back to checking for IFlashable in radius")]
    public LayerMask smilerMask;

    private List<Transform> points = new List<Transform>();
    private Dictionary<Transform, GameObject> lastSpawned = new Dictionary<Transform, GameObject>();
    private Coroutine spawnCoroutine;

    private void Awake()
    {
        RefreshSpawnPoints();
        if (startOnAwake) StartSpawning();
    }

    /// <summary>
    /// Refresh the internal spawn points list from children or explicit array
    /// </summary>
    public void RefreshSpawnPoints()
    {
        points.Clear();
        if (useChildrenAsSpawnPoints)
        {
            for (int i = 0; i < transform.childCount; i++)
                points.Add(transform.GetChild(i));
        }

        if (!useChildrenAsSpawnPoints && spawnPoints != null && spawnPoints.Length > 0)
        {
            points.AddRange(spawnPoints);
        }
    }

    public void StartSpawning()
    {
        if (spawnCoroutine == null)
            spawnCoroutine = StartCoroutine(SpawnLoop());
    }

    public void StopSpawning()
    {
        if (spawnCoroutine != null)
        {
            StopCoroutine(spawnCoroutine);
            spawnCoroutine = null;
        }
    }

    private IEnumerator SpawnLoop()
    {
        while (true)
        {
            yield return new WaitForSeconds(spawnInterval);
            SpawnOne();
        }
    }

    /// <summary>
    /// Spawns one smiler at a random free spawn point. No-op if no free point exists or prefab not assigned.
    /// </summary>
    [ContextMenu("Spawn One")]
    public void SpawnOne()
    {
        RefreshSpawnPoints();

        if (smilerPrefab == null)
        {
            Debug.LogWarning("SmilerSpawner: no prefab assigned", this);
            return;
        }

        var free = new List<Transform>();
        foreach (var p in points)
        {
            if (!IsOccupied(p)) free.Add(p);
        }

        if (free.Count == 0)
        {
            // nothing free right now
            return;
        }

        var pick = free[Random.Range(0, free.Count)];
        var go = Instantiate(smilerPrefab, pick.position, pick.rotation);
        lastSpawned[pick] = go;
    }

    /// <summary>
    /// Returns true if a live smiler is occupying the spawn point.
    /// Uses cached last spawned reference first, then physics overlap check as fallback.
    /// </summary>
    private bool IsOccupied(Transform p)
    {
        if (lastSpawned.TryGetValue(p, out var go))
        {
            if (go == null)
            {
                lastSpawned.Remove(p);
                return false;
            }

            // if the spawned object is within occupancy radius consider it occupying
            if ((go.transform.position - p.position).sqrMagnitude <= occupancyRadius * occupancyRadius)
                return true;

            // it moved away: free the point
            lastSpawned.Remove(p);
            return false;
        }

        // If we have a mask, use it
        if (smilerMask != 0)
        {
            var cols = Physics.OverlapSphere(p.position, occupancyRadius, smilerMask);
            if (cols.Length > 0) return true;
        }
        else
        {
            // fallback: look for an IFlashable in radius
            var cols = Physics.OverlapSphere(p.position, occupancyRadius);
            foreach (var c in cols)
            {
                if (c.GetComponentInParent<IFlashable>() != null) return true;
            }
        }

        return false;
    }

    private void OnDrawGizmosSelected()
    {
        RefreshSpawnPoints();
        Gizmos.color = Color.cyan;
        foreach (var p in points)
        {
            if (p == null) continue;
            Gizmos.DrawWireSphere(p.position, occupancyRadius);
            Gizmos.DrawLine(p.position, p.position + p.forward * 0.25f);
        }
    }
}