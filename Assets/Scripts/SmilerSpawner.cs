using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SmilerSpawner : MonoBehaviour
{
    public GameObject smilerPrefab;
    public float spawnInterval = 5f;
    public bool startOnAwake = false;
    public bool useChildrenAsSpawnPoints = true;
    public Transform[] spawnPoints;

    public float occupancyRadius = 0.5f;
    public LayerMask smilerMask;

    private List<Transform> points = new List<Transform>();
    private Dictionary<Transform, GameObject> lastSpawned = new Dictionary<Transform, GameObject>();
    private Coroutine spawnCoroutine;

    private void Awake()
    {
        RefreshSpawnPoints();
        if (startOnAwake) StartSpawning();
    }

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
            return;
        }

        var pick = free[Random.Range(0, free.Count)];
        var go = Instantiate(smilerPrefab, pick.position, pick.rotation);
        lastSpawned[pick] = go;
    }

    private bool IsOccupied(Transform p)
    {
        if (lastSpawned.TryGetValue(p, out var go))
        {
            if (go == null)
            {
                lastSpawned.Remove(p);
                return false;
            }

            if ((go.transform.position - p.position).sqrMagnitude <= occupancyRadius * occupancyRadius)
                return true;

            lastSpawned.Remove(p);
            return false;
        }

        if (smilerMask != 0)
        {
            var cols = Physics.OverlapSphere(p.position, occupancyRadius, smilerMask);
            if (cols.Length > 0) return true;
        }
        else
        {
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