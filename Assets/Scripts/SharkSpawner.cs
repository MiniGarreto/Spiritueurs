using UnityEngine;

public class SharkSpawner : MonoBehaviour
{
    public ScaleOnMaxHeight waterRiseSystem;
    public Transform targetWater;
    public float maxWaterYScale = 2f;      
    public GameObject sharkPrefab;
    public Transform sharkSpawnPoint;
    public Transform playerTransform;
    public Transform[] waypoints; 
    public float spawnDelay = 1f;

    private bool sharkSpawned = false;
    private bool waterIsFull = false;
    private float fullWaterTimer = 0f;

    void Update()
    {
        if (sharkSpawned) return;

        if (targetWater != null)
        {
            if (targetWater.localScale.y >= maxWaterYScale - 0.01f)
            {
                waterIsFull = true;
            }
        }

        if (waterIsFull)
        {
            fullWaterTimer += Time.deltaTime;

            if (fullWaterTimer >= spawnDelay)
            {
                SpawnShark();
            }
        }
    }

    private void SpawnShark()
    {
        if (sharkPrefab == null || sharkSpawnPoint == null)
        {
            Debug.LogWarning("SharkSpawner: Prefab ou spawn point manquant!");
            return;
        }

        GameObject shark = Instantiate(sharkPrefab, sharkSpawnPoint.position, sharkSpawnPoint.rotation);

        SharkAI sharkAI = shark.GetComponent<SharkAI>();
        if (sharkAI != null)
        {
            sharkAI.waypoints = waypoints;
            sharkAI.Activate(playerTransform);
        }

        sharkSpawned = true;
    }

    public void ResetSpawner()
    {
        sharkSpawned = false;
        waterIsFull = false;
        fullWaterTimer = 0f;
    }
}
